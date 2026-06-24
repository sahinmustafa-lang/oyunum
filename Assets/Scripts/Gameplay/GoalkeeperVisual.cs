using UnityEngine;

public class GoalkeeperVisual : MonoBehaviour
{
    static readonly Color JERSEY = new Color(0.08f, 0.68f, 0.08f);
    static readonly Color JERSEY2 = new Color(0.05f, 0.50f, 0.05f); // stripe
    static readonly Color SHORTS  = new Color(0.08f, 0.08f, 0.45f);
    static readonly Color SKIN    = new Color(0.96f, 0.78f, 0.58f);
    static readonly Color HAIR    = new Color(0.18f, 0.10f, 0.06f);
    static readonly Color GLOVE   = new Color(1.00f, 0.88f, 0.00f);
    static readonly Color SOCK    = new Color(0.85f, 0.85f, 0.85f);
    static readonly Color BOOT    = new Color(0.08f, 0.08f, 0.08f);

    Transform root;
    Transform tHair, tHead, tTorso, tStripe, tShorts;
    Transform tLArm, tRArm, tLGlove, tRGlove;
    Transform tLLeg, tRLeg, tLSock, tRSock, tLBoot, tRBoot;
    SpriteRenderer srLA, srRA, srLG, srRG;

    static Sprite sBox, sDisk, sRound;

    void Awake()
    {
        if (sBox   == null) sBox   = MakeBox();
        if (sDisk  == null) sDisk  = MakeDisk(32);
        if (sRound == null) sRound = MakeRounded(32, 20, 6);
    }

    void Start()
    {
        root = new GameObject("GK_Root").transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.one * 1.28f;

        // Z-order: back=5, mid=6-7, front=8-9
        tHair   = P("GK_Hair",   HAIR,    0.24f, 0.15f, sDisk,  9);
        tHead   = P("GK_Head",   SKIN,    0.26f, 0.26f, sDisk,  8);
        tTorso  = P("GK_Torso",  JERSEY,  0.30f, 0.40f, sRound, 6);
        tStripe = P("GK_Stripe", JERSEY2, 0.08f, 0.28f, sBox,   7);
        tShorts = P("GK_Shorts", SHORTS,  0.32f, 0.17f, sRound, 7);

        tLArm   = P("GK_LArm",  JERSEY,  0.11f, 0.28f, sRound, 5);
        tRArm   = P("GK_RArm",  JERSEY,  0.11f, 0.28f, sRound, 7);
        tLGlove = P("GK_LGlove",GLOVE,   0.15f, 0.15f, sDisk,  5);
        tRGlove = P("GK_RGlove",GLOVE,   0.15f, 0.15f, sDisk,  7);
        srLA = tLArm.GetComponent<SpriteRenderer>();
        srRA = tRArm.GetComponent<SpriteRenderer>();
        srLG = tLGlove.GetComponent<SpriteRenderer>();
        srRG = tRGlove.GetComponent<SpriteRenderer>();

        tLLeg  = P("GK_LLeg",  SHORTS,  0.13f, 0.24f, sRound, 5);
        tRLeg  = P("GK_RLeg",  SHORTS,  0.13f, 0.24f, sRound, 7);
        tLSock = P("GK_LSock", SOCK,    0.11f, 0.14f, sRound, 5);
        tRSock = P("GK_RSock", SOCK,    0.11f, 0.14f, sRound, 7);
        tLBoot = P("GK_LBoot", BOOT,    0.17f, 0.09f, sRound, 5);
        tRBoot = P("GK_RBoot", BOOT,    0.17f, 0.09f, sRound, 7);

        SetIdle();
    }

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

    void SetIdle()
    {
        root.localRotation = Quaternion.identity;

        Set(tHair,   0f,     0.63f,  0f);
        Set(tHead,   0f,     0.54f,  0f);
        Set(tTorso,  0f,     0.21f,  0f);
        Set(tStripe, 0f,     0.21f,  0f);
        Set(tShorts, 0f,    -0.04f,  0f);

        Set(tLArm,  -0.25f,  0.25f,  38f);
        Set(tRArm,   0.25f,  0.25f, -38f);
        Set(tLGlove,-0.15f,  0.12f,   0f);  // arm ends (idle, arms 38°)
        Set(tRGlove, 0.15f,  0.12f,   0f);

        Set(tLLeg,  -0.10f, -0.26f,   5f);
        Set(tRLeg,   0.10f, -0.26f,  -5f);
        Set(tLSock, -0.10f, -0.41f,   5f);
        Set(tRSock,  0.10f, -0.41f,  -5f);
        Set(tLBoot, -0.12f, -0.49f,   0f);
        Set(tRBoot,  0.12f, -0.49f,   0f);

        srLA.sortingOrder = 5; srRA.sortingOrder = 7;
        srLG.sortingOrder = 5; srRG.sortingOrder = 7;
    }

    void SetDive(bool right)
    {
        float s = right ? 1f : -1f;
        root.localRotation = Quaternion.Euler(0f, 0f, -s * 65f);

        Set(tHair,   0f,     0.63f,  0f);
        Set(tHead,   0f,     0.54f,  0f);
        Set(tTorso,  0f,     0.21f,  0f);
        Set(tStripe, 0f,     0.21f,  0f);
        Set(tShorts, 0f,    -0.04f,  0f);

        // Uzanan kol + eldiven
        Transform ext    = right ? tRArm   : tLArm;
        Transform tuck   = right ? tLArm   : tRArm;
        Transform extG   = right ? tRGlove : tLGlove;
        Transform tuckG  = right ? tLGlove : tRGlove;

        Set(ext,   s * 0.42f,  0.34f, -s * 18f);
        Set(tuck, -s * 0.08f,  0.30f,  s * 72f);
        Set(extG,  s * 0.48f,  0.19f,   0f);   // uzanan elin ucu
        Set(tuckG,-s * 0.06f,  0.26f,   0f);   // bükülü elin ucu

        Set(tLLeg,  -0.10f, -0.26f,  10f);
        Set(tRLeg,   0.10f, -0.26f, -10f);
        Set(tLSock, -0.10f, -0.41f,  10f);
        Set(tRSock,  0.10f, -0.41f, -10f);
        Set(tLBoot, -0.11f, -0.49f,   0f);
        Set(tRBoot,  0.11f, -0.49f,   0f);

        srLA.sortingOrder = right ? 5 : 7;
        srRA.sortingOrder = right ? 7 : 5;
        srLG.sortingOrder = right ? 5 : 7;
        srRG.sortingOrder = right ? 7 : 5;
    }

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
            {
                float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(cx, cx));
                tex.SetPixel(x, y, d < cx - .4f ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * .5f, s);
    }

    // Yuvarlatılmış köşeli dikdörtgen — daha organik görünüm
    static Sprite MakeRounded(int w, int h, int r)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                int cx = Mathf.Clamp(x, r, w - r - 1);
                int cy = Mathf.Clamp(y, r, h - r - 1);
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), w);
    }
}
