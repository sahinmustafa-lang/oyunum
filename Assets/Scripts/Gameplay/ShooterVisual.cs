using UnityEngine;
using System.Collections;

// Prosedürel eklem tabanlı şutör figürü.
// ShooterController bu GO'yu hareket ettirir; bu bileşen sadece eklemleri günceller.
public class ShooterVisual : MonoBehaviour
{
    static readonly Color JERSEY    = new Color(0.85f, 0.10f, 0.10f);
    static readonly Color SHORTS    = new Color(0.90f, 0.90f, 0.90f);
    static readonly Color SKIN      = new Color(0.98f, 0.80f, 0.62f);
    static readonly Color SOCK      = new Color(0.90f, 0.90f, 0.90f);
    static readonly Color BOOT      = new Color(0.10f, 0.10f, 0.10f);
    static readonly Color BOOT_KICK = new Color(0.20f, 0.20f, 0.80f);

    Transform root;
    Transform tHead, tTorso, tShorts;
    Transform tLArm, tRArm;
    Transform tLThigh, tLCalf, tLBoot;
    Transform tRThigh, tRCalf, tRBoot;

    Coroutine runCo;

    static Sprite sBox, sDisk;

    void Awake()
    {
        if (sBox  == null) sBox  = MakeBox();
        if (sDisk == null) sDisk = MakeDisk(32);
    }

    void Start()
    {
        root = new GameObject("SH_Root").transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.one * 1.4f;

        tHead   = P("SH_Head",   SKIN,   0.28f, 0.28f, sDisk, 13);
        tTorso  = P("SH_Torso",  JERSEY, 0.32f, 0.44f, sBox,  11);
        tShorts = P("SH_Shorts", SHORTS, 0.34f, 0.18f, sBox,  12);

        tLArm   = P("SH_LArm",   JERSEY, 0.12f, 0.30f, sBox,  10);
        tRArm   = P("SH_RArm",   JERSEY, 0.12f, 0.30f, sBox,  12);

        tLThigh = P("SH_LThigh", SHORTS, 0.15f, 0.26f, sBox,  10);
        tLCalf  = P("SH_LCalf",  SOCK,   0.12f, 0.24f, sBox,  10);
        tLBoot  = P("SH_LBoot",  BOOT,   0.17f, 0.11f, sBox,  10);

        tRThigh = P("SH_RThigh", SHORTS, 0.15f, 0.26f, sBox,  12);
        tRCalf  = P("SH_RCalf",  SOCK,   0.12f, 0.24f, sBox,  12);
        tRBoot  = P("SH_RBoot",  BOOT,   0.17f, 0.11f, sBox,  12);

        SetIdle();
    }

    // ── Herkese açık API ───────────────────────────────────────────────────────

    public void StartRunning()
    {
        if (runCo != null) StopCoroutine(runCo);
        runCo = StartCoroutine(RunCycle());
    }

    public void StopRunning()
    {
        if (runCo != null) { StopCoroutine(runCo); runCo = null; }
        SetIdle();
    }

    public void PlayKick()
    {
        if (runCo != null) { StopCoroutine(runCo); runCo = null; }
        StartCoroutine(KickRoutine());
    }

    public void PlayCelebrate()
    {
        if (runCo != null) { StopCoroutine(runCo); runCo = null; }
        StartCoroutine(CelebrateRoutine());
    }

    // ── Pozlar ────────────────────────────────────────────────────────────────

    void SetIdle()
    {
        root.localRotation = Quaternion.identity;

        Set(tHead,    0f,     0.56f,  0f);
        Set(tTorso,   0f,     0.22f,  0f);
        Set(tShorts,  0f,    -0.04f,  0f);
        Set(tLArm,   -0.25f,  0.24f, 20f);
        Set(tRArm,    0.25f,  0.24f,-20f);

        SetLeg(false,  8f,  0f, 0f);
        SetLeg(true,  -8f,  0f, 0f);
    }

