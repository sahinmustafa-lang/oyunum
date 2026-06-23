using UnityEngine;

// Prosedürel eklem tabanlı kaleci figürü.
// GoalkeeperController bu bileşenin bulunduğu GO'yu hareket ettirir;
// bu bileşen sadece eklemlerin rotasyon/pozisyonunu ayarlar.
public class GoalkeeperVisual : MonoBehaviour
{
    static readonly Color JERSEY = new Color(0.08f, 0.70f, 0.08f);
    static readonly Color SHORTS = new Color(0.05f, 0.05f, 0.40f);
    static readonly Color SKIN   = new Color(0.98f, 0.80f, 0.62f);

    Transform root;
    Transform tHead, tTorso, tShorts;
    Transform tLArm, tRArm;
    Transform tLLeg, tRLeg;
    SpriteRenderer srLA, srRA;

    static Sprite sBox, sDisk;

    void Awake()
    {
        if (sBox  == null) sBox  = MakeBox();
        if (sDisk == null) sDisk = MakeDisk(32);
    }

    void Start()
    {
        root = new GameObject("GK_Root").transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.one * 1.1f;

        tHead   = P("GK_Head",   SKIN,   0.26f, 0.26f, sDisk, 8);
        tTorso  = P("GK_Torso",  JERSEY, 0.32f, 0.42f, sBox,  6);
        tShorts = P("GK_Shorts", SHORTS, 0.34f, 0.18f, sBox,  7);
        tLArm   = P("GK_LArm",  JERSEY, 0.12f, 0.32f, sBox,  5);
        tRArm   = P("GK_RArm",  JERSEY, 0.12f, 0.32f, sBox,  7);
        tLLeg   = P("GK_LLeg",  SHORTS, 0.14f, 0.30f, sBox,  5);
        tRLeg   = P("GK_RLeg",  SHORTS, 0.14f, 0.30f, sBox,  7);
        srLA    = tLArm.GetComponent<SpriteRenderer>();
        srRA    = tRArm.GetComponent<SpriteRenderer>();

        SetIdle();
    }

    // ── Herkese açık API ───────────────────────────────────────────────────────

    public void ReturnToIdle() => SetIdle();
    public void ShowSad()      => SetIdle();

    public void DiveToZone(ShotZone zone)
    {
        switch (zone)
        {
            case ShotZone.LowLeft:
            case ShotZone.MidLeft:
            case ShotZone.HighLeft:  SetDive(false); return;
            case ShotZone.LowRight:
            case ShotZone.MidRight:
            case ShotZone.HighRight: SetDive(true);  return;
            default: SetIdle(); return;
        }
    }

    // ── Pozlar ────────────────────────────────────────────────────────────────

    void SetIdle()
    {
        root.localRotation = Quaternion.identity;
        Set(tHead,   0f,      0.54f,   0f);
        Set(tTorso,  0f,      0.20f,   0f);
        Set(tShorts, 0f,     -0.05f,   0f);
        // Kollar yana açık (kaleci hazır duruşu)
        Set(tLArm,  -0.26f,   0.24f,  40f);
        Set(tRArm,   0.26f,   0.24f, -40f);
        Set(tLLeg,  -0.11f,  -0.28f,   6f);
        Set(tRLeg,   0.11f,  -0.28f,  -6f);
        srLA.sortingOrder = 5;
        srRA.sortingOrder = 7;
    }

    void SetDive(bool right)
    {
        float s = right ? 1f : -1f;
        // Tüm vücut yana yatar
        root.localRotation = Quaternion.Euler(0f, 0f, -s * 65f);

        Set(tHead,   0f,     0.54f,  0f);
        Set(tTorso,  0f,     0.20f,  0f);
        Set(tShorts, 0f,    -0.05f,  0f);

        // Uzanan kol dive yönünde, bükülen kol geri
        Transform ext  = right ? tRArm : tLArm;
        Transform tuck = right ? tLArm : tRArm;
        Set(ext,   s * 0.42f,  0.34f, -s * 18f);
        Set(tuck, -s * 0.08f,  0.30f,  s * 72f);

        Set(tLLeg, -0.11f, -0.28f,  12f);
        Set(tRLeg,  0.11f, -0.28f, -12f);

        srLA.sortingOrder = right ? 5 : 7;
        srRA.sortingOrder = right ? 7 : 5;
    }

    // ── Yardımcı metodlar ─────────────────────────────────────────────────────

    static void Set(Transform t, float x, float y, float deg)
    {
        t.localPosition = new Vector3(x, y, 0);
        t.localRotation = Quaternion.Euler(0, 0, deg);
    }

    Transform P(string n, Color c, float w, float h, Sprite sp, int sort)
    {
        var go = new GameObject(n);
        go.transform.SetParent(root, false);
        go.transform.localScale = new Vector3(w, h, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.color = c; sr.sortingOrder = sort;
        return go.transform;
    }

    static Sprite MakeBox()
    {
        var tex = new Texture2D(2, 2) { filterMode = FilterMode.Point };
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * .5f, 2f);
    }

    static Sprite MakeDisk(int s)
    {
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float cx = s * .5f;
        for (int x = 0; x < s; x++)
            for (int y = 0; y < s; y++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(cx, cx)) < cx - .4f
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * .5f, s);
    }
}
