using UnityEngine;
using UnityEngine.UI;

public class InstructionsUI : MonoBehaviour
{
    void Start()
    {
        var go = GameObject.Find("BackButton");
        if (go != null)
        {
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnBackPressed);
        }
    }

    void OnBackPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneLoader.Instance.LoadMainMenu();
    }
}
