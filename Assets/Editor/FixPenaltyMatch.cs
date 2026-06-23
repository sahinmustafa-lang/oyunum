using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixPenaltyMatch : EditorWindow
{
    [MenuItem("PenaltyGame/Fix PenaltyMatch Scene")]
    public static void Fix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PenaltyMatch.unity", OpenSceneMode.Single);

        // ── Remove the old standalone GoalkeeperController GO if it exists ──
        // (GoalkeeperController now lives ON the "Goalkeeper" visual GO)
        var oldKeeperCtrl = GameObject.Find("GoalkeeperController");
        if (oldKeeperCtrl != null)
            DestroyImmediate(oldKeeperCtrl);

        // ── Visual GameObjects ──────────────────────────────────────────────
        EnsureGO("FieldBackground");
        EnsureGO("Goal");
        EnsureGO("GoalPostLeft");
        EnsureGO("GoalPostRight");
        EnsureGO("GoalCrossbar");
        EnsureGO("Shooter");
        EnsureGO("Ball");

        // GoalkeeperController lives ON "Goalkeeper" — script + visual together
        var keeperGO = EnsureGO("Goalkeeper");
        if (keeperGO.GetComponent<GoalkeeperController>() == null)
            keeperGO.AddComponent<GoalkeeperController>();

        // ── Gameplay anchors ────────────────────────────────────────────────
        EnsureGO("PenaltySpot");
        EnsureGO("RunUpPosition");
        EnsureGO("GoalCenter");
        EnsureGO("MissTarget");

        // ── Manager script hosts ────────────────────────────────────────────
        EnsureScript<PenaltyMatchManager>("PenaltyMatchManager");
        EnsureScript<ShooterController>("ShooterController");
        EnsureScript<BallController>("BallController");
        EnsureScript<MatchHUD>("MatchHUD");
        EnsureScript<PenaltyMatchVisuals>("PenaltyMatchVisuals");
        EnsureScript<GoalIndicator>("GoalIndicator");
        EnsureScript<DirectionArrow>("DirectionArrow");
        EnsureScript<AimingCursor>("AimingCursor");

        // ── Wire references ─────────────────────────────────────────────────
        var pmm     = Object.FindFirstObjectByType<PenaltyMatchManager>();
        var shooter = Object.FindFirstObjectByType<ShooterController>();
        var keeper  = Object.FindFirstObjectByType<GoalkeeperController>();
        var ball    = Object.FindFirstObjectByType<BallController>();
        var hud     = Object.FindFirstObjectByType<MatchHUD>();

        if (pmm != null)
        {
            pmm.shooterController    = shooter;
            pmm.goalkeeperController = keeper;
            pmm.ballController       = ball;
            pmm.matchHUD             = hud;
            EditorUtility.SetDirty(pmm);
        }

        if (shooter != null)
        {
            shooter.shooterTransform    = FindT("Shooter");
            shooter.ballTransform       = FindT("Ball");
            shooter.penaltySpotPosition = FindT("PenaltySpot");
            shooter.runUpPosition       = FindT("RunUpPosition");
            EditorUtility.SetDirty(shooter);
        }

        // keeper uses this.transform — no extra fields to assign
        if (keeper != null) EditorUtility.SetDirty(keeper);

        if (ball != null)
        {
            ball.ballTransform = FindT("Ball");
            ball.penaltySpot   = FindT("PenaltySpot");
            ball.goalCenter    = FindT("GoalCenter");
            ball.missTarget    = FindT("MissTarget");
            EditorUtility.SetDirty(ball);
        }

        if (hud != null)
        {
            hud.penaltyIconPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/PenaltyIcon.prefab");
            EditorUtility.SetDirty(hud);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("PenaltyMatch scene fixed!");
        EditorUtility.DisplayDialog("Done!", "PenaltyMatch scene fixed!", "OK");
    }

    static GameObject EnsureGO(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        return go;
    }

    static void EnsureScript<T>(string goName) where T : MonoBehaviour
    {
        var go = GameObject.Find(goName);
        if (go == null) go = new GameObject(goName);
        if (go.GetComponent<T>() == null) go.AddComponent<T>();
    }

    static Transform FindT(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }
}
