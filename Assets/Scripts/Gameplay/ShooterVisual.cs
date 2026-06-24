using UnityEngine;
using System.Collections;

public class ShooterVisual : MonoBehaviour
{
    static readonly Color JERSEY    = new Color(0.82f, 0.10f, 0.10f);
    static readonly Color JERSEY2   = new Color(0.60f, 0.06f, 0.06f); // koyu şerit
    static readonly Color SHORTS    = new Color(0.88f, 0.88f, 0.88f);
    static readonly Color SKIN      = new Color(0.96f, 0.78f, 0.58f);
    static readonly Color HAIR      = new Color(0.18f, 0.10f, 0.06f);
    static readonly Color SOCK      = new Color(0.88f, 0.88f, 0.88f);
    static readonly Color BOOT      = new Color(0.08f, 0.08f, 0.08f);
    static readonly Color BOOT_KICK = new Color(0.15f, 0.15f, 0.80f);

    Transform root;
    Transform tHair, tHead, tTorso, tStripe, tShorts;
    Transform tLArm, tRArm, tLHand, tRHand;
    Transform tLThigh, tLCalf, tLSock, tLBoot;
    Transform tRThigh, tRCalf, tRSock, tRBoot;

    Coroutine runCo;
    Coroutine swaCo;
    static Sprite sBox, sDisk, sRound;

    void Awake()
    {
        if (sBox   == null) sBox   = MakeBox();
        if (sDisk  == null) sDisk  = MakeDisk(32);
        if (sRound == null) sRound = MakeRounded(32, 20, 6);
    }

    void Start()
    {
        root = new GameObject("SH_Root").transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.one * 1.6f;

        tHair   = P("SH_Hair",   HAIR,    0.28f, 0.17f, sDisk,  14);
        tHead   = P("SH_Head",   SKIN,    0.31f, 0.31f, sDisk,  13);
        tTorso  = P("SH_Torso",  JERSEY,  0.36f, 0.42f, sRound, 11);
        tStripe = P("SH_Stripe", JERSEY2, 0.10f, 0.28f, sBox,   12);
        tShorts = P("SH_Shorts", SHORTS,  0.38f, 0.17f, sRound, 12);

        tLArm   = P("SH_LArm",  JERSEY,  0.14f, 0.26f, sRound, 10);
        tRArm   = P("SH_RArm",  JERSEY,  0.14f, 0.26f, sRound, 12);
        tLHand  = P("SH_LHand", SKIN,    0.16f, 0.16f, sDisk,  10);
        tRHand  = P("SH_RHand", SKIN,    0.16f, 0.16f, sDisk,  12);

        tLThigh = P("SH_LThigh", SHORTS,  0.17f, 0.24f, sRound, 10);
        tLCalf  = P("SH_LCalf",  SKIN,    0.14f, 0.20f, sRound, 10);
        tLSock  = P("SH_LSock",  SOCK,    0.13f, 0.10f, sRound, 10);
        tLBoot  = P("SH_LBoot",  BOOT,    0.20f, 0.09f, sRound, 10);

        tRThigh = P("SH_RThigh", SHORTS,  0.17f, 0.24f, sRound, 12);
        tRCalf  = P("SH_RCalf",  SKIN,    0.14f, 0.20f, sRound, 12);
        tRSock  = P("SH_RSock",  SOCK,    0.13f, 0.10f, sRound, 12);
        tRBoot  = P("SH_RBoot",  BOOT,    0.20f, 0.09f, sRound, 12);

        SetIdle();
    }

    public void StartRunning()
    {
        if (swaCo != null) { StopCoroutine(swaCo); swaCo = null; }
        if (runCo != null) StopCoroutine(runCo);
        root.localRotation = Quaternion.identity;
        runCo = StartCoroutine(RunCycle());
    }

    public void StopRunning()
    {
        if (runCo != null) { StopCoroutine(runCo); runCo = null; }
        SetIdle();
    }

    public void PlayKick()
    {
        if (swaCo != null) { StopCoroutine(swaCo); swaCo = null; }
        if (runCo != null) { StopCoroutine(runCo); runCo = null; }
        root.localRotation = Quaternion.identity;
        StartCoroutine(KickRoutine());
    }

    public void PlayCelebrate()
    {
        if (runCo != null) { StopCoroutine(runCo); runCo = null; }
        StartCoroutine(CelebrateRoutine());
    }

    void SetIdle()
    {
        if (swaCo != null) { StopCoroutine(swaCo); swaCo = null; }
        root.localRotation = Quaternion.identity;

        Set(tHair,    0f,     0.65f,  0f);
        Set(tHead,    0f,     0.55f,  0f);
        Set(tTorso,   0f,     0.22f,  0f);
        Set(tStripe,  0f,     0.22f,  0f);
        Set(tShorts,  0f,    -0.04f,  0f);

        Set(tLArm,   -0.24f,  0.25f,  20f);
        Set(tRArm,    0.24f,  0.25f, -20f);
        Set(tLHand,  -0.20f,  0.11f,   0f);
        Set(tRHand,   0.20f,  0.11f,   0f);

        SetLeg(false,  7f, 0f);
        SetLeg(true,  -7f, 0f);

        swaCo = StartCoroutine(IdleSway());
    }

    IEnumerator IdleSway()
    {
        float phase = Random.Range(0f, Mathf.PI * 2f);
        while (true)
        {
            phase += Time.deltaTime * 1.1f;
            float sw = Mathf.Sin(phase) * 0.009f;
            root.localRotation = Quaternion.Euler(0, 0, sw * 400f);
            Set(tHead,   sw * 0.10f, 0.55f, sw * 180f);
            Set(tHair,   sw * 0.10f, 0.65f, 0f);
            Set(tTorso,  sw * 0.05f, 0.22f, sw * 250f);
            Set(tStripe, sw * 0.05f, 0.22f, sw * 250f);
            yield return null;
        }
    }

