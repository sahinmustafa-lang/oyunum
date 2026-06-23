using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    void Start()
    {
        BindButton("PlayButton",         OnPlayPressed);
        BindButton("InstructionsButton", OnInstructionsPressed);
        BindButton("QuitButton",         OnQuitPressed);
    }

    void BindButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        var go = GameObject.Find(buttonName);
        if (go != null)
        {
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(action);
        }
    }

    void OnPlayPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneLoader.Instance.LoadTeamSelection();
    }

    void OnInstructionsPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneLoader.Instance.LoadInstructions();
    }

    void OnQuitPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        Application.Quit();
    }
}
