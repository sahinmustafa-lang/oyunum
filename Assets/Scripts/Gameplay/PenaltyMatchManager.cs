using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PenaltyMatchManager : MonoBehaviour
{
    public static PenaltyMatchManager Instance { get; private set; }

    [Header("References")]
    public ShooterController shooterController;
    public GoalkeeperController goalkeeperController;
    public BallController ballController;
    public MatchHUD matchHUD;

    public TeamData PlayerTeam { get; private set; }
    public TeamData OpponentTeam { get; private set; }

    public int PlayerGoals { get; private set; }
    public int OpponentGoals { get; private set; }
    public int PlayerShotsTaken { get; private set; }
    public int OpponentShotsTaken { get; private set; }
    public bool IsSuddenDeath { get; private set; }
    public MatchPhase Phase { get; private set; }
    public MatchSide CurrentSide { get; private set; }

    private List<ShotOutcome> playerHistory = new List<ShotOutcome>();
    private List<ShotOutcome> opponentHistory = new List<ShotOutcome>();
    private int roundDifficulty = 1;

    // Sudden death: player shoots first each round, then opponent (same as normal order)
    private bool sdPlayerShot   = false;
    private bool sdOpponentShot = false;

    const int SHOTS_PER_TEAM = 5;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find references if not assigned in Inspector
        if (shooterController == null)    shooterController    = FindFirstObjectByType<ShooterController>();
        if (goalkeeperController == null) goalkeeperController = FindFirstObjectByType<GoalkeeperController>();
        if (ballController == null)       ballController       = FindFirstObjectByType<BallController>();
        if (matchHUD == null)             matchHUD             = FindFirstObjectByType<MatchHUD>();

        PlayerTeam   = GameManager.Instance.SelectedTeam;
        OpponentTeam = GameManager.Instance.CurrentTournament.currentOpponent;

        TournamentRound round = GameManager.Instance.CurrentTournament.currentRound;
        roundDifficulty = round == TournamentRound.QuarterFinal ? 1 :
                          round == TournamentRound.SemiFinal ? 2 : 3;

        if (matchHUD != null)
            matchHUD.Setup(PlayerTeam, OpponentTeam, TournamentManager.Instance.GetCurrentRoundName());

        StartMatch();
    }

    public void StartMatch()
    {
        PlayerGoals = 0;
        OpponentGoals = 0;
        PlayerShotsTaken = 0;
        OpponentShotsTaken = 0;
        IsSuddenDeath = false;
        sdOpponentShot = false;
        sdPlayerShot   = false;
        playerHistory.Clear();
        opponentHistory.Clear();

        AudioManager.Instance.PlayWhistle();
        StartCoroutine(NextTurnRoutine());
    }

    IEnumerator NextTurnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (CheckMatchEnd()) yield break;

        if (IsSuddenDeath)
        {
            // Sudden death: Player first, then Opponent — same alternation as normal play
            if (!sdPlayerShot)
                StartPlayerShot();
            else
                StartOpponentShot();
        }
        else
        {
            // Normal: player shoots first in every pair
            if (PlayerShotsTaken <= OpponentShotsTaken)
                StartPlayerShot();
            else
                StartOpponentShot();
        }
    }

    void StartPlayerShot()
    {
        CurrentSide = MatchSide.Player;
        Phase = MatchPhase.PlayerShooting;
        matchHUD.ShowInstruction("YOUR SHOT - Tap to start run-up");
        shooterController.PreparePlayerShot();
        goalkeeperController.SetAsOpponentKeeper();
    }

    void StartOpponentShot()
    {
        CurrentSide = MatchSide.Opponent;
        Phase = MatchPhase.OpponentShooting;
        matchHUD.ShowInstruction("SAVE IT! Tap to dive");
        goalkeeperController.PreparePlayerKeep();
        shooterController.PrepareOpponentShot();

        // AI shoots after short delay
        StartCoroutine(OpponentShootRoutine());
    }

    IEnumerator OpponentShootRoutine()
    {
        PenaltyShotData aiShot = OpponentAI.GenerateShot(OpponentTeam, roundDifficulty);

        // Run-up starts
        yield return new WaitForSeconds(0.8f);

        // Show red dot at target zone briefly
        if (GoalIndicator.Instance != null)
        {
            matchHUD.ShowInstruction("SAVE IT! Tap to dive!");
            yield return StartCoroutine(GoalIndicator.Instance.ShowTargetDotRoutine(aiShot.targetZone, 0.6f));
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        // Small gap after dot disappears
        yield return new WaitForSeconds(0.3f);

        shooterController.ExecuteOpponentShot(aiShot);
    }

    // Called after player's FIRST tap — keeper dives to fake zone immediately
    public void OnPlayerFakeTap(ShotZone fakeZone)
    {
        goalkeeperController.DiveTo(fakeZone);
    }

    public void OnPlayerShotInput(PenaltyShotData shot)
    {
        if (Phase != MatchPhase.PlayerShooting) return;
        Phase = MatchPhase.Resolving;

        // AI decides whether to follow the fake or read the real shot
        ShotZone aiKeeperZone = OpponentAI.ChooseKeeperZone(shot.fakeZone, OpponentTeam.aiStrength);

        // If AI reads the real shot, keeper makes a last-second lunge
        if (aiKeeperZone != shot.fakeZone)
            goalkeeperController.QuickDiveTo(aiKeeperZone);

        ShotOutcome outcome = ShotResolver.Resolve(shot, aiKeeperZone, PlayerTeam.shooterStrength, OpponentTeam.keeperStrength);
        StartCoroutine(ResolveRoutine(outcome, MatchSide.Player, shot.targetZone, shot.wideMissSide));
    }

    public void OnPlayerKeeperInput(ShotZone keeperZone)
    {
        if (Phase != MatchPhase.OpponentShooting) return;
        goalkeeperController.SetPlayerKeeperZone(keeperZone);
        // Keeper visual dive is already done in GoalkeeperController.Update()
    }

    public void OnOpponentShotResolved(PenaltyShotData shot, ShotZone playerKeeperZone)
    {
        if (Phase != MatchPhase.OpponentShooting) return;
        Phase = MatchPhase.Resolving;

        ShotOutcome outcome = ShotResolver.Resolve(shot, playerKeeperZone, OpponentTeam.shooterStrength, PlayerTeam.keeperStrength);
        StartCoroutine(ResolveRoutine(outcome, MatchSide.Opponent, shot.targetZone));
    }

    IEnumerator ResolveRoutine(ShotOutcome outcome, MatchSide side, ShotZone targetZone, int wideMissSide = 0)
    {
        // Top neredeyse anında hareket eder — kaleci ile eş zamanlı uçar
        yield return new WaitForSeconds(0.05f);

        ballController.AnimateBall(outcome, side == MatchSide.Player, targetZone, wideMissSide);

        if (outcome == ShotOutcome.Goal)
        {
            GoalNet.Instance?.TriggerBallEntered();
            // Gol atan oyuncunun kutlama animasyonu
            if (side == MatchSide.Player)
            {
                var sv = FindFirstObjectByType<ShooterVisual>();
                sv?.PlayCelebrate();
            }
        }

        yield return new WaitForSeconds(1.2f);

        // Kaleci topun nereye gittiğini izledikten sonra yavaşça kalkıp ortalanır
        goalkeeperController.ReturnToIdle(0.3f);

        ApplyOutcome(side, outcome);
        Phase = MatchPhase.ShowingResult;
        matchHUD.ShowOutcome(outcome);
        matchHUD.UpdateScores(PlayerGoals, OpponentGoals, playerHistory, opponentHistory);

        PlayOutcomeSound(outcome);

        yield return new WaitForSeconds(1.5f);

        matchHUD.HideOutcome();
        StartCoroutine(NextTurnRoutine());
    }

    void ApplyOutcome(MatchSide side, ShotOutcome outcome)
    {
        bool scored = outcome == ShotOutcome.Goal;

        if (IsSuddenDeath)
        {
            if (side == MatchSide.Opponent) sdOpponentShot = true;
            else                            sdPlayerShot   = true;
        }

        if (side == MatchSide.Player)
        {
            PlayerShotsTaken++;
            playerHistory.Add(outcome);
            if (scored) PlayerGoals++;
        }
        else
        {
            OpponentShotsTaken++;
            opponentHistory.Add(outcome);
            if (scored) OpponentGoals++;
        }
    }

    bool CheckMatchEnd()
    {
        // ── Sudden death ──────────────────────────────────────────────────
        if (IsSuddenDeath)
        {
            // Wait until both have shot this round
            if (!sdPlayerShot || !sdOpponentShot) return false;

            if (PlayerGoals != OpponentGoals)
            {
                EndMatch(PlayerGoals > OpponentGoals);
                return true;
            }

            // Both scored or both missed → next SD round
            sdPlayerShot   = false;
            sdOpponentShot = false;
            matchHUD.ShowInstruction("SUDDEN DEATH CONTINUES!");
            return false;
        }

        // ── Normal 5 kicks — mathematical elimination ─────────────────────
        // How many kicks each side still has left
        int playerLeft   = Mathf.Max(0, SHOTS_PER_TEAM - PlayerShotsTaken);
        int opponentLeft = Mathf.Max(0, SHOTS_PER_TEAM - OpponentShotsTaken);

        // Player wins: even if opponent scores every remaining kick, can't catch up
        if (PlayerGoals > OpponentGoals + opponentLeft)
        {
            EndMatch(true);
            return true;
        }

        // Opponent wins: even if player scores every remaining kick, can't catch up
        if (OpponentGoals > PlayerGoals + playerLeft)
        {
            EndMatch(false);
            return true;
        }

        // Both sides finished all 5 kicks
        if (playerLeft == 0 && opponentLeft == 0)
        {
            if (PlayerGoals != OpponentGoals)
            {
                EndMatch(PlayerGoals > OpponentGoals);
                return true;
            }
            // Tied → enter sudden death
            IsSuddenDeath  = true;
            sdPlayerShot   = false;
            sdOpponentShot = false;
            matchHUD.ShowInstruction("SUDDEN DEATH!");
            return false;
        }

        return false;
    }

    void EndMatch(bool playerWon)
    {
        Phase = MatchPhase.MatchEnded;
        GameManager.Instance.CompleteMatch(playerWon, PlayerGoals, OpponentGoals);
        TournamentManager.Instance.CompletePlayerMatch(playerWon, PlayerGoals, OpponentGoals);

        StartCoroutine(EndMatchRoutine(playerWon));
    }

    IEnumerator EndMatchRoutine(bool playerWon)
    {
        yield return new WaitForSeconds(1.5f);

        if (playerWon && TournamentManager.Instance.IsTournamentWon())
            SceneLoader.Instance.LoadChampion();
        else if (playerWon)
        {
            TournamentManager.Instance.AdvanceRound();
            SceneLoader.Instance.LoadResult();
        }
        else
            SceneLoader.Instance.LoadGameOver();
    }

    void PlayOutcomeSound(ShotOutcome outcome)
    {
        switch (outcome)
        {
            case ShotOutcome.Goal: AudioManager.Instance.PlayGoal(); AudioManager.Instance.PlayCrowd(); break;
            case ShotOutcome.Save: AudioManager.Instance.PlaySave(); break;
            case ShotOutcome.Miss: AudioManager.Instance.PlayMiss(); break;
            case ShotOutcome.Post: AudioManager.Instance.PlayPost(); break;
        }
    }
}
