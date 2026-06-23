using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip buttonClick;
    public AudioClip kickSound;
    public AudioClip goalSound;
    public AudioClip saveSound;
    public AudioClip missSound;
    public AudioClip postSound;
    public AudioClip whistleSound;
    public AudioClip crowdShort;

    [Header("Music")]
    public AudioSource musicSource;

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

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayKick()        => PlaySFX(kickSound);
    public void PlayGoal()        => PlaySFX(goalSound);
    public void PlaySave()        => PlaySFX(saveSound);
    public void PlayMiss()        => PlaySFX(missSound);
    public void PlayPost()        => PlaySFX(postSound);
    public void PlayWhistle()     => PlaySFX(whistleSound);
    public void PlayCrowd()       => PlaySFX(crowdShort);
}
