using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

// Sahne görünümünü Flash Footie stilinde oluşturur.
public class PenaltyMatchVisuals : MonoBehaviour
{
    void Awake()
    {
        // Sahnedeki eski nesneleri temizle — bunlar kod tarafından yeniden oluşturulur
        foreach (string old in new[] { "FieldBackground", "GoalBG", "GoalPostLeft",
                                       "GoalPostRight", "GoalCrossbar", "Goal" })
        {
            var go = GameObject.Find(old);
            if (go != null) DestroyImmediate(go);
        }

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0f, 0.4f, -10f);
            Camera.main.backgroundColor = new Color(0.10f, 0.42f, 0.20f);
            Camera.main.clearFlags      = CameraClearFlags.SolidColor;
        }

        BuildStadiumBackground();
        BuildField();
        BuildGoal();
        BuildFieldMarkings();
        SetupKeeper();
        SetupShooter();
        SetupBall();
        SetupNet();
        SetupAnchors();
    }

    void Start()
    {
        StartCoroutine(LoadTribuneSprite());
        StartCoroutine(LoadAdBoardSprite());
        StartCoroutine(LoadFieldGoalSprite());
        RepositionHUD();
    }

    // Penaltı ikonları + tüm HUD yazıları → sağ alt köşe
    void RepositionHUD()
    {
        // Sağ-alt anchor, sağ-alt pivot: pozisyon = sağdan/alttan piksel mesafesi (negatif x)
        // Sıralama: en altta PlayerIcons, üzerinde OpponentIcons, üzerinde skor/talimat
        MoveUI("PlayerIcons",    ax: 1, ay: 0, px: -10, py:  15, w: 200, h: 30);
        MoveUI("OpponentIcons",  ax: 1, ay: 0, px: -10, py:  52, w: 200, h: 30);
        MoveUI("ScoreText",      ax: 1, ay: 0, px: -10, py:  90, w: 130, h: 38, align: TextAnchor.MiddleRight);
        MoveUI("InstructionText",ax: 1, ay: 0, px: -10, py: 135, w: 380, h: 40, align: TextAnchor.MiddleRight);
        MoveUI("RoundText",      ax: 1, ay: 0, px: -10, py: 180, w: 260, h: 36, align: TextAnchor.MiddleRight);
        // Takım adı etiketleri
        MoveUI("PlayerTeamText",   ax: 1, ay: 0, px: -10, py: 220, w: 260, h: 34, align: TextAnchor.MiddleRight);
        MoveUI("OpponentTeamText", ax: 1, ay: 0, px: -10, py: 258, w: 260, h: 34, align: TextAnchor.MiddleRight);
        // Sonuç metni ortada kalabilir — sadece küçültelim gereksizse
    }

    void MoveUI(string goName, float ax, float ay, float px, float py,
                float w, float h, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var go = GameObject.Find(goName);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot     = new Vector2(1f, 0f);  // sağ-alt pivot
        rt.anchoredPosition = new Vector2(px, py);
        rt.sizeDelta        = new Vector2(w, h);
        var txt = go.GetComponent<Text>();
        if (txt != null) txt.alignment = align;
    }

    IEnumerator LoadTribuneSprite()
    {
        yield return null;
        var sr = LoadPhoto("Sprites/tribune.png", "TribuneSprite");
        if (sr == null) yield break;
        // w=24 → landscape kamerada da kenar grilik kalmaz
        PlacePhoto(sr, y: 5.10f, w: 24f, h: 2.10f, sort: -6);
    }

    IEnumerator LoadAdBoardSprite()
    {
        yield return null;
        var sr = LoadPhoto("Sprites/adboard.png", "AdBoardSprite");
        if (sr == null) yield break;
        // Büyük ve geniş — kiriş ile tribün arasını tam kapla
        PlacePhoto(sr, y: 3.62f, w: 24f, h: 0.80f, sort: -6);
    }

    IEnumerator LoadFieldGoalSprite()
    {
        yield return null;
        // field_bg.png = ağ silinmiş; goal_net.png GoalNet.cs tarafından ayrıca yüklenir
        var sr = LoadPhoto("Sprites/field_bg.png", "FieldGoalSprite");
        if (sr == null) sr = LoadPhoto("Sprites/fieldgoal.png", "FieldGoalSprite");
        if (sr == null) yield break;

        // Saha alt (y=-5) → kiriş (y=3.22) arası: merkez=-0.89, yükseklik=8.22
        // Fotoğraf bu alanı kaplar; kiriş direği kod tarafından üstüne çizilir
        const float fgY = -0.89f, fgH = 8.22f, fgW = 20f;
        PlacePhoto(sr, y: fgY, w: fgW, h: fgH, sort: -7);
        // sort -7: GoalBG (-9) ve net çizgileri (-8) arkada kalır, direkler (-5) önde
    }

    SpriteRenderer LoadPhoto(string relativePath, string goName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (!File.Exists(path)) return null;
        byte[] data = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(data);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        var go = new GameObject(goName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        return sr;
    }

    static void PlacePhoto(SpriteRenderer sr, float y, float w, float h, int sort)
    {
        var tex = sr.sprite.texture;
        float natW = tex.width  / 100f;
        float natH = tex.height / 100f;
        sr.transform.position   = new Vector3(0, y, 0);
        sr.transform.localScale = new Vector3(w / natW, h / natH, 1f);
        sr.sortingOrder = sort;
    }

    // ── Stadyum arka planı ─────────────────────────────────────────────────────

    void BuildStadiumBackground()
    {
        // Tribün / reklam panosu fotoğraflar kaplıyor; sadece doldurma arka planı.
        // 30 birim geniş → her kamera boyutunda kenar bırakmaz.
        Rect("Sky", new Color(0.12f, 0.14f, 0.18f),
             new Vector3(0, 5.0f, 0), 30f, 10f, -13);
    }

    // ── Zemin ──────────────────────────────────────────────────────────────────

    void BuildField()
    {
        // Fotoğraf (fieldgoal.png) sahayı kapsıyor — sadece boşluk doldurucu arka plan
        const float fieldTop = 3.22f, fieldBot = -5.0f;
        float fH  = fieldTop - fieldBot;
        float fCY = (fieldTop + fieldBot) * 0.5f;
        Rect("FieldBackground", new Color(0.08f, 0.10f, 0.08f),
             new Vector3(0, fCY, 0), 30f, fH, -11);
    }

    // ── Kale ───────────────────────────────────────────────────────────────────

    void BuildGoal()
    {
        // Kale tamamen fotoğraftan (fieldgoal.png) geliyor — kod tarafından hiçbir şey çizilmiyor.
    }

    // ── Çizgiler ───────────────────────────────────────────────────────────────

    void BuildFieldMarkings()
    {
        // Tüm çizgiler kaldırıldı — fotoğraf kale ve sahayı kapsıyor
    }

    // ── Karakterler ────────────────────────────────────────────────────────────

    void SetupKeeper()
    {
        var go = Ensure("Goalkeeper", new Vector3(0, 1.6f, 0));
        if (!go.TryGetComponent<GoalkeeperController>(out _)) go.AddComponent<GoalkeeperController>();
        if (!go.TryGetComponent<GoalkeeperVisual>(out _))     go.AddComponent<GoalkeeperVisual>();
    }

    void SetupShooter()
    {
        var go = Ensure("Shooter", new Vector3(-0.4f, -2.8f, 0));
        if (!go.TryGetComponent<ShooterController>(out _)) go.AddComponent<ShooterController>();
        if (!go.TryGetComponent<ShooterVisual>(out _))     go.AddComponent<ShooterVisual>();
    }

    void SetupBall()
    {
        var go = Ensure("Ball", new Vector3(0, -1.45f, 0));
        go.transform.localScale = new Vector3(0.34f, 0.34f, 1f);
        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        sr.sprite = FootballSprite();
        sr.color  = Color.white;
        sr.sortingOrder = 3;
    }

    static Sprite FootballSprite()
    {
        const int S = 128;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float cx = S / 2f, R = cx - 1.5f;

        // Sol-üstten gelen ışık vektörü (normalize)
        var light = new Vector3(-0.45f, 0.55f, 0.70f);
        float lLen = Mathf.Sqrt(light.x*light.x + light.y*light.y + light.z*light.z);
        light = new Vector3(light.x/lLen, light.y/lLen, light.z/lLen);

        // Küresel gölgeli beyaz zemin
        for (int x = 0; x < S; x++)
        {
            for (int y = 0; y < S; y++)
            {
                float dx = x - cx, dy = y - cx;
                float dist = Mathf.Sqrt(dx*dx + dy*dy);
                float edge = R - dist;
                if (edge < 0f) { tex.SetPixel(x, y, Color.clear); continue; }

                float nx = dx / R, ny = dy / R;
                float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx*nx - ny*ny));
                float diffuse = Mathf.Clamp01(nx*light.x + ny*light.y + nz*light.z);
                // Specular: (h · n)^24 ; h = half(light, view(0,0,1))
                float hx = light.x, hy = light.y, hz = (light.z + 1f);
                float hL = Mathf.Sqrt(hx*hx + hy*hy + hz*hz);
                float spec = Mathf.Pow(Mathf.Clamp01((hx/hL)*nx + (hy/hL)*ny + (hz/hL)*nz), 24f);
                float b = Mathf.Clamp01(0.50f + 0.40f*diffuse + 0.55f*spec);

                // Yumuşak kenar anti-alias
                float alpha = Mathf.Clamp01(edge * 2f);
                tex.SetPixel(x, y, new Color(b, b, b, alpha));
            }
        }

        // Beşgen yamalar — klasik siyah-beyaz futbol topu deseni
        // Merkez beşgen
        FbPentagon(tex, cx, cx, 14f, R, -Mathf.PI/2f, light);
        // 5 orta beşgen
        for (int i = 0; i < 5; i++)
        {
            float a = i * 72f * Mathf.Deg2Rad - Mathf.PI/2f;
            FbPentagon(tex, cx + Mathf.Cos(a)*28f, cx + Mathf.Sin(a)*28f, 12f, R, a, light);
        }
        // 5 dış beşgen (kenar yamaları)
        for (int i = 0; i < 5; i++)
        {
            float a = i * 72f * Mathf.Deg2Rad - Mathf.PI/2f + 36f*Mathf.Deg2Rad;
            FbPentagon(tex, cx + Mathf.Cos(a)*50f, cx + Mathf.Sin(a)*50f, 11f, R, a, light);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
    }

    static void FbPentagon(Texture2D tex, float px, float py, float pr, float ballR, float rot, Vector3 light)
    {
        float cx = tex.width / 2f;
        // Beşgen köşeleri
        var verts = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            float a = rot + i * 72f * Mathf.Deg2Rad;
            verts[i] = new Vector2(px + Mathf.Cos(a)*pr, py + Mathf.Sin(a)*pr);
        }

        int x0 = Mathf.Max(0, (int)(px - pr - 2));
        int x1 = Mathf.Min(tex.width,  (int)(px + pr + 2));
        int y0 = Mathf.Max(0, (int)(py - pr - 2));
        int y1 = Mathf.Min(tex.height, (int)(py + pr + 2));

        for (int x = x0; x < x1; x++)
        {
            for (int y = y0; y < y1; y++)
            {
                float dx = x - cx, dy = y - cx;
                if (dx*dx + dy*dy >= ballR*ballR) continue;
                if (!FbPointInPoly(new Vector2(x, y), verts)) continue;

                // Siyah yama üzerinde hafif küresel gölge
                float nx = dx / ballR, ny = dy / ballR;
                float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx*nx - ny*ny));
                float diff = Mathf.Clamp01(nx*light.x + ny*light.y + nz*light.z);
                float b = 0.02f + 0.10f * diff;
                tex.SetPixel(x, y, new Color(b, b, b, 1f));
            }
        }
    }

    static bool FbPointInPoly(Vector2 p, Vector2[] v)
    {
        bool inside = false;
        int j = v.Length - 1;
        for (int i = 0; i < v.Length; i++)
        {
            if (((v[i].y > p.y) != (v[j].y > p.y)) &&
                p.x < (v[j].x - v[i].x) * (p.y - v[i].y) / (v[j].y - v[i].y) + v[i].x)
                inside = !inside;
            j = i;
        }
        return inside;
    }

    void SetupNet()
    {
        var existing = GameObject.Find("GoalNet");
        if (existing == null)
        {
            var go = new GameObject("GoalNet");
            go.AddComponent<GoalNet>();
        }
    }

    // ── Oyun konumlandırma noktaları ───────────────────────────────────────────

    void SetupAnchors()
    {
        // Fotoğraftaki kale: x ±3.6, kiriş y=2.52, kale çizgisi y=0.88, merkez y=1.70
        Anchor("PenaltySpot",   new Vector3( 0f,    -1.45f, 0));
        Anchor("RunUpPosition", new Vector3(-0.4f,  -2.80f, 0));
        Anchor("GoalCenter",    new Vector3( 0f,     1.70f, 0));
        Anchor("MissTarget",    new Vector3( 4.5f,   1.70f, 0));

        Anchor("KP_Center",    new Vector3( 0f,    1.35f, 0));
        Anchor("KP_Left",      new Vector3(-2.8f,  1.35f, 0));
        Anchor("KP_Right",     new Vector3( 2.8f,  1.35f, 0));
        Anchor("KP_HighLeft",  new Vector3(-2.5f,  2.10f, 0));
        Anchor("KP_HighRight", new Vector3( 2.5f,  2.10f, 0));
        Anchor("KP_LowLeft",   new Vector3(-2.5f,  1.00f, 0));
        Anchor("KP_LowRight",  new Vector3( 2.5f,  1.00f, 0));
    }

    // ── Yardımcı metodlar ──────────────────────────────────────────────────────

    // Yatay/dikey çizgi (pozisyon merkezi)
    void Line(string n, float cx, float cy, float w, float h, Color c, int sort = 0)
        => Rect(n, c, new Vector3(cx, cy, 0), w, h, sort);

    // Eksen hizalı dikdörtgen
    void Rect(string n, Color c, Vector3 pos, float w, float h, int sort)
    {
        var go = new GameObject(n);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(w, h, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite();
        sr.color  = c;
        sr.sortingOrder = sort;
    }

    // Çember/disk
    void Circle(string n, Color c, float r, Vector3 pos, int sort)
    {
        var go = new GameObject(n);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(r * 2, r * 2, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = DiskSprite(32);
        sr.color  = c;
        sr.sortingOrder = sort;
    }

    // İki nokta arasında dolu dikdörtgen çizgi
    void DrawDiag(string n, Vector3 from, Vector3 to, float w, Color c, int sort)
    {
        Vector3 dir = to - from;
        var go = new GameObject(n);
        go.transform.position   = (from + to) * 0.5f;
        go.transform.rotation   = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        go.transform.localScale = new Vector3(dir.magnitude, w, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite();
        sr.color  = c;
        sr.sortingOrder = sort;
    }

    // Çember yayı (açı: standart matematik, 0=sağ, derece cinsinden)
    void DrawArc(string name, Vector3 center, float r, float startDeg, float endDeg, int segs, Color c, float thickness)
    {
        float step = (endDeg - startDeg) / segs;
        for (int i = 0; i < segs; i++)
        {
            float a1 = (startDeg + i * step) * Mathf.Deg2Rad;
            float a2 = (startDeg + (i + 1) * step) * Mathf.Deg2Rad;
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1) * r, Mathf.Sin(a1) * r, 0);
            Vector3 p2 = center + new Vector3(Mathf.Cos(a2) * r, Mathf.Sin(a2) * r, 0);
            Vector3 d  = p2 - p1;
            var go = new GameObject($"{name}_{i}");
            go.transform.position   = (p1 + p2) * 0.5f;
            go.transform.rotation   = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            go.transform.localScale = new Vector3(d.magnitude + 0.01f, thickness, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WhiteSprite();
            sr.color  = c;
            sr.sortingOrder = 0;
        }
    }

    static GameObject Ensure(string n, Vector3 pos)
    {
        var go = GameObject.Find(n);
        if (go == null) { go = new GameObject(n); go.transform.position = pos; }
        return go;
    }

    static void Anchor(string n, Vector3 pos)
    {
        var go = GameObject.Find(n) ?? new GameObject(n);
        go.transform.position = pos;
    }

    static Sprite WhiteSprite()
    {
        var tex = new Texture2D(2, 2) { filterMode = FilterMode.Point };
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f, 2f);
    }

    static Sprite DiskSprite(int sz)
    {
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        float cx = sz * 0.5f;
        for (int x = 0; x < sz; x++)
            for (int y = 0; y < sz; y++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(cx, cx)) < cx - .5f
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), Vector2.one * 0.5f, sz);
    }
}
