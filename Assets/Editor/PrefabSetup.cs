using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class PrefabSetup : EditorWindow
{
    [MenuItem("PenaltyGame/Create Prefabs & Link Scenes")]
    public static void CreatePrefabs()
    {
        CreateTeamCardPrefab();
        CreatePenaltyIconPrefab();
        LinkTeamSelectionScene();
        LinkMatchHUDScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done!", "Prefabs created and scenes linked!", "OK");
    }

    static void CreateTeamCardPrefab()
    {
        // Create TeamCard GameObject
        var card = new GameObject("TeamCard");
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(320, 140);

        var cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.15f, 0.2f, 0.35f);

        // Logo
        var logoGO = new GameObject("LogoImage");
        logoGO.transform.SetParent(card.transform, false);
        var logoRect = logoGO.AddComponent<RectTransform>();
        logoRect.sizeDelta = new Vector2(80, 80);
        logoRect.anchoredPosition = new Vector2(-100, 10);
        var logoImg = logoGO.AddComponent<Image>();
        logoImg.color = Color.white;

        // Kit
        var kitGO = new GameObject("KitImage");
        kitGO.transform.SetParent(card.transform, false);
        var kitRect = kitGO.AddComponent<RectTransform>();
        kitRect.sizeDelta = new Vector2(50, 70);
        kitRect.anchoredPosition = new Vector2(-20, 10);
        var kitImg = kitGO.AddComponent<Image>();
        kitImg.color = Color.gray;

        // Team name
        var nameGO = new GameObject("TeamNameText");
        nameGO.transform.SetParent(card.transform, false);
        var nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(160, 40);
        nameRect.anchoredPosition = new Vector2(60, 30);
        var nameText = nameGO.AddComponent<Text>();
        nameText.text = "Team Name";
        nameText.fontSize = 22;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = Color.white;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Strength
        var strGO = new GameObject("StrengthText");
        strGO.transform.SetParent(card.transform, false);
        var strRect = strGO.AddComponent<RectTransform>();
        strRect.sizeDelta = new Vector2(160, 30);
        strRect.anchoredPosition = new Vector2(60, -15);
        var strText = strGO.AddComponent<Text>();
        strText.text = "STR: 80  GK: 70";
        strText.fontSize = 18;
        strText.alignment = TextAnchor.MiddleLeft;
        strText.color = new Color(0.8f, 0.8f, 0.8f);
        strText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Select Button
        var btnGO = new GameObject("SelectButton");
        btnGO.transform.SetParent(card.transform, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(120, 36);
        btnRect.anchoredPosition = new Vector2(80, -48);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.55f, 0.9f);
        var btn = btnGO.AddComponent<Button>();

        var btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnTRect = btnTextGO.AddComponent<RectTransform>();
        btnTRect.anchorMin = Vector2.zero;
        btnTRect.anchorMax = Vector2.one;
        btnTRect.offsetMin = Vector2.zero;
        btnTRect.offsetMax = Vector2.zero;
        var btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "SELECT";
        btnText.fontSize = 20;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Add TeamCardUI component
        var cardUI = card.AddComponent<TeamCardUI>();
        cardUI.logoImage    = logoImg;
        cardUI.kitImage     = kitImg;
        cardUI.teamNameText = nameText;
        cardUI.strengthText = strText;
        cardUI.selectButton = btn;

        // Save as prefab
        var prefab = PrefabUtility.SaveAsPrefabAsset(card, "Assets/Prefabs/UI/TeamCard.prefab");
        Object.DestroyImmediate(card);
        Debug.Log("TeamCard prefab created.");
    }

    static void CreatePenaltyIconPrefab()
    {
        var icon = new GameObject("PenaltyIcon");
        var rect = icon.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(28, 28);
        var img = icon.AddComponent<Image>();
        img.color = Color.green;

        PrefabUtility.SaveAsPrefabAsset(icon, "Assets/Prefabs/UI/PenaltyIcon.prefab");
        Object.DestroyImmediate(icon);
        Debug.Log("PenaltyIcon prefab created.");
    }

    static void LinkTeamSelectionScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/TeamSelection.unity", OpenSceneMode.Single);

        var tsUI = Object.FindFirstObjectByType<TeamSelectionUI>();
        if (tsUI != null)
        {
            var db = AssetDatabase.LoadAssetAtPath<TeamDatabase>("Assets/ScriptableObjects/TeamDatabase.asset");
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/TeamCard.prefab");

            if (db != null) tsUI.teamDatabase = db;
            if (cardPrefab != null) tsUI.teamCardPrefab = cardPrefab;

            // Link TeamGrid
            var grid = GameObject.Find("TeamGrid");
            if (grid != null) tsUI.teamGrid = grid.transform;

            EditorUtility.SetDirty(tsUI);
            Debug.Log("TeamSelectionUI linked.");
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void LinkMatchHUDScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PenaltyMatch.unity", OpenSceneMode.Single);

        var hud = Object.FindFirstObjectByType<MatchHUD>();
        if (hud != null)
        {
            var iconPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/PenaltyIcon.prefab");
            if (iconPrefab != null) hud.penaltyIconPrefab = iconPrefab;

            // Link text fields
            hud.roundText        = FindText("RoundText");
            hud.playerTeamText   = FindText("PlayerTeamText");
            hud.opponentTeamText = FindText("OpponentTeamText");
            hud.scoreText        = FindText("ScoreText");
            hud.instructionText  = FindText("InstructionText");
            hud.outcomeText      = FindText("OutcomeText");

            var playerIcons   = GameObject.Find("PlayerIcons");
            var opponentIcons = GameObject.Find("OpponentIcons");
            if (playerIcons != null)   hud.playerIconsParent   = playerIcons.transform;
            if (opponentIcons != null) hud.opponentIconsParent = opponentIcons.transform;

            EditorUtility.SetDirty(hud);
            Debug.Log("MatchHUD linked.");
        }

        // Link PenaltyMatchManager references
        var pmm = Object.FindFirstObjectByType<PenaltyMatchManager>();
        if (pmm != null)
        {
            pmm.shooterController    = Object.FindFirstObjectByType<ShooterController>();
            pmm.goalkeeperController = Object.FindFirstObjectByType<GoalkeeperController>();
            pmm.ballController       = Object.FindFirstObjectByType<BallController>();
            pmm.matchHUD             = hud;
            EditorUtility.SetDirty(pmm);
            Debug.Log("PenaltyMatchManager linked.");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("PenaltyMatch scene saved.");
    }

    static Text FindText(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Text>() : null;
    }
}
