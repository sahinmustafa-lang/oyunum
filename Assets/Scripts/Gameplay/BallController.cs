using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    [Header("References (auto-found at runtime)")]
    public Transform ballTransform;
    public Transform penaltySpot;
    public Transform goalCenter;
    public Transform missTarget;

    static readonly Vector3 DEFAULT_SPOT = new Vector3(0f, -1.45f, 0f);

    Transform      shadowTransform;
    SpriteRenderer shadowRenderer;

    void Start()
    {
        if (ballTransform == null) ballTransform = FindT("Ball");
        if (penaltySpot   == null) penaltySpot   = FindT("PenaltySpot");
        if (goalCenter    == null) goalCenter     = FindT("GoalCenter");
        if (missTarget    == null) missTarget     = FindT("MissTarget");

        BuildShadow();
    }

    void BuildShadow()
    {
        var go = new GameObject("BallShadow");
        shadowTransform = go.transform;
        shadowRenderer  = go.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite       = MakeShadowSprite();
        shadowRenderer.sortingOrder = 2;   // topun (3) hemen altında
        shadowRenderer.color        = new Color(0f, 0f, 0f, 0f);

        Vector3 spot = penaltySpot != null ? penaltySpot.position : DEFAULT_SPOT;
        shadowTransform.position   = new Vector3(spot.x, spot.y - 0.04f, 0f);
        shadowTransform.localScale = new Vector3(0.42f, 0.12f, 1f);
    }

    static Sprite MakeShadowSprite()
    {
        const int W = 64, H = 20;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float cx = W / 2f, cy = H / 2f;
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float d  = dx * dx + dy * dy;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, d < 1f ? (1f - d) * 0.65f : 0f));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), W);
    }

    static Transform FindT(string n) { var g = GameObject.Find(n); return g != null ? g.transform : null; }

    public void AnimateBall(ShotOutcome outcome, bool isPlayerShot, ShotZone targetZone, int wideMissSide = 0)
    {
        StartCoroutine(BallRoutine(outcome, targetZone, wideMissSide));
    }

    IEnumerator BallRoutine(ShotOutcome outcome, ShotZone targetZone, int wideMissSide = 0)
    {
        if (ballTransform == null) yield break;

        Vector3 start  = penaltySpot != null ? penaltySpot.position : DEFAULT_SPOT;
        Vector3 target = GetTarget(outcome, targetZone, wideMissSide);

        yield return MoveBall(start, target, 0.40f, arcAmount: 0.18f);

        if (outcome == ShotOutcome.Goal)
        {
            // Ağa çarptıktan sonra: yavaşlayan iki sekiş, kale içinde durur
            Vector3 b1 = target + new Vector3(Random.Range(-0.20f, 0.20f), -0.45f, 0f);
            b1.y = Mathf.Max(b1.y, 0.90f);
            yield return MoveBall(target, b1, 0.20f, arcAmount: 0.06f, spin: 0.4f);

            Vector3 b2 = b1 + new Vector3(Random.Range(-0.10f, 0.10f), -0.18f, 0f);
            b2.y = Mathf.Max(b2.y, 0.85f);
            yield return MoveBall(b1, b2, 0.15f, arcAmount: 0.02f, spin: 0.2f);

            yield return new WaitForSeconds(0.55f);
        }
        else if (outcome == ShotOutcome.Save)
        {
            yield return new WaitForSeconds(0.04f);
            Vector3 sv1 = GetSaveBounce(target);
            yield return MoveBall(target, sv1, 0.22f, arcAmount: 0.08f);

            Vector3 sv2 = sv1 + new Vector3(Random.Range(-0.4f, 0.4f), -0.25f, 0f);
            sv2.y = Mathf.Max(sv2.y, -1.8f);
            yield return MoveBall(sv1, sv2, 0.16f, arcAmount: 0.04f, spin: 0.3f);

            yield return new WaitForSeconds(0.35f);
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        ResetBall();
    }

    static Vector3 GetSaveBounce(Vector3 savePos)
    {
        float side = savePos.x >= 0 ? 1f : -1f;
        return new Vector3(
            savePos.x + side * Random.Range(0.4f, 1.0f),
            Mathf.Max(-1.0f, savePos.y - Random.Range(1.2f, 2.0f)), 0f);
    }

    // arcAmount: uçuş sırasındaki yay yüksekliği; spin: dönme hızı çarpanı (1=normal)
    IEnumerator MoveBall(Vector3 from, Vector3 to, float duration,
                         float arcAmount = 0.15f, float spin = 1f)
    {
        float groundY = DEFAULT_SPOT.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t   = Mathf.Clamp01(elapsed / duration);
            float arc = Mathf.Sin(t * Mathf.PI) * arcAmount;

            Vector3 pos = Vector3.Lerp(from, to, t) + new Vector3(0f, arc, 0f);
            ballTransform.position = pos;
            ballTransform.Rotate(0f, 0f, -280f * spin * Time.deltaTime / duration);

            // Gölge güncelle
            UpdateShadow(pos.x, pos.y + arc, groundY);

            yield return null;
        }

        ballTransform.position = to;
        UpdateShadow(to.x, to.y, groundY);
    }

    void UpdateShadow(float ballX, float ballY, float groundY)
    {
        if (shadowTransform == null) return;

        float height      = Mathf.Max(0f, ballY - groundY);
        float heightFactor = 1f - Mathf.Clamp01(height / 3.8f);

        // Gölge topu izler (x), ama her zaman yerde (y = ground)
        shadowTransform.position = new Vector3(ballX, groundY - 0.04f, 0f);

        float sc = Mathf.Lerp(0.18f, 0.44f, heightFactor);
        shadowTransform.localScale = new Vector3(sc, sc * 0.27f, 1f);

        var col = shadowRenderer.color;
        col.a = 0.55f * heightFactor;
        shadowRenderer.color = col;
    }

    Vector3 GetTarget(ShotOutcome outcome, ShotZone zone, int wideMissSide = 0)
    {
        if (outcome == ShotOutcome.Miss)
        {
            if (wideMissSide != 0)
            {
                float x = wideMissSide < 0 ? -3.6f : 3.6f;
                float y = Random.Range(2.2f, 3.0f);
                x += wideMissSide * Random.Range(0f, 0.4f);
                return new Vector3(x, y, 0f);
            }
            int col = ZoneColumn(zone);
            float bx = col == 0 ? -2.4f : col == 2 ? 2.4f : 0f;
            float by = 3.8f + Random.Range(0f, 0.5f);
            bx += Random.Range(-0.25f, 0.25f);
            return new Vector3(bx, by, 0f);
        }
        Vector3 zonePos = GoalIndicator.GetZonePosition(zone);
        return zonePos + new Vector3(Random.Range(-0.10f, 0.10f), Random.Range(-0.05f, 0.05f), 0f);
    }

    static int ZoneColumn(ShotZone z)
    {
        int i = (int)z;
        if (i <= 2) return 0;
        if (i <= 5) return 1;
        return 2;
    }

    void ResetBall()
    {
        if (ballTransform == null) return;
        Vector3 spot = penaltySpot != null ? penaltySpot.position : DEFAULT_SPOT;
        ballTransform.position = spot;
        ballTransform.rotation = Quaternion.identity;
        UpdateShadow(spot.x, spot.y, DEFAULT_SPOT.y);
    }
}
