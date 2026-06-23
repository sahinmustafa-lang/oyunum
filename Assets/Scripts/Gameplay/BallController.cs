using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    [Header("References (auto-found at runtime)")]
    public Transform ballTransform;
    public Transform penaltySpot;
    public Transform goalCenter;
    public Transform missTarget;

    static readonly Vector3 DEFAULT_SPOT = new Vector3(0f,  -1.45f, 0f);
    static readonly Vector3 DEFAULT_MISS = new Vector3(3.2f, 0.5f,  0f);

    void Start()
    {
        if (ballTransform == null) ballTransform = FindT("Ball");
        if (penaltySpot   == null) penaltySpot   = FindT("PenaltySpot");
        if (goalCenter    == null) goalCenter     = FindT("GoalCenter");
        if (missTarget    == null) missTarget     = FindT("MissTarget");
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

        // Top penaltı noktasından hedef noktaya gider
        yield return MoveBall(start, target, 0.40f);

        if (outcome == ShotOutcome.Save)
        {
            // Kurtarışta top kaleciden sekip geri düşer
            yield return new WaitForSeconds(0.05f);
            Vector3 bounce1 = GetBounceTarget(target);
            yield return MoveBall(target, bounce1, 0.22f);

            // İkinci, küçük sekiş — daha kısa ve yana doğru
            Vector3 bounce2 = bounce1 + new Vector3(Random.Range(-0.5f, 0.5f), -0.3f, 0f);
            bounce2.y = Mathf.Max(bounce2.y, -1.8f);
            yield return MoveBall(bounce1, bounce2, 0.18f);

            yield return new WaitForSeconds(0.4f);
        }
        else
        {
            yield return new WaitForSeconds(0.7f);
        }

        ResetBall();
    }

    // Top kaleciden sektiğinde düştüğü nokta
    static Vector3 GetBounceTarget(Vector3 savePos)
    {
        // Kurtarış noktasından aşağı + biraz yana sekme
        float sideDir  = savePos.x >= 0 ? 1f : -1f;
        float bounceX  = savePos.x + sideDir * Random.Range(0.4f, 1.0f);
        float bounceY  = Mathf.Max(-1.0f, savePos.y - Random.Range(1.2f, 2.0f));
        return new Vector3(bounceX, bounceY, 0f);
    }

    IEnumerator MoveBall(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Hafif arc efekti — orta noktada biraz yükselir
            float arc = Mathf.Sin(t * Mathf.PI) * 0.15f;
            ballTransform.position = Vector3.Lerp(from, to, t) + new Vector3(0, arc, 0);
            // Top kendi etrafında döner (sekme hissi)
            ballTransform.Rotate(0f, 0f, -360f * Time.deltaTime / duration);
            yield return null;
        }
        ballTransform.position = to;
    }

    Vector3 GetTarget(ShotOutcome outcome, ShotZone zone, int wideMissSide = 0)
    {
        if (outcome == ShotOutcome.Miss)
        {
            if (wideMissSide != 0)
            {
                // Yan kaçış: direğin yanından çıkar (sola veya sağa)
                float x = wideMissSide < 0 ? -3.6f : 3.6f;
                float y = Random.Range(2.2f, 3.0f);  // yan direk hizasında
                x += wideMissSide * Random.Range(0f, 0.4f);
                return new Vector3(x, y, 0f);
            }
            // Over-bar: yukarıdan çıkar, yönüne göre (sol/orta/sağ)
            int col = ZoneColumn(zone);
            float bx = col == 0 ? -2.4f : col == 2 ? 2.4f : 0f;
            float by = 3.8f + Random.Range(0f, 0.5f);
            bx += Random.Range(-0.25f, 0.25f);
            return new Vector3(bx, by, 0f);
        }

        // Goal / Save / Post — ball goes toward the targeted zone position
        Vector3 zonePos = GoalIndicator.GetZonePosition(zone);
        return zonePos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.05f, 0.05f), 0);
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
        ballTransform.position = penaltySpot != null ? penaltySpot.position : DEFAULT_SPOT;
    }
}
