using UnityEngine;
using System.Collections;

// Shooting mechanic:
//   Tap 1  → fake direction (arrow position) → keeper dives there → run-up starts
//   During run-up (~0.87s window): press & HOLD for 2nd input
//     PRESS locks direction (arrow position at that moment)
//     RELEASE locks height:
//       Release very quickly (<0.07s)  → Low  (çok zor)
//       Release medium (0.07-0.18s)    → Mid  (zor)
//       Release long   (0.18-0.38s)    → High (orta)
//       Still holding / >0.38s         → Over bar (kolay yapılır, miss)
//   No 2nd press before run-up ends → auto-fires in fake direction
public class ShooterController : MonoBehaviour
{
    [Header("Positions (auto-found at runtime)")]
    public Transform shooterTransform;
    public Transform ballTransform;
    public Transform penaltySpotPosition;
    public Transform runUpPosition;

    private bool          isPlayerShooter    = false;
    private bool          waitingForFirstTap = false;
    private bool          inRunUpWindow      = false;
    private bool          secondButtonHeld   = false;
    private float         secondPressTime    = -1f;
    private ShotZone      fakeZone;
    private ShotZone      lockedDirZone;   // direction locked at 2nd-press moment
    private ShooterVisual visual;

    const float RUN_UP_DURATION = 0.87f;  // %13 kısaltıldı

    // Hold → height thresholds (seconds) — sıkılaştırıldı
    const float LOW_THRESHOLD  = 0.07f;  // < 0.07s  → Low  (çok kısa pencere)
    const float MID_THRESHOLD  = 0.18f;  // < 0.18s  → Mid
    const float HIGH_THRESHOLD = 0.38f;  // < 0.38s  → High (üstü over bar)

    static readonly Vector3 SHOOTER_START = new Vector3(-0.4f, -2.4f,  0f);
    static readonly Vector3 PENALTY_SPOT  = new Vector3( 0.0f, -1.45f, 0f);

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Start()
    {
        if (shooterTransform    == null) shooterTransform    = FindT("Shooter");
        if (ballTransform       == null) ballTransform       = FindT("Ball");
        if (penaltySpotPosition == null) penaltySpotPosition = FindT("PenaltySpot");
        if (runUpPosition       == null) runUpPosition       = FindT("RunUpPosition");

        if (shooterTransform != null)
            visual = shooterTransform.GetComponent<ShooterVisual>();
    }

    static Transform FindT(string n)
    {
        var g = GameObject.Find(n);
        return g != null ? g.transform : null;
    }

    // ── State setters ──────────────────────────────────────────────────────

    public void PreparePlayerShot()
    {
        isPlayerShooter    = true;
        waitingForFirstTap = true;
        inRunUpWindow      = false;
        secondButtonHeld   = false;
        secondPressTime    = -1f;
        ResetShooter();

        if (DirectionArrow.Instance != null)
            DirectionArrow.Instance.StartArrow();
    }

    public void PrepareOpponentShot()
    {
        isPlayerShooter    = false;
        waitingForFirstTap = false;
        inRunUpWindow      = false;
        ResetShooter();

        if (DirectionArrow.Instance != null)
            DirectionArrow.Instance.HideAll();
    }

    // ── Input ──────────────────────────────────────────────────────────────

    void Update()
    {
        if (!isPlayerShooter) return;

        // First tap → lock fake direction + start run-up
        if (waitingForFirstTap && Input.GetMouseButtonDown(0))
        {
            HandleFirstTap();
            return;
        }

        if (inRunUpWindow)
        {
            // PRESS → direction locks NOW, start timing hold
            if (!secondButtonHeld && Input.GetMouseButtonDown(0))
            {
                secondButtonHeld = true;
                secondPressTime  = Time.time;

                // Direction is wherever the live arrow points at THIS moment
                lockedDirZone = DirectionArrow.Instance != null
                    ? DirectionArrow.Instance.OnSecondTap()
                    : fakeZone;
            }

            // RELEASE → height determined by hold duration → fire
            if (secondButtonHeld && Input.GetMouseButtonUp(0))
            {
                float hold    = Time.time - secondPressTime;
                inRunUpWindow = false;
                secondButtonHeld = false;
                FirePlayerShot(hold);
            }
        }
    }

    // ── First tap ──────────────────────────────────────────────────────────

    void HandleFirstTap()
    {
        waitingForFirstTap = false;
        secondButtonHeld   = false;
        secondPressTime    = -1f;

        fakeZone = DirectionArrow.Instance != null
            ? DirectionArrow.Instance.OnFirstTap()
            : GoalkeeperController.ScreenToZone(Input.mousePosition);

        PenaltyMatchManager.Instance.OnPlayerFakeTap(fakeZone);
        StartCoroutine(RunUpRoutine());
    }

    IEnumerator RunUpRoutine()
    {
        inRunUpWindow = true;

        var hud = PenaltyMatchManager.Instance != null ? PenaltyMatchManager.Instance.matchHUD : null;
        if (hud != null)
            hud.ShowInstruction("PRESS & HOLD → release for height!");

        StartCoroutine(AnimateRunUp());

        float elapsed = 0f;
        while (elapsed < RUN_UP_DURATION)
        {
            if (!inRunUpWindow) yield break; // Update already fired the shot

            elapsed += Time.deltaTime;
            yield return null;
        }

        inRunUpWindow = false;

        if (secondButtonHeld)
        {
            // Button still held past time limit → over bar
            secondButtonHeld = false;
            FirePlayerShot(1.0f); // timingQuality will be 0 → guaranteed Miss
        }
        else
        {
            // Player never pressed second button → shoots in fake direction
            FireAutoFake();
        }
    }

