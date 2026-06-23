using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ProjectSetup : EditorWindow
{
    [MenuItem("PenaltyGame/Setup Project")]
    public static void SetupProject()
    {
        CreateAllScenes();
        CreateTeamDatabase();
        Debug.Log("=== PROJECT SETUP COMPLETE ===");
        EditorUtility.DisplayDialog("Done!", "All scenes created and added to Build Settings!", "OK");
    }

    static void CreateAllScenes()
    {
        string[] sceneNames = {
            "Boot", "MainMenu", "Instructions", "TeamSelection",
            "TeamConfirmation", "Bracket", "PenaltyMatch",
            "Result", "Champion", "GameOver"
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        foreach (string sceneName in sceneNames)
        {
            string path = $"Assets/Scenes/{sceneName}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            switch (sceneName)
            {
                case "Boot":         SetupBootScene(); break;
                case "MainMenu":     SetupMainMenuScene(); break;
                case "Instructions": SetupInstructionsScene(); break;
                case "TeamSelection":SetupTeamSelectionScene(); break;
                case "TeamConfirmation": SetupTeamConfirmationScene(); break;
                case "Bracket":      SetupBracketScene(); break;
                case "PenaltyMatch": SetupPenaltyMatchScene(); break;
                case "Result":       SetupResultScene(); break;
                case "Champion":     SetupChampionScene(); break;
                case "GameOver":     SetupGameOverScene(); break;
            }

            EditorSceneManager.SaveScene(scene, path);
            buildScenes.Add(new EditorBuildSettingsScene(path, true));
            Debug.Log($"Created: {path}");
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
    }

    // ── BOOT ──────────────────────────────────────────────────────────────
    static void SetupBootScene()
    {
        CreateCamera();

        var gm = CreateGameObject("GameManager");
        gm.AddComponent<GameManager>();

        var sl = CreateGameObject("SceneLoader");
        sl.AddComponent<SceneLoader>();

        var tm = CreateGameObject("TournamentManager");
        tm.AddComponent<TournamentManager>();

        var am = CreateGameObject("AudioManager");
        var audioMgr = am.AddComponent<AudioManager>();
        am.AddComponent<AudioSource>(); // sfxSource — assign in inspector
    }

    // ── MAIN MENU ─────────────────────────────────────────────────────────
    static void SetupMainMenuScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "TitleText", "PENALTY SHOOTOUT CUP", 60, TextAnchor.MiddleCenter,
            new Vector2(0, 200), new Vector2(800, 120));

        CreateButton(panel.transform, "PlayButton",        "PLAY",         new Vector2(0, 50));
        CreateButton(panel.transform, "InstructionsButton","INSTRUCTIONS", new Vector2(0, -50));
        CreateButton(panel.transform, "QuitButton",        "QUIT",         new Vector2(0, -150));

        var menuGO = CreateGameObject("MainMenuUI");
        menuGO.AddComponent<MainMenuUI>();
    }

    // ── INSTRUCTIONS ──────────────────────────────────────────────────────
    static void SetupInstructionsScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "HeaderText", "HOW TO PLAY", 50, TextAnchor.MiddleCenter,
            new Vector2(0, 300), new Vector2(700, 80));

        string instructions =
            "1. Pick a team\n" +
            "2. TAP to start run-up\n" +
            "3. TAP again to shoot\n" +
            "4. TAP to dive as goalkeeper\n" +
            "5. Win all 3 rounds to become Champion!";

        CreateText(panel.transform, "InstructionsText", instructions, 30, TextAnchor.MiddleLeft,
            new Vector2(0, 0), new Vector2(700, 400));

        CreateButton(panel.transform, "BackButton", "BACK", new Vector2(0, -350));

        var go = CreateGameObject("InstructionsUI");
        go.AddComponent<InstructionsUI>();
    }

    // ── TEAM SELECTION ────────────────────────────────────────────────────
    static void SetupTeamSelectionScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "HeaderText", "SELECT YOUR TEAM", 48, TextAnchor.MiddleCenter,
            new Vector2(0, 380), new Vector2(700, 80));

        // Grid for team cards
        var gridGO = new GameObject("TeamGrid");
        gridGO.transform.SetParent(panel.transform, false);
        var rectGrid = gridGO.AddComponent<RectTransform>();
        rectGrid.sizeDelta = new Vector2(700, 600);
        rectGrid.anchoredPosition = new Vector2(0, -50);
        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(320, 140);
        glg.spacing = new Vector2(20, 20);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;
        glg.childAlignment = TextAnchor.UpperCenter;

        // Team card prefab hint
        CreateText(panel.transform, "HintText", "(Team cards will appear here at runtime)", 22,
            TextAnchor.MiddleCenter, new Vector2(0, -50), new Vector2(700, 60));

        var go = CreateGameObject("TeamSelectionUI");
        go.AddComponent<TeamSelectionUI>();
    }

    // ── TEAM CONFIRMATION ─────────────────────────────────────────────────
    static void SetupTeamConfirmationScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "HeaderText", "YOUR TEAM", 48, TextAnchor.MiddleCenter,
            new Vector2(0, 350), new Vector2(700, 80));

        var logoGO = new GameObject("LogoImage");
        logoGO.transform.SetParent(panel.transform, false);
        var logoRect = logoGO.AddComponent<RectTransform>();
        logoRect.sizeDelta = new Vector2(200, 200);
        logoRect.anchoredPosition = new Vector2(0, 100);
        var logoImg = logoGO.AddComponent<Image>();
        logoImg.color = Color.white;

        CreateText(panel.transform, "TeamNameText", "Team Name", 40, TextAnchor.MiddleCenter,
            new Vector2(0, -60), new Vector2(600, 70));

        CreateButton(panel.transform, "ConfirmButton", "CONFIRM", new Vector2(0, -180));
        CreateButton(panel.transform, "BackButton",    "BACK",    new Vector2(0, -280));

        var go = CreateGameObject("TeamConfirmationUI");
        go.AddComponent<TeamConfirmationUI>();
    }

    // ── BRACKET ───────────────────────────────────────────────────────────
    static void SetupBracketScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "RoundNameText", "QUARTER FINAL", 52, TextAnchor.MiddleCenter,
            new Vector2(0, 350), new Vector2(700, 80));

        CreateText(panel.transform, "MatchupText", "Team A\nvs\nTeam B", 38, TextAnchor.MiddleCenter,
            new Vector2(0, 150), new Vector2(600, 200));

        CreateText(panel.transform, "OtherMatchesText", "", 24, TextAnchor.MiddleLeft,
            new Vector2(0, -80), new Vector2(600, 200));

        CreateButton(panel.transform, "StartMatchButton", "START MATCH", new Vector2(0, -270));
        CreateButton(panel.transform, "MainMenuButton",   "MAIN MENU",   new Vector2(0, -370));

        var go = CreateGameObject("BracketUI");
        go.AddComponent<BracketUI>();
    }

    // ── PENALTY MATCH ─────────────────────────────────────────────────────
    static void SetupPenaltyMatchScene()
    {
        CreateCamera();
        CreateEventSystem();

        // Field background
        var field = CreateColoredSprite("FieldBackground", new Color(0.2f, 0.6f, 0.2f),
            Vector3.zero, new Vector3(12f, 8f, 1f));

        // Goal
        var goal = CreateColoredSprite("Goal", Color.white,
            new Vector3(0f, 1.5f, 0f), new Vector3(4f, 2.5f, 1f));

        // Goal posts (visual)
        CreateColoredSprite("GoalPostLeft",  Color.white, new Vector3(-2f, 1.5f, -0.1f), new Vector3(0.15f, 2.5f, 1f));
        CreateColoredSprite("GoalPostRight", Color.white, new Vector3(2f,  1.5f, -0.1f), new Vector3(0.15f, 2.5f, 1f));
        CreateColoredSprite("GoalCrossbar",  Color.white, new Vector3(0f,  2.8f, -0.1f), new Vector3(4.15f, 0.15f, 1f));

        // Penalty spot
        var spot = new GameObject("PenaltySpot");
        spot.transform.position = new Vector3(0f, -2f, 0f);

        // Ball
        var ball = CreateColoredSprite("Ball", Color.white,
            new Vector3(0f, -2f, -0.2f), new Vector3(0.3f, 0.3f, 1f));

        // Shooter (placeholder)
        var shooter = CreateColoredSprite("Shooter", new Color(0.2f, 0.4f, 0.8f),
            new Vector3(-1f, -2.5f, -0.3f), new Vector3(0.5f, 0.8f, 1f));

        // Goalkeeper
        var keeper = CreateColoredSprite("Goalkeeper", new Color(0.8f, 0.2f, 0.2f),
            new Vector3(0f, 1.2f, -0.1f), new Vector3(0.6f, 0.9f, 1f));

        // Keeper zone markers
        var kpLeft   = new GameObject("KP_Left");   kpLeft.transform.position   = new Vector3(-1.5f, 1.2f, 0);
        var kpRight  = new GameObject("KP_Right");  kpRight.transform.position  = new Vector3(1.5f,  1.2f, 0);
        var kpCenter = new GameObject("KP_Center"); kpCenter.transform.position = new Vector3(0f,    1.2f, 0);
        var kpHL     = new GameObject("KP_HighLeft");  kpHL.transform.position  = new Vector3(-1.5f, 2.4f, 0);
        var kpHR     = new GameObject("KP_HighRight"); kpHR.transform.position  = new Vector3(1.5f,  2.4f, 0);
        var kpLL     = new GameObject("KP_LowLeft");   kpLL.transform.position  = new Vector3(-1.5f, 0.3f, 0);
        var kpLR     = new GameObject("KP_LowRight");  kpLR.transform.position  = new Vector3(1.5f,  0.3f, 0);

        // Goal target center
        var goalCenter = new GameObject("GoalCenter");
        goalCenter.transform.position = new Vector3(0f, 1.5f, 0f);

        // Miss target
        var missTarget = new GameObject("MissTarget");
        missTarget.transform.position = new Vector3(4f, -1f, 0f);

        // Run-up position
        var runUpPos = new GameObject("RunUpPosition");
        runUpPos.transform.position = new Vector3(-1.5f, -3f, 0f);

        // HUD Canvas
        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "RoundText",       "Quarter Final", 28, TextAnchor.UpperCenter,  new Vector2(0, -10),  new Vector2(400, 50));
        CreateText(panel.transform, "PlayerTeamText",  "Player Team",   24, TextAnchor.UpperLeft,    new Vector2(-300, -60), new Vector2(250, 40));
        CreateText(panel.transform, "ScoreText",       "0 - 0",         36, TextAnchor.UpperCenter,  new Vector2(0, -60),  new Vector2(200, 50));
        CreateText(panel.transform, "OpponentTeamText","Opponent",      24, TextAnchor.UpperRight,   new Vector2(300, -60), new Vector2(250, 40));

        var instrText = CreateText(panel.transform, "InstructionText", "YOUR SHOT - Tap to start", 30,
            TextAnchor.LowerCenter, new Vector2(0, 80), new Vector2(600, 60));

        var outcomeText = CreateText(panel.transform, "OutcomeText", "GOAL!", 72,
            TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(600, 120));
        outcomeText.color = Color.green;

        // Penalty icons parents
        var playerIconsGO = new GameObject("PlayerIcons");
        playerIconsGO.transform.SetParent(panel.transform, false);
        var piRect = playerIconsGO.AddComponent<RectTransform>();
        piRect.anchoredPosition = new Vector2(-200, -120);
        piRect.sizeDelta = new Vector2(200, 30);
        var pigLayout = playerIconsGO.AddComponent<HorizontalLayoutGroup>();
        pigLayout.spacing = 5;

        var opponentIconsGO = new GameObject("OpponentIcons");
        opponentIconsGO.transform.SetParent(panel.transform, false);
        var oiRect = opponentIconsGO.AddComponent<RectTransform>();
        oiRect.anchoredPosition = new Vector2(200, -120);
        oiRect.sizeDelta = new Vector2(200, 30);
        var oigLayout = opponentIconsGO.AddComponent<HorizontalLayoutGroup>();
        oigLayout.spacing = 5;

        // Scripts
        var pmm = CreateGameObject("PenaltyMatchManager");
        var pmmComp = pmm.AddComponent<PenaltyMatchManager>();

        var shooterGO = CreateGameObject("ShooterController");
        var shooterComp = shooterGO.AddComponent<ShooterController>();
        shooterComp.shooterTransform  = shooter.transform;
        shooterComp.ballTransform     = ball.transform;
        shooterComp.penaltySpotPosition = spot.transform;
        shooterComp.runUpPosition     = runUpPos.transform;

        // GoalkeeperController lives on "Goalkeeper" visual GO — uses this.transform
        keeper.AddComponent<GoalkeeperController>();

        var ballGO = CreateGameObject("BallController");
        var ballComp = ballGO.AddComponent<BallController>();
        ballComp.ballTransform  = ball.transform;
        ballComp.penaltySpot    = spot.transform;
        ballComp.goalCenter     = goalCenter.transform;
        ballComp.missTarget     = missTarget.transform;
    }

    // ── RESULT ────────────────────────────────────────────────────────────
    static void SetupResultScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "ResultText", "YOU WON!", 64, TextAnchor.MiddleCenter,
            new Vector2(0, 200), new Vector2(700, 100));
        CreateText(panel.transform, "ScoreText",  "3 - 1",   48, TextAnchor.MiddleCenter,
            new Vector2(0, 80),  new Vector2(400, 80));

        CreateButton(panel.transform, "ContinueButton", "CONTINUE",  new Vector2(0, -80));
        CreateButton(panel.transform, "RetryButton",    "RETRY",     new Vector2(0, -180));
        CreateButton(panel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -280));

        var go = CreateGameObject("ResultUI");
        go.AddComponent<ResultUI>();
    }

    // ── CHAMPION ──────────────────────────────────────────────────────────
    static void SetupChampionScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "ChampionText", "CHAMPION!", 72, TextAnchor.MiddleCenter,
            new Vector2(0, 300), new Vector2(700, 100));

        var logoGO = new GameObject("TeamLogoImage");
        logoGO.transform.SetParent(panel.transform, false);
        var logoRect = logoGO.AddComponent<RectTransform>();
        logoRect.sizeDelta = new Vector2(180, 180);
        logoRect.anchoredPosition = new Vector2(0, 80);
        logoGO.AddComponent<Image>().color = Color.yellow;

        CreateText(panel.transform, "TeamNameText", "Istanbul Lions", 40, TextAnchor.MiddleCenter,
            new Vector2(0, -60), new Vector2(600, 70));

        CreateButton(panel.transform, "PlayAgainButton", "PLAY AGAIN", new Vector2(0, -200));
        CreateButton(panel.transform, "MainMenuButton",  "MAIN MENU", new Vector2(0, -300));

        var go = CreateGameObject("ChampionUI");
        go.AddComponent<ChampionUI>();
    }

    // ── GAME OVER ─────────────────────────────────────────────────────────
    static void SetupGameOverScene()
    {
        CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas("Canvas");
        var panel  = CreatePanel(canvas.transform, "SafeAreaPanel");

        CreateText(panel.transform, "GameOverText", "ELIMINATED!", 64, TextAnchor.MiddleCenter,
            new Vector2(0, 200), new Vector2(700, 100));
        CreateText(panel.transform, "ScoreText", "0 - 0", 40, TextAnchor.MiddleCenter,
            new Vector2(0, 80), new Vector2(400, 70));

        CreateButton(panel.transform, "RetryButton",    "TRY AGAIN", new Vector2(0, -80));
        CreateButton(panel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -180));

        var go = CreateGameObject("GameOverUI");
        go.AddComponent<GameOverUI>();
    }

    // ── TEAM DATABASE ─────────────────────────────────────────────────────
    static void CreateTeamDatabase()
    {
        TeamDatabase db = ScriptableObject.CreateInstance<TeamDatabase>();

        string[] names = {
            "Istanbul Lions", "Madrid Crowns", "London Blues", "Milan Flames",
            "Paris Stars", "Munich Eagles", "Lisbon Waves", "Amsterdam Reds"
        };
        Color[] primaries = {
            new Color(0.8f,0.1f,0.1f), new Color(0.8f,0.6f,0f),
            new Color(0f,0.2f,0.8f),   new Color(0.1f,0.1f,0.1f),
            new Color(0f,0.2f,0.6f),   new Color(0.6f,0.5f,0f),
            new Color(0.6f,0f,0.8f),   new Color(0.8f,0.1f,0.1f)
        };
        int[] shooterStr = { 80, 75, 70, 85, 65, 78, 60, 72 };
        int[] keeperStr  = { 70, 80, 75, 65, 78, 70, 65, 68 };
        int[] aiStr      = { 65, 75, 70, 80, 60, 72, 55, 65 };

        for (int i = 0; i < names.Length; i++)
        {
            TeamData team = ScriptableObject.CreateInstance<TeamData>();
            team.teamId         = names[i].Replace(" ", "").ToLower();
            team.teamName       = names[i];
            team.primaryColor   = primaries[i];
            team.secondaryColor = Color.white;
            team.shooterStrength = shooterStr[i];
            team.keeperStrength  = keeperStr[i];
            team.aiStrength      = aiStr[i];

            AssetDatabase.CreateAsset(team, $"Assets/ScriptableObjects/Teams/{team.teamId}.asset");
            db.allTeams.Add(team);
        }

        AssetDatabase.CreateAsset(db, "Assets/ScriptableObjects/TeamDatabase.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("TeamDatabase created with 8 teams.");
    }

    // ── HELPERS ───────────────────────────────────────────────────────────
    static GameObject CreateGameObject(string name)
    {
        var go = new GameObject(name);
        return go;
    }

    static GameObject CreateCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.AddComponent<AudioListener>();
        return camGO;
    }

    static GameObject CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        return go;
    }

    static GameObject CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        return go;
    }

    static Text CreateText(Transform parent, string name, string content, int fontSize,
        TextAnchor alignment, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }

    static GameObject CreateButton(Transform parent, string name, string label, Vector2 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 80);
        rect.anchoredPosition = position;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.5f, 0.85f);
        var btn = go.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tRect = textGO.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;
        var text = textGO.AddComponent<Text>();
        text.text = label;
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return go;
    }

    static GameObject CreateColoredSprite(string name, Color color, Vector3 position, Vector3 scale)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return go;
    }
}
