using UnityEngine;
using System.Collections;

// Kale ağı — top girince yatay şeritler sallanır.
public class GoalNet : MonoBehaviour
{
    public static GoalNet Instance { get; private set; }

    const int   H = 8;       // yatay şerit sayısı
    const int   V = 14;      // dikey şerit sayısı
    const float LEFT  = -2.8f;
    const float RIGHT =  2.8f;
    const float BOTTOM = 0.68f;
    const float TOP    = 3.15f;

    Transform[] hStrands;
    Vector3[]   hOrigins;
    bool        swaying;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => BuildNet();

    void BuildNet()
    {
        var netColor = new Color(1f, 1f, 1f, 0.45f);
        var backColor = new Color(1f, 1f, 1f, 0.20f);

        hStrands = new Transform[H];
        hOrigins = new Vector3[H];

        for (int i = 0; i < H; i++)
        {
            float y = Mathf.Lerp(BOTTOM, TOP, (float)i / (H - 1));
            float alpha = Mathf.Lerp(0.28f, 0.50f, (float)i / (H - 1));
            var c = new Color(1f, 1f, 1f, alpha);
            var go = MakeLine($"H{i}", new Vector3(0, y, 1f), RIGHT - LEFT + 0.1f, 0.022f, c, -9);
            hStrands[i] = go.transform;
            hOrigins[i] = go.transform.position;
        }

        for (int i = 0; i < V; i++)
        {
            float x = Mathf.Lerp(LEFT, RIGHT, (float)i / (V - 1));
            float midY = (BOTTOM + TOP) * 0.5f;
            MakeLine($"V{i}", new Vector3(x, midY, 1f), 0.022f, TOP - BOTTOM, backColor, -10);
        }
    }

    public void TriggerBallEntered()
    {
        if (!swaying) StartCoroutine(SwayRoutine());
    }

    IEnumerator SwayRoutine()
    {
        swaying = true;
        float elapsed = 0f, duration = 1.8f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t     = Mathf.Clamp01(elapsed / duration);
            float decay = Mathf.Pow(1f - t, 1.5f);

            for (int i = 0; i < hStrands.Length; i++)
            {
                float phase  = i * 0.5f + elapsed * 9f;
                float offset = Mathf.Sin(phase) * 0.18f * decay;
                hStrands[i].position = hOrigins[i] + new Vector3(offset, Mathf.Sin(phase * 0.7f) * 0.08f * decay, 0);
            }
            yield return null;
        }

        for (int i = 0; i < hStrands.Length; i++)
            hStrands[i].position = hOrigins[i];

        swaying = false;
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
