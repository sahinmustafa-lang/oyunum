using UnityEngine;
using System.Collections;
using System.IO;

// Fotoğraftaki gerçek ağı shader ile animasyonlar.
// field_bg.png = ağsız arka plan, goal_net.png = sadece ağ (şeffaf PNG).
// Gol olunca ağ UV dalgalanması tetiklenir, sonra söner.
public class GoalNet : MonoBehaviour
{
    public static GoalNet Instance { get; private set; }

    SpriteRenderer netRenderer;
    Material       netMat;
    bool           animating;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(LoadNetSprite());
    }

    IEnumerator LoadNetSprite()
    {
        yield return null;

        string path = Path.Combine(Application.streamingAssetsPath, "Sprites/goal_net.png");
        if (!File.Exists(path)) { Debug.LogWarning("goal_net.png bulunamadı: " + path); yield break; }

        byte[]    data   = File.ReadAllBytes(path);
        Texture2D tex    = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode   = FilterMode.Bilinear;
        tex.LoadImage(data);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                      new Vector2(0.5f, 0.5f), 100f);

        var go = new GameObject("GoalNetSprite");
        netRenderer = go.AddComponent<SpriteRenderer>();
        netRenderer.sprite = sprite;

        // field_bg ile aynı boyut ve konuma yerleştir
        float natW = tex.width  / 100f;
        float natH = tex.height / 100f;
        float w = 20f, h = 8.22f, y = -0.89f;
        go.transform.position   = new Vector3(0, y, 0);
        go.transform.localScale = new Vector3(w / natW, h / natH, 1f);

        // NetWave shader'ını ata
        Shader shader = Shader.Find("Custom/NetWave");
        if (shader != null)
        {
            netMat = new Material(shader);
            netMat.mainTexture    = tex;
            netMat.SetFloat("_WaveAmp",   0.018f);
            netMat.SetFloat("_WaveFreq",  7.0f);
            netMat.SetFloat("_WaveSpeed", 4.0f);
            netMat.SetFloat("_Intensity", 0f);
            netRenderer.material  = netMat;
        }
        else
        {
            Debug.LogWarning("Custom/NetWave shader bulunamadı — default kullanılıyor");
        }

        netRenderer.sortingOrder = 2;
    }

    public void TriggerBallEntered()
    {
        if (!animating && netMat != null)
            StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        animating = true;

        // 0.12 sn'de yoğunluk 0 → 1
        yield return AnimateIntensity(0f, 1f, 0.12f);

        // 1.6 sn boyunca yoğunluk azalır (top çarptıktan sonra ağ durur)
        float elapsed = 0f, dur = 1.6f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float decay = Mathf.Pow(1f - Mathf.Clamp01(elapsed / dur), 1.5f);
            netMat.SetFloat("_Intensity", decay);
            yield return null;
        }

        netMat.SetFloat("_Intensity", 0f);
        animating = false;
    }

    IEnumerator AnimateIntensity(float from, float to, float dur)
    {
        float e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            netMat.SetFloat("_Intensity", Mathf.Lerp(from, to, e / dur));
            yield return null;
        }
        netMat.SetFloat("_Intensity", to);
    }
}
