using UnityEngine;
using System.Collections;

public class GoalIndicator : MonoBehaviour
{
    public static GoalIndicator Instance { get; private set; }

    private GameObject redDot;
    private GameObject[] zoneHints = new GameObject[9];
    private bool isShowing = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        CreateRedDot();
        CreateZoneHints();
    }

    void CreateRedDot()
    {
        redDot = new GameObject("RedDot");
        var sr = redDot.AddComponent<SpriteRenderer>();
        sr.sprite = MakeCircleSprite(Color.red);
        sr.sortingOrder = 10;
        redDot.transform.localScale = Vector3.one * 0.4f;
        redDot.SetActive(false);
    }

    void CreateZoneHints()
    {
        string[] names = {
            "LowLeft","MidLeft","HighLeft",
            "LowCenter","MidCenter","HighCenter",
            "LowRight","MidRight","HighRight"
        };

        for (int i = 0; i < 9; i++)
        {
            var go = new GameObject("ZoneHint_" + names[i]);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite(new Color(1f, 1f, 0f, 0.25f));
            sr.sortingOrder = 8;
            go.transform.position = GetZonePosition((ShotZone)i);
            go.transform.localScale = new Vector3(1.25f, 0.80f, 1f);
            go.SetActive(false);
            zoneHints[i] = go;
        }
    }

    // Called when opponent is about to shoot — show red dot briefly
    public IEnumerator ShowTargetDotRoutine(ShotZone targetZone, float showDuration)
    {
        if (isShowing) yield break;
        isShowing = true;

        Vector3 pos = GetZonePosition(targetZone);
        redDot.transform.position = pos;
        redDot.SetActive(true);

        // Pulse effect
        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            float scale = 0.3f + Mathf.PingPong(elapsed * 4f, 0.15f);
            redDot.transform.localScale = Vector3.one * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        redDot.SetActive(false);
        isShowing = false;
    }

    // Show zone grid when player is aiming
    public void ShowZoneHints()
    {
        foreach (var hint in zoneHints)
            if (hint != null) hint.SetActive(true);
    }

    public void HideZoneHints()
    {
        foreach (var hint in zoneHints)
            if (hint != null) hint.SetActive(false);
    }

    // Flash the chosen zone briefly after shot
    public IEnumerator FlashZone(ShotZone zone, Color color)
    {
        int idx = (int)zone;
        if (zoneHints[idx] == null) yield break;

        var sr = zoneHints[idx].GetComponent<SpriteRenderer>();
        sr.color = new Color(color.r, color.g, color.b, 0.7f);
        zoneHints[idx].SetActive(true);
        yield return new WaitForSeconds(0.4f);
        sr.color = new Color(1f, 1f, 0f, 0.25f);
        zoneHints[idx].SetActive(false);
    }

    // Goal zone world positions — goal center Y=1.9, height=2.6 (Y: 0.6 to 3.2)
    public static Vector3 GetZonePosition(ShotZone zone)
    {
        // Fotoğraftaki kale: direkler ±3.6, kiriş y=2.52, kale çizgisi y=0.88
        float[] xPos = { -2.4f, -2.4f, -2.4f,  0f,  0f,  0f,  2.4f, 2.4f, 2.4f };
        float[] yPos = { 1.10f, 1.70f, 2.28f, 1.10f, 1.70f, 2.28f, 1.10f, 1.70f, 2.28f };
        int idx = (int)zone;
        return new Vector3(xPos[idx], yPos[idx], -0.5f);
    }

    static Sprite MakeCircleSprite(Color color)
    {
        int size = 32;
        var tex = new Texture2D(size, size);
        float center = size / 2f;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist < center - 1 ? color : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    static Sprite MakeSquareSprite(Color color)
    {
        var tex = new Texture2D(4, 4);
        for (int i = 0; i < 16; i++) tex.SetPixel(i % 4, i / 4, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
