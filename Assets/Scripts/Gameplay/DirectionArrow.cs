using UnityEngine;
using System.Collections;

// Yellow arrow on the ground that swings left-right showing shot direction.
// Tap 1 → ghost arrow stays (fake zone for goalkeeper), live arrow keeps moving.
// Tap 2 → live arrow locks in (real shot zone), then hides.
public class DirectionArrow : MonoBehaviour
{
    public static DirectionArrow Instance { get; private set; }

    private GameObject liveArrow;
    private GameObject ghostArrow;

    private bool  oscillating = false;
    private float phase       = 0f;
    private float lastArrowX  = 0f;

    const float SPEED     = 8.8f;    // oscillation speed (rad/s)
    const float AMPLITUDE = 1.6f;    // world units left/right
    const float ARROW_Y   = -2.15f;  // ground level at shooter feet
    const float ARROW_Z   = -1f;     // in front of everything

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        liveArrow  = BuildArrow("LiveArrow",  new Color(1f, 0.9f, 0f, 1.00f));
        ghostArrow = BuildArrow("GhostArrow", new Color(1f, 0.9f, 0f, 0.35f));
        liveArrow.SetActive(false);
        ghostArrow.SetActive(false);
    }

    void Update()
    {
        if (!oscillating || liveArrow == null) return;
        phase += Time.deltaTime * SPEED;
        liveArrow.transform.position = new Vector3(Mathf.Sin(phase) * AMPLITUDE, ARROW_Y, ARROW_Z);
    }

    public void StartArrow()
    {
        oscillating = true;
        phase       = -Mathf.PI / 2f;  // begin at left
        ghostArrow.SetActive(false);
        liveArrow.SetActive(true);
    }

    // Call on first tap — ghost freezes here (fake zone), live arrow keeps going
    public ShotZone OnFirstTap()
    {
        ghostArrow.transform.position = liveArrow.transform.position;
        ghostArrow.SetActive(true);
        return ArrowToZone(liveArrow.transform.position.x);
    }

    // Call on second tap — live arrow locks in (real shot zone)
    public ShotZone OnSecondTap()
    {
        oscillating = false;
        lastArrowX  = liveArrow != null ? liveArrow.transform.position.x : 0f;
        ShotZone zone = ArrowToZone(lastArrowX);
        StartCoroutine(ConfirmAndHide());
        return zone;
    }

    // Returns the exact x position where the arrow was locked on second tap
    public float GetLastArrowX() => lastArrowX;

    public void HideAll()
    {
        oscillating = false;
        StopAllCoroutines();
        if (liveArrow  != null) liveArrow.SetActive(false);
        if (ghostArrow != null) ghostArrow.SetActive(false);
    }

    IEnumerator ConfirmAndHide()
    {
        // Flash orange to confirm shot direction
        SetColor(liveArrow, new Color(1f, 0.35f, 0f, 1f));
        yield return new WaitForSeconds(0.4f);
        liveArrow.SetActive(false);
        ghostArrow.SetActive(false);
    }

    static ShotZone ArrowToZone(float x)
    {
        if (x < -0.6f) return ShotZone.MidLeft;
        if (x >  0.6f) return ShotZone.MidRight;
        return ShotZone.MidCenter;
    }

    void SetColor(GameObject arrow, Color c)
    {
        if (arrow == null) return;
        foreach (var sr in arrow.GetComponentsInChildren<SpriteRenderer>())
            sr.color = c;
    }

    // Arrow = shaft (rectangle) + head (triangle), visually clear upward-pointing shape
    GameObject BuildArrow(string name, Color color)
    {
        var root = new GameObject(name);
        root.transform.position = new Vector3(0, ARROW_Y, ARROW_Z);

        // Shaft — tall thin rectangle
        var shaft = MakeQuad("Shaft", root.transform, color,
            localPos:   new Vector3(0, -0.35f, 0),
            localScale: new Vector3(0.22f, 0.7f, 1f),
            sortOrder: 12);

        // Arrowhead — wide triangle on top
        var head = MakeTriQuad("Head", root.transform, color,
            localPos:   new Vector3(0,  0.22f, 0),
            localScale: new Vector3(0.72f, 0.52f, 1f),
            sortOrder: 12);

        return root;
    }

    static GameObject MakeQuad(string name, Transform parent, Color color,
        Vector3 localPos, Vector3 localScale, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeFilledSprite(color, 4, 4);
        sr.sortingOrder = sortOrder;
        return go;
    }

    static GameObject MakeTriQuad(string name, Transform parent, Color color,
        Vector3 localPos, Vector3 localScale, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeTriangleSprite(color);
        sr.sortingOrder = sortOrder;
        return go;
    }

    static Sprite MakeFilledSprite(Color color, int w, int h)
    {
        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), Mathf.Max(w, h));
    }

    // Upward triangle: wide at y=0 (bottom), tip at y=size (top)
    static Sprite MakeTriangleSprite(Color color)
    {
        int   size   = 64;
        float cx     = size / 2f;
        var   tex    = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                // Normalise y: 0 at bottom, 1 at top
                float ny        = (float)y / (size - 1);
                float halfWidth = (1f - ny) * cx;
                bool  inside    = Mathf.Abs(x - cx) < halfWidth - 0.5f;
                tex.SetPixel(x, y, inside ? color : Color.clear);
            }

        tex.Apply();
        // Pivot at bottom-center so the head sits on top of shaft
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), size);
    }
}