    // ── Shot execution ─────────────────────────────────────────────────────

    void FirePlayerShot(float holdDuration)
    {
        if (GoalIndicator.Instance != null) GoalIndicator.Instance.HideZoneHints();

        int col = ZoneToColumn(lockedDirZone);
        int row = HoldToRow(holdDuration);
        ShotZone finalZone = (ShotZone)(col * 3 + row);

        bool overBar = holdDuration >= HIGH_THRESHOLD;
        float timingQ;
        if (overBar)
            timingQ = 0f;
        else if (holdDuration < LOW_THRESHOLD)
            timingQ = Mathf.Lerp(0.95f, 0.75f, holdDuration / LOW_THRESHOLD);
        else if (holdDuration < MID_THRESHOLD)
            timingQ = Mathf.Lerp(0.70f, 0.35f,
                (holdDuration - LOW_THRESHOLD) / (MID_THRESHOLD - LOW_THRESHOLD));
        else
            timingQ = Mathf.Lerp(0.30f, 0.10f,
                (holdDuration - MID_THRESHOLD) / (HIGH_THRESHOLD - MID_THRESHOLD));

        // Yan kaçma: ok kenara çok yakınsa top direğin dışından gidebilir
        float arrowX = DirectionArrow.Instance != null
            ? DirectionArrow.Instance.GetLastArrowX() : 0f;
        int wideMissSide = 0;
        const float WIDE_START = 1.1f;  // bu noktadan itibaren şans başlar (amplitude=1.6)
        float edgeFactor = Mathf.Max(0f, (Mathf.Abs(arrowX) - WIDE_START) / (1.6f - WIDE_START));
        float wideMissChance = edgeFactor * 0.50f;  // kenarda max %50
        if (!overBar && wideMissChance > 0f && UnityEngine.Random.value < wideMissChance)
            wideMissSide = arrowX < 0 ? -1 : 1;

        PenaltyShotData shot = new PenaltyShotData
        {
            targetZone    = finalZone,
            fakeZone      = fakeZone,
            timingQuality = timingQ,
            power         = overBar ? 1.0f : Mathf.Lerp(0.55f, 0.95f, holdDuration / HIGH_THRESHOLD),
            accuracy      = overBar ? 0f   : timingQ,
            isPlayerShot  = true,
            shootingTeam  = GameManager.Instance.SelectedTeam,
            wideMissSide  = wideMissSide
        };

        visual?.PlayKick();
        AudioManager.Instance.PlayKick();
        PenaltyMatchManager.Instance.OnPlayerShotInput(shot);
    }

    void FireAutoFake()
    {
        // Ran out of time → shoots in fake direction (keeper already dived there)
        if (GoalIndicator.Instance != null) GoalIndicator.Instance.HideZoneHints();
        if (DirectionArrow.Instance != null) DirectionArrow.Instance.HideAll();

        PenaltyShotData shot = new PenaltyShotData
        {
            targetZone    = fakeZone,
            fakeZone      = fakeZone,
            timingQuality = 0.1f,
            power         = 0.4f,
            accuracy      = 0.2f,
            isPlayerShot  = true,
            shootingTeam  = GameManager.Instance.SelectedTeam
        };

        AudioManager.Instance.PlayKick();
        PenaltyMatchManager.Instance.OnPlayerShotInput(shot);
    }

    // ── Opponent shot ──────────────────────────────────────────────────────

    public void ExecuteOpponentShot(PenaltyShotData shot)
    {
        StartCoroutine(OpponentShotRoutine(shot));
    }

    IEnumerator OpponentShotRoutine(PenaltyShotData shot)
    {
        yield return StartCoroutine(AnimateRunUp());
        visual?.PlayKick();
        AudioManager.Instance.PlayKick();

        // Kaleci topu gördükten sonra tepki veriyor — top ve dalış neredeyse eş zamanlı
        yield return new WaitForSeconds(0.22f);

        GoalkeeperController.Instance.CommitDive();
        ShotZone keeperZone = GoalkeeperController.Instance.GetPlayerKeeperZone();
        PenaltyMatchManager.Instance.OnOpponentShotResolved(shot, keeperZone);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    IEnumerator AnimateRunUp()
    {
        if (shooterTransform == null) yield break;

        Vector3 start = runUpPosition       != null ? runUpPosition.position       : SHOOTER_START;
        Vector3 end   = penaltySpotPosition != null ? penaltySpotPosition.position : PENALTY_SPOT;

        visual?.StartRunning();

        float elapsed = 0f;
        float animDur = 0.43f;  // %14 daha hızlı koşu
        while (elapsed < animDur)
        {
            elapsed += Time.deltaTime;
            shooterTransform.position = Vector3.Lerp(start, end, elapsed / animDur);
            yield return null;
        }
        shooterTransform.position = end;
        visual?.StopRunning();
    }

    void ResetShooter()
    {
        if (shooterTransform == null) return;
        shooterTransform.position = runUpPosition != null ? runUpPosition.position : SHOOTER_START;
        visual?.StopRunning();
    }

    // Column from zone: Left=0, Center=1, Right=2
    static int ZoneToColumn(ShotZone z)
    {
        int i = (int)z;
        if (i <= 2) return 0;
        if (i <= 5) return 1;
        return 2;
    }

    // Row from hold duration: Low=0, Mid=1, High=2
    static int HoldToRow(float hold)
    {
        if (hold < LOW_THRESHOLD) return 0;
        if (hold < MID_THRESHOLD) return 1;
        return 2;
    }
}