    void SetRunFrame(bool rightForward)
    {
        float s = rightForward ? 1f : -1f;

        Set(tHair,    0f,     0.65f,  s * 2f);
        Set(tHead,    0f,     0.55f,  s * 2f);
        Set(tTorso,   0f,     0.22f,  s * 4f);
        Set(tStripe,  0f,     0.22f,  s * 4f);
        Set(tShorts,  0f,    -0.04f,  s * 4f);

        Set(tLArm,   -0.24f,  0.25f, -s * 38f);
        Set(tRArm,    0.24f,  0.25f,  s * 38f);
        // El konumları kol rotasyonuyla kabaca uyuşur
        Set(tLHand,  -0.24f + s * 0.06f, 0.10f, 0f);
        Set(tRHand,   0.24f - s * 0.06f, 0.10f, 0f);

        SetLeg(!rightForward,  s * 36f, -s * 28f);
        SetLeg(rightForward,  -s * 28f,  s * 14f);
    }

    void SetKick()
    {
        Set(tHair,    0f,     0.61f, -8f);
        Set(tHead,    0f,     0.52f, -8f);
        Set(tTorso,   0f,     0.20f, -8f);
        Set(tStripe,  0f,     0.20f, -8f);
        Set(tShorts,  0f,    -0.06f, -8f);

        Set(tLArm,   -0.30f,  0.27f,  55f);
        Set(tRArm,    0.22f,  0.27f, -15f);
        Set(tLHand,  -0.20f,  0.38f,   0f); // sol el yukarıda denge
        Set(tRHand,   0.25f,  0.14f,   0f);

        // Sol bacak: denge
        SetLeg(false, 10f, 5f);

        // Sağ bacak: vuruş
        tRThigh.localPosition = new Vector3( 0.12f, -0.20f, 0);
        tRThigh.localRotation = Quaternion.Euler(0, 0, -55f);
        tRCalf.localPosition  = new Vector3( 0.28f, -0.13f, 0);
        tRCalf.localRotation  = Quaternion.Euler(0, 0, -20f);
        tRSock.localPosition  = new Vector3( 0.41f, -0.09f, 0);
        tRSock.localRotation  = Quaternion.Euler(0, 0, -10f);
        tRBoot.localPosition  = new Vector3( 0.50f, -0.06f, 0);
        tRBoot.localRotation  = Quaternion.Euler(0, 0,  10f);
        tRBoot.GetComponent<SpriteRenderer>().color = BOOT_KICK;
    }

    void SetCelebrate()
    {
        Set(tHair,    0f,     0.65f,  0f);
        Set(tHead,    0f,     0.55f,  0f);
        Set(tTorso,   0f,     0.22f,  0f);
        Set(tStripe,  0f,     0.22f,  0f);
        Set(tShorts,  0f,    -0.04f,  0f);
        Set(tLArm,   -0.22f,  0.44f, -155f);
        Set(tRArm,    0.22f,  0.44f,  155f);
        Set(tLHand,  -0.26f,  0.58f,   0f);
        Set(tRHand,   0.26f,  0.58f,   0f);
        SetLeg(false,  14f, 0f);
        SetLeg(true,  -14f, 0f);
    }

    // Bacak grubu: uyluk/baldır/çorap/bot
    void SetLeg(bool right, float thighAng, float calfDelta)
    {
        Transform th = right ? tRThigh : tLThigh;
        Transform ca = right ? tRCalf  : tLCalf;
        Transform sk = right ? tRSock  : tLSock;
        Transform bo = right ? tRBoot  : tLBoot;
        float sx = right ? 1f : -1f;

        th.localPosition = new Vector3(sx * 0.10f, -0.22f, 0);
        th.localRotation = Quaternion.Euler(0, 0, thighAng);

        float calfAng = thighAng + calfDelta;
        float thRad = thighAng * Mathf.Deg2Rad;
        Vector3 calfBase = th.localPosition + new Vector3(Mathf.Sin(-thRad) * 0.22f, -Mathf.Cos(thRad) * 0.22f, 0);
        ca.localPosition = calfBase;
        ca.localRotation = Quaternion.Euler(0, 0, calfAng);

        float caRad = calfAng * Mathf.Deg2Rad;
        Vector3 sockBase = calfBase + new Vector3(Mathf.Sin(-caRad) * 0.18f, -Mathf.Cos(caRad) * 0.18f, 0);
        sk.localPosition = sockBase;
        sk.localRotation = Quaternion.Euler(0, 0, calfAng);

        bo.localPosition = sockBase + new Vector3(Mathf.Sin(-caRad) * 0.10f, -Mathf.Cos(caRad) * 0.10f, 0);
        bo.localRotation = Quaternion.Euler(0, 0, 0);
        bo.GetComponent<SpriteRenderer>().color = BOOT;
    }

    IEnumerator RunCycle()
    {
        bool phase = false;
        while (true)
        {
            SetRunFrame(phase);
            phase = !phase;
            yield return new WaitForSeconds(0.13f);
        }
    }

    IEnumerator KickRoutine()
    {
        SetKick();
        yield return new WaitForSeconds(0.38f);
        SetIdle();
    }

    IEnumerator CelebrateRoutine()
    {
        for (int i = 0; i < 4; i++)
        {
            SetCelebrate();
            yield return new WaitForSeconds(0.20f);
            SetIdle();
            yield return new WaitForSeconds(0.15f);
        }
        SetCelebrate();
        yield return new WaitForSeconds(0.80f);
        SetIdle();
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
