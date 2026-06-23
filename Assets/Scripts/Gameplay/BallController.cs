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

        float duration = 0.45f;
        float elapsed  = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ballTransform.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        ballTransform.position = target;

        yield return new WaitForSeconds(0.7f);
        ResetBall();
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
