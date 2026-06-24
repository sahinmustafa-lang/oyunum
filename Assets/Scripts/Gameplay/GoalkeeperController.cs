using UnityEngine;
using System.Collections;

// GoalkeeperController lives ON the "Goalkeeper" visual GameObject.
// Dive uses SmoothStep + Z-rotation so the keeper tilts like a real goalkeeper.
// After the dive the keeper slowly returns to the upright idle position.
public class GoalkeeperController : MonoBehaviour
{
    public static GoalkeeperController Instance { get; private set; }

    // Root y=1.00 → gövde merkezi y≈1.28, kale ağzında görünür konum
    static readonly Vector3 IDLE = new Vector3(0f, 1.00f, 0f);

    const float X_MIN = -3.0f, X_MAX = 3.0f;
    const float Y_MIN =  0.88f, Y_MAX = 2.30f;

    private bool     isPlayerKeeper  = false;
    private bool     waitingForClick = false;
    private bool     trackingMouse   = false;
    private ShotZone playerZone      = ShotZone.MidCenter;
    private ShotZone lastDiveZone    = ShotZone.MidCenter;

    private GameObject      cursorGO;
    private GoalkeeperVisual visual;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        transform.position = IDLE;
        transform.rotation = Quaternion.identity;
        visual   = GetComponent<GoalkeeperVisual>();
        cursorGO = BuildCursor();
        cursorGO.SetActive(false);
    }

    // ── State setters ──────────────────────────────────────────────────────

    public void SetAsOpponentKeeper()
    {
        isPlayerKeeper  = false;
        waitingForClick = false;
        trackingMouse   = false;
        StopAllCoroutines();
        StartCoroutine(SmoothReturnToIdle(0f));
        cursorGO.SetActive(false);
    }

    public void PreparePlayerKeep()
    {
        isPlayerKeeper  = true;
        waitingForClick = true;
        trackingMouse   = true;
        playerZone      = ShotZone.MidCenter;
        StopAllCoroutines();
        StartCoroutine(SmoothReturnToIdle(0f));
        cursorGO.SetActive(true);
    }

    // ── Update: cursor tracks mouse, keeper stays at center ───────────────

    void Update()
    {
        if (!isPlayerKeeper) return;

        if (trackingMouse)
        {
            Vector3 goalPos = MouseToGoalWorld();
            cursorGO.transform.position = new Vector3(goalPos.x, goalPos.y, -1f);
            playerZone = WorldToZone(goalPos);
        }

        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
            trackingMouse   = false;
        }
    }

    // Called by ShooterController exactly when the shot is kicked
    public void CommitDive()
    {
        waitingForClick = false;
        trackingMouse   = false;
        cursorGO.SetActive(false);
        DiveTo(playerZone);
    }

    // ── Diving ─────────────────────────────────────────────────────────────

    // Fake'e yumuşak reaksiyon — tam dalış değil, hafif yana yaslanma
    public void FakeReact(ShotZone fakeZone)
    {
        lastDiveZone = fakeZone;
        StopAllCoroutines();
        // Sadece %22 oranında o yöne kaymak: keskin dalış değil, hafif step
        Vector3 leanTarget = Vector3.Lerp(IDLE, ZoneToWorld(fakeZone), 0.22f);
        float leanRot      = ZoneToRotation(fakeZone) * 0.20f;
        StartCoroutine(Dive(leanTarget, leanRot, 0.22f));

        int horiz = (int)fakeZone % 3; // 0=sol, 1=merkez, 2=sağ
        if      (horiz == 0) visual?.SetLean(false);
        else if (horiz == 2) visual?.SetLean(true);
        else                 visual?.SetCrouch();
    }

    public void DiveTo(ShotZone zone)
    {
        lastDiveZone = zone;
        StopAllCoroutines();

        bool isCorner = zone == ShotZone.LowLeft  || zone == ShotZone.LowRight ||
                        zone == ShotZone.HighLeft  || zone == ShotZone.HighRight;

        if (isCorner)
            StartCoroutine(CornerDive(zone, 0.28f));
        else
        {
            StartCoroutine(Dive(ZoneToWorld(zone), ZoneToRotation(zone), 0.28f, ZoneArc(zone)));
            visual?.DiveToZone(zone);
        }
    }

    // Köşe dalışı: önce dizleri kır (0.13s), sonra parabolik tam dalış
    IEnumerator CornerDive(ShotZone zone, float dur)
    {
        visual?.SetCrouch();
        yield return new WaitForSeconds(0.13f);
        StartCoroutine(Dive(ZoneToWorld(zone), ZoneToRotation(zone), dur, ZoneArc(zone)));
        visual?.DiveToZone(zone);
    }

    public void QuickDiveTo(ShotZone zone)
    {
        lastDiveZone = zone;
        StopAllCoroutines();
        StartCoroutine(Dive(ZoneToWorld(zone), ZoneToRotation(zone), 0.13f, ZoneArc(zone) * 0.4f));
        visual?.DiveToZone(zone);
    }

    // Parabolik dalış — arc > 0 ise sin eğrisi ile yukarı yay ekler
    IEnumerator Dive(Vector3 targetPos, float targetZ, float dur, float arc = 0f)
    {
        Vector3    startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion endRot   = visual != null
            ? Quaternion.identity
            : Quaternion.Euler(0f, 0f, targetZ);

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p  = Mathf.Clamp01(t / dur);
            float sp = Mathf.SmoothStep(0f, 1f, p);
            Vector3 pos = Vector3.Lerp(startPos, targetPos, sp);
            if (arc > 0f)
                pos.y += Mathf.Sin(p * Mathf.PI) * arc; // yay: zirve ortada
            transform.position = pos;
            transform.rotation = Quaternion.Lerp(startRot, endRot, sp);
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = endRot;
    }

    // Her zone için uygun yay yüksekliği (birim)
    static float ZoneArc(ShotZone zone)
    {
        switch (zone)
        {
            case ShotZone.HighLeft:
            case ShotZone.HighRight:  return 0.45f;
            case ShotZone.HighCenter: return 0.38f;
            case ShotZone.MidLeft:
            case ShotZone.MidRight:   return 0.18f;
            default:                  return 0f;
        }
    }

    // Called by PenaltyMatchManager after the ball animation is done
    public void ReturnToIdle(float delay = 0.6f)
    {
        StartCoroutine(SmoothReturnToIdle(delay));
    }

    IEnumerator SmoothReturnToIdle(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        Vector3 startPos = transform.position;
        bool isSideDive  = (int)lastDiveZone % 3 != 1; // sol veya sağ kolondaysa

        bool isHighDive = lastDiveZone == ShotZone.HighLeft  ||
                          lastDiveZone == ShotZone.HighRight ||
                          lastDiveZone == ShotZone.HighCenter;

        if (visual != null && isSideDive)
        {
            bool right = (int)lastDiveZone % 3 == 2;
            visual.TriggerLanding(right);

            // Aşama 1 — yere düşme (0.40s)
            // Yüksek dalışlarda y ekseni yerçekimi eğrisiyle (t²) hızlanarak düşer
            // Yatay dalışlarda her iki eksen de smooth
            Vector3 groundPos = new Vector3(startPos.x * 0.35f, IDLE.y, 0f);
            float dur = 0.40f, t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p  = Mathf.Clamp01(t / dur);
                float px = Mathf.SmoothStep(0f, 1f, p);
                float py = isHighDive ? p * p : px; // yüksek: t² (yerçekimi), yatay: smooth
                transform.position = new Vector3(
                    Mathf.Lerp(startPos.x, groundPos.x, px),
                    Mathf.Lerp(startPos.y, groundPos.y, py),
                    0f);
                yield return null;
            }

            // Aşama 2 — IDLE'a yerleşme (0.50s)
            startPos = transform.position;
            dur = 0.50f; t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, IDLE, Mathf.SmoothStep(0f, 1f, t / dur));
                yield return null;
            }
        }
        else
        {
            // Merkez dalışları: anında görsel sıfırla, pozisyon lerp
            visual?.ReturnToIdle();
            Quaternion startRot = transform.rotation;
            float dur = 0.45f, t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / dur);
                transform.position = Vector3.Lerp(startPos, IDLE, p);
                transform.rotation = Quaternion.Lerp(startRot, Quaternion.identity, p);
                yield return null;
            }
        }

        transform.position = IDLE;
        transform.rotation = Quaternion.identity;
    }

    // ── Zone helpers ───────────────────────────────────────────────────────

    public void SetPlayerKeeperZone(ShotZone z) => playerZone = z;
    public ShotZone GetPlayerKeeperZone()        => playerZone;

    // Top kalecinin gövde merkezine gider (root + torso_local×scale = +0.26)
    public static Vector3 GetDiveTarget(ShotZone zone) =>
        ZoneToWorld(zone) + new Vector3(0f, 0.26f, 0f);

    static Vector3 ZoneToWorld(ShotZone zone)
    {
        switch (zone)
        {
            case ShotZone.LowLeft:    return new Vector3(-1.8f, 0.60f, 0f);
            case ShotZone.MidLeft:    return new Vector3(-2.2f, 1.00f, 0f);
            case ShotZone.HighLeft:   return new Vector3(-1.8f, 1.80f, 0f);
            case ShotZone.LowCenter:  return new Vector3( 0.0f, 0.65f, 0f);
            case ShotZone.MidCenter:  return new Vector3( 0.0f, 0.80f, 0f);
            case ShotZone.HighCenter: return new Vector3( 0.0f, 1.85f, 0f);
            case ShotZone.LowRight:   return new Vector3( 1.8f, 0.60f, 0f);
            case ShotZone.MidRight:   return new Vector3( 2.2f, 1.00f, 0f);
            case ShotZone.HighRight:  return new Vector3( 1.8f, 1.80f, 0f);
            default:                  return IDLE;
        }
    }

    // Z-rotation (degrees) per zone — positive = counter-clockwise (lean left), negative = lean right
    static float ZoneToRotation(ShotZone zone)
    {
        switch (zone)
        {
            case ShotZone.LowLeft:    return  80f;   // neredeyse yatay, sola uzanmış
            case ShotZone.MidLeft:    return  50f;   // klasik sol dalış
            case ShotZone.HighLeft:   return  25f;   // sol üst, zıplayarak
            case ShotZone.LowCenter:  return   5f;   // öne hafif eğilme
            case ShotZone.MidCenter:  return   0f;   // dik
            case ShotZone.HighCenter: return   0f;   // düz yukarı zıplama
            case ShotZone.LowRight:   return -80f;   // neredeyse yatay, sağa uzanmış
            case ShotZone.MidRight:   return -50f;   // klasik sağ dalış
            case ShotZone.HighRight:  return -25f;   // sağ üst, zıplayarak
            default:                  return   0f;
        }
    }

    static ShotZone WorldToZone(Vector3 pos)
    {
        bool left  = pos.x < -0.80f;
        bool right = pos.x >  0.80f;
        bool high  = pos.y >  1.90f;
        bool low   = pos.y <  1.15f;

        if (left)  return high ? ShotZone.HighLeft  : low ? ShotZone.LowLeft  : ShotZone.MidLeft;
        if (right) return high ? ShotZone.HighRight : low ? ShotZone.LowRight : ShotZone.MidRight;
        return     high ? ShotZone.HighCenter : low ? ShotZone.LowCenter : ShotZone.MidCenter;
    }

    public static ShotZone ScreenToZone(Vector2 screenPos)
    {
        float x = screenPos.x / Screen.width;
        if (x < 0.33f) return ShotZone.MidLeft;
        if (x > 0.66f) return ShotZone.MidRight;
        return ShotZone.MidCenter;
    }

    // ── Mouse → goal world (clamped) ──────────────────────────────────────

    static Vector3 MouseToGoalWorld()
    {
        if (Camera.main == null) return IDLE;
        Vector3 w = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        return new Vector3(
            Mathf.Clamp(w.x, X_MIN, X_MAX),
            Mathf.Clamp(w.y, Y_MIN, Y_MAX),
            0f);
    }

    // ── Cursor visual (red crosshair) ──────────────────────────────────────

    GameObject BuildCursor()
    {
        var root = new GameObject("KeeperCursor");

        var ring = new GameObject("Ring");
        ring.transform.SetParent(root.transform, false);
        var srRing = ring.AddComponent<SpriteRenderer>();
        srRing.sprite       = MakeRing(new Color(1f, 0.08f, 0.08f, 0.95f), 48, 6);
        srRing.sortingOrder = 20;
        ring.transform.localScale = Vector3.one * 0.55f;

        var dot = new GameObject("Dot");
        dot.transform.SetParent(root.transform, false);
        var srDot = dot.AddComponent<SpriteRenderer>();
        srDot.sprite       = MakeDisk(new Color(1f, 0.08f, 0.08f, 1f), 16);
        srDot.sortingOrder = 21;
        dot.transform.localScale = Vector3.one * 0.12f;

        return root;
    }

    static Sprite MakeRing(Color color, int size, int thickness)
    {
        var tex = new Texture2D(size, size);
        float cx = size / 2f, outerR = cx - 1f, innerR = cx - thickness;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cx));
                tex.SetPixel(x, y, (d < outerR && d > innerR) ? color : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
    }

    static Sprite MakeDisk(Color color, int size)
    {
        var tex = new Texture2D(size, size);
        float cx = size / 2f;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cx));
                tex.SetPixel(x, y, d < cx - 0.5f ? color : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
    }
}
