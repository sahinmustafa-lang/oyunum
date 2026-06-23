using UnityEngine;
using System.Collections;

// Kale ağı animasyonu — normalde görünmez, gol olunca belirir/sallanır/söner.
// Böylece fotoğraftaki ağ sanki hareket ediyormuş izlenimi verir.
public class GoalNet : MonoBehaviour
{
    public static GoalNet Instance { get; private set; }

    const int   H      = 10;
    const int   V      = 16;
    const float LEFT   = -2.8f;
    const float RIGHT  =  2.8f;
    const float BOTTOM =  0.68f;
    const float TOP    =  3.15f;

    SpriteRenderer[] hRenderers;
    SpriteRenderer[] vRenderers;
    Transform[]      hStrands;
    Vector3[]        hOrigins;
    float[]          hBaseAlpha;
    bool             swaying;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => BuildNet();

    void BuildNet()
    {
        hStrands    = new Transform[H];
        hOrigins    = new Vector3[H];
        hRenderers  = new SpriteRenderer[H];
        hBaseAlpha  = new float[H];
        vRenderers  = new SpriteRenderer[V];

        // Yatay şeritler — başlangıçta görünmez (alpha=0)
        for (int i = 0; i < H; i++)
        {
            float y     = Mathf.Lerp(BOTTOM, TOP, (float)i / (H - 1));
            float alpha = Mathf.Lerp(0.55f, 0.85f, (float)i / (H - 1));
            hBaseAlpha[i] = alpha;
            var go = MakeLine($"H{i}", new Vector3(0, y, 1f), RIGHT - LEFT + 0.1f, 0.020f,
                              new Color(1f, 1f, 1f, 0f), 2);
            hStrands[i]   = go.transform;
            hOrigins[i]   = go.transform.position;
            hRenderers[i] = go.GetComponent<SpriteRenderer>();
        }

        // Dikey şeritler — başlangıçta görünmez (alpha=0)
        for (int i = 0; i < V; i++)
        {
            float x    = Mathf.Lerp(LEFT, RIGHT, (float)i / (V - 1));
            float midY = (BOTTOM + TOP) * 0.5f;
            var go = MakeLine($"V{i}", new Vector3(x, midY, 1f), 0.018f, TOP - BOTTOM,
                              new Color(1f, 1f, 1f, 0f), 1);
            vRenderers[i] = go.GetComponent<SpriteRenderer>();
        }
    }

    public void TriggerBallEntered()
    {
        if (!swaying) StartCoroutine(SwayRoutine());
    }

    IEnumerator SwayRoutine()
    {
        swaying = true;

        // 1. Hızlıca belir (0.12 sn)
        yield return FadeAll(0f, 1f, 0.12f);

        // 2. Salla (1.6 sn)
        float elapsed = 0f, duration = 1.6f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t     = Mathf.Clamp01(elapsed / duration);
            float decay = Mathf.Pow(1f - t, 1.4f);

            for (int i = 0; i < hStrands.Length; i++)
            {
                float phase  = i * 0.6f + elapsed * 10f;
                float ox     = Mathf.Sin(phase)       * 0.22f * decay;
                float oy     = Mathf.Sin(phase * 0.7f) * 0.10f * decay;
                hStrands[i].position = hOrigins[i] + new Vector3(ox, oy, 0);
            }
            yield return null;
        }

        // Şeritleri orijinal pozisyona döndür
        for (int i = 0; i < hStrands.Length; i++)
            hStrands[i].position = hOrigins[i];

        // 3. Yavaşça söner (0.4 sn)
        yield return FadeAll(1f, 0f, 0.40f);

        swaying = false;
    }

    IEnumerator FadeAll(float fromT, float toT, float dur)
    {
        float e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            float t = Mathf.Clamp01(e / dur);
            float blend = Mathf.Lerp(fromT, toT, t);
            for (int i = 0; i < hRenderers.Length; i++)
            {
                var c = hRenderers[i].color;
                c.a = hBaseAlpha[i] * blend;
                hRenderers[i].color = c;
            }
            float vAlpha = 0.35f * blend;
            foreach (var sr in vRenderers)
            {
                var c = sr.color; c.a = vAlpha; sr.color = c;
            }
            yield return null;
        }
    }

    static GameObject MakeLine(string n, Vector3 pos, float w, float h, Color c, int sort)
    {
        var go = new GameObject(n);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(w, h, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeSprite(c);
        sr.sortingOrder = sort;
        return go;
    }

    static Sprite MakeSprite(Color c)
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { c, c, c, c });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }
}
