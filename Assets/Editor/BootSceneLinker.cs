using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BootSceneLinker : EditorWindow
{
    [MenuItem("PenaltyGame/Link Boot Scene References")]
    public static void LinkBootScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity", OpenSceneMode.Single);

        // GameManager → TeamDatabase
        var gmGO = GameObject.Find("GameManager");
        if (gmGO != null)
        {
            var gm = gmGO.GetComponent<GameManager>();
            var db = AssetDatabase.LoadAssetAtPath<TeamDatabase>("Assets/ScriptableObjects/TeamDatabase.asset");
            if (gm != null && db != null)
            {
                gm.teamDatabase = db;
                Debug.Log("TeamDatabase linked to GameManager.");
            }
        }

        // AudioManager → AudioSource
        var amGO = GameObject.Find("AudioManager");
        if (amGO != null)
        {
            var am = amGO.GetComponent<AudioManager>();
            var sources = amGO.GetComponents<AudioSource>();
            if (sources.Length == 0)
            {
                var sfx = amGO.AddComponent<AudioSource>();
                sfx.playOnAwake = false;
                am.sfxSource = sfx;
            }
            else
            {
                sources[0].playOnAwake = false;
                am.sfxSource = sources[0];
            }
        }

        // BootLoader
        var bootGO = GameObject.Find("BootLoader");
        if (bootGO == null)
        {
            bootGO = new GameObject("BootLoader");
            bootGO.AddComponent<BootLoader>();
            Debug.Log("BootLoader added.");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Boot scene saved.");
        EditorUtility.DisplayDialog("Done!", "Boot scene fully linked!", "OK");
    }

    [MenuItem("PenaltyGame/Open Boot Scene")]
    public static void OpenBootScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity", OpenSceneMode.Single);
    }

    [MenuItem("PenaltyGame/Open MainMenu Scene")]
    public static void OpenMainMenuScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
    }

    [MenuItem("PenaltyGame/Open PenaltyMatch Scene")]
    public static void OpenPenaltyMatchScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PenaltyMatch.unity", OpenSceneMode.Single);
    }
}
