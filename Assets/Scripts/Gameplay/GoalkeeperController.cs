using UnityEngine;
using System.Collections;

// GoalkeeperController lives ON the "Goalkeeper" visual GameObject.
// Dive uses SmoothStep + Z-rotation so the keeper tilts like a real goalkeeper.
// After the dive the keeper slowly returns to the upright idle position.
public class GoalkeeperController : MonoBehaviour
{
    public static GoalkeeperController Instance { get; private set; }

    // Kale çizgisi y=0.88; kalecinin gövde merkezi kale çizgisinde
    static readonly Vector3 IDLE = new Vector3(0f, 0.88f, 0f);

    // Fotoğraftaki kale: direkler ±3.6, kiriş y=2.52
    const float X_MIN = -3.0f, X_MAX = 3.0f;
    const float Y_MIN =  0.70f, Y_MAX = 2.30f;

    private bool     isPlayerKeeper  = false;
    private bool     waitingForClick = false;
    private bool     trackingMouse   = false;
    private ShotZone playerZone      = ShotZone.MidCenter;

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

    public void DiveTo(ShotZone zone)
    {
        StopAllCoroutines();
        StartCoroutine(Dive(ZoneToWorld(zone), ZoneToRotation(zone), 0.28f));
        visual?.DiveToZone(zone);
    }

    public void QuickDiveTo(ShotZone zone)
    {
        StopAllCoroutines();
        StartCoroutine(Dive(ZoneToWorld(zone), ZoneToRotation(zone), 0.13f));
        visual?.DiveToZone(zone);
    }

    // Smooth dive to target. Sprite visual kendi poz animasyonunu içerdiğinden
    // GoalkeeperVisual varken GO'yu döndürmüyoruz — sadece pozisyon değişir.
    IEnumerator Dive(Vector3 targetPos, float targetZ, float dur)
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
            float p = Mathf.SmoothStep(0f, 1f, t / dur);
            transform.position = Vector3.Lerp(startPos, targetPos, p);
            transform.rotation = Quaternion.Lerp(startRot, endRot, p);
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = endRot;
    }

    // Called by PenaltyMatchManager after the ball animation is done
    public void ReturnToIdle(float delay = 0.6f)
    {
        StartCoroutine(SmoothReturnToIdle(delay));
    }

    IEnumerator SmoothReturnToIdle(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        visual?.ReturnToIdle();

        Vector3    startPos = transform.position;
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
        transform.position = IDLE;
        transform.rotation = Quaternion.identity;
    }

    // ── Zone helpers ───────────────────────────────────────────────────────

    public void SetPlayerKeeperZone(ShotZone z) => playerZone = z;
    public ShotZone GetPlayerKeeperZone()        => playerZone;

    // World positions for each zone (posts at ±2.6)
    static Vector3 ZoneToWorld(ShotZone zone)
    {
        // Root pozisyonu + 65° dönüş ≈ 0.55 birim ek uzanma.
        // Direk x=±3.6 → root max ±2.8 → toplam ≈ ±3.35, kale içinde kalır.
        switch (zone)
        {
            case ShotZone.LowLeft:    return new Vector3(-1.8f, 0.80f, 0f);
            case ShotZone.MidLeft:    return new Vector3(-2.1f, 0.88f, 0f);
            case ShotZone.HighLeft:   return new Vector3(-1.8f, 1.85f, 0f);
            case ShotZone.LowCenter:  return new Vector3( 0.0f, 0.80f, 0f);
            case ShotZone.MidCenter:  return IDLE;
            case ShotZone.HighCenter: return new Vector3( 0.0f, 1.90f, 0f);
            case ShotZone.LowRight:   return new Vector3( 1.8f, 0.80f, 0f);
            case ShotZone.MidRight:   return new Vector3( 2.1f, 0.88f, 0f);
            case ShotZone.HighRight:  return new Vector3( 1.8f, 1.85f, 0f);
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
