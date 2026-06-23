using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMainMenu()       => SceneManager.LoadScene("MainMenu");
    public void LoadInstructions()   => SceneManager.LoadScene("Instructions");
    public void LoadTeamSelection()  => SceneManager.LoadScene("TeamSelection");
    public void LoadTeamConfirmation() => SceneManager.LoadScene("TeamConfirmation");
    public void LoadBracket()        => SceneManager.LoadScene("Bracket");
    public void LoadPenaltyMatch()   => SceneManager.LoadScene("PenaltyMatch");
    public void LoadResult()         => SceneManager.LoadScene("Result");
    public void LoadChampion()       => SceneManager.LoadScene("Champion");
    public void LoadGameOver()       => SceneManager.LoadScene("GameOver");
}