    // frame: false=sol öne, true=sağ öne
    void SetRunFrame(bool rightForward)
    {
        float s = rightForward ? 1f : -1f;

        Set(tHead,   0f,     0.56f,   s * 3f);
        Set(tTorso,  0f,     0.22f,   s * 5f);
        Set(tShorts, 0f,    -0.04f,   s * 5f);
        Set(tLArm,  -0.25f,  0.24f,  -s * 40f);
        Set(tRArm,   0.25f,  0.24f,   s * 40f);

        // Öne bacak: uyluk öne, baldır geri (diz açısı)
        SetLeg(!rightForward,  s * 38f, -s * 30f, 0f);
        // Arka bacak: uyluk geri, düz
        SetLeg(rightForward,  -s * 30f,  s * 15f, 0f);
    }

    void SetKick()
    {
        // Vücut hafif öne eğik, sağ bacak yukarı vuruş
        Set(tHead,    0f,     0.52f, -8f);
        Set(tTorso,   0f,     0.20f, -8f);
        Set(tShorts,  0f,    -0.06f, -8f);
        Set(tLArm,   -0.30f,  0.26f, 55f);
        Set(tRArm,    0.22f,  0.26f,-15f);

        // Sol bacak: denge, hafif bükülü
        SetLeg(false, 10f, 5f, 0f);
        // Sağ bacak: vuruş — uyluk yukarı öne, baldır ileri uzanmış
        tRThigh.localPosition = new Vector3( 0.12f, -0.20f, 0);
        tRThigh.localRotation = Quaternion.Euler(0, 0, -55f);
        tRCalf.localPosition  = new Vector3( 0.28f, -0.12f, 0);
        tRCalf.localRotation  = Quaternion.Euler(0, 0, -20f);
        tRBoot.localPosition  = new Vector3( 0.42f, -0.06f, 0);
        tRBoot.localRotation  = Quaternion.Euler(0, 0,  10f);
        tRBoot.GetComponent<SpriteRenderer>().color = BOOT_KICK;
    }

    void SetCelebrate()
    {
        Set(tHead,    0f,     0.56f,  0f);
        Set(tTorso,   0f,     0.22f,  0f);
        Set(tShorts,  0f,    -0.04f,  0f);
        // Her iki kol yukarı
        Set(tLArm,   -0.22f,  0.42f, -160f);
        Set(tRArm,    0.22f,  0.42f,  160f);
        SetLeg(false,  15f, 0f, 0f);
        SetLeg(true,  -15f, 0f, 0f);
    }

    // Bacak üçlüsünü (uyluk / baldır / çizme) ayarla
    // right=true → sağ bacak, thighAng = uyluk açısı (+ = saat yönü)
    void SetLeg(bool right, float thighAng, float calfDelta, float bootAng)
    {
        Transform th = right ? tRThigh : tLThigh;
        Transform ca = right ? tRCalf  : tLCalf;
        Transform bo = right ? tRBoot  : tLBoot;
        float sx = right ? 1f : -1f;

        float ty = -0.22f;

        th.localPosition = new Vector3(sx * 0.11f, ty, 0);
        th.localRotation = Quaternion.Euler(0, 0, thighAng);

        float calfAng = thighAng + calfDelta;
        float thRad = thighAng * Mathf.Deg2Rad;
        Vector3 calfBase = th.localPosition + new Vector3(Mathf.Sin(-thRad) * 0.22f, -Mathf.Cos(thRad) * 0.22f, 0);
        ca.localPosition = calfBase;
        ca.localRotation = Quaternion.Euler(0, 0, calfAng);

        float caRad = calfAng * Mathf.Deg2Rad;
        bo.localPosition = calfBase + new Vector3(Mathf.Sin(-caRad) * 0.20f, -Mathf.Cos(caRad) * 0.20f, 0);
        bo.localRotation = Quaternion.Euler(0, 0, bootAng);
        bo.GetComponent<SpriteRenderer>().color = BOOT;
    }

    // ── Coroutine animasyonlar ─────────────────────────────────────────────────

    IEnumerator RunCycle()
    {
        bool phase = false;
        while (true)
        {
            SetRunFrame(phase);
            phase = !phase;
            yield return new WaitForSeconds(0.14f);
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
