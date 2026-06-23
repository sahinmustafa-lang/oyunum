using UnityEngine;
using System.Collections;

public class BootLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(0.1f);
        SceneLoader.Instance.LoadMainMenu();
    }
}
