using UnityEngine;

public class Castle : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float aimHeightOffset = 0.5f;
    public float projectileSpeed = 20f;
    public int damage = 3;              // how much damage each projectile does
    public float heightScale = 1f;   // tower height multiplier (1 = prefab's normal size)

    float cooldown;

    void Awake() {
        transform.localScale *= heightScale; //height multiplied
    }

    void Update() {
        cooldown -= Time.deltaTime;

        Transform target = null;
        Vector3 targetVel = Vector3.zero;

        Enemy smallTarget = FindTarget();
        Big_Enemy bigTarget = FindBigTarget();

        if (smallTarget != null) { target = smallTarget.transform; targetVel = smallTarget.Velocity; }
        if (bigTarget != null)   { target = bigTarget.transform;   targetVel = bigTarget.Velocity; }   // big enemy takes priority

        if (target == null) return;

        Transform origin = firePoint != null ? firePoint : transform;

        // Predicted intercept point based on the target's current position and velocity
        Vector3 aimPoint = PredictAimPoint(origin.position,
                                           target.position + Vector3.up * aimHeightOffset,
                                           targetVel,
                                           projectileSpeed);

        if (cooldown <= 0f) {
            Vector3 dir = (aimPoint - origin.position).normalized;
            var p = Instantiate(projectilePrefab, origin.position, Quaternion.LookRotation(dir));
            p.GetComponent<Projectile>().Launch(dir, target, aimHeightOffset, projectileSpeed, damage);
            cooldown = 1f / fireRate;
        }
    }

    // Finds where a projectile fired from 'origin' at 'projSpeed' will meet a target
    // at 'targetPos' moving with 'targetVel'. Falls back to the current position if
    // no intercept is possible.
    Vector3 PredictAimPoint(Vector3 origin, Vector3 targetPos, Vector3 targetVel, float projSpeed) {
        Vector3 d = targetPos - origin;
        float a = Vector3.Dot(targetVel, targetVel) - projSpeed * projSpeed;
        float b = 2f * Vector3.Dot(d, targetVel);
        float c = Vector3.Dot(d, d);

        float t;
        if (Mathf.Abs(a) < 0.0001f) {
            if (Mathf.Abs(b) < 0.0001f) return targetPos;
            t = -c / b;
        } else {
            float disc = b * b - 4f * a * c;
            if (disc < 0f) return targetPos;          // no intercept possible
            float sq = Mathf.Sqrt(disc);
            float t1 = (-b - sq) / (2f * a);
            float t2 = (-b + sq) / (2f * a);
            t = Mathf.Min(t1 > 0f ? t1 : float.MaxValue, t2 > 0f ? t2 : float.MaxValue);
        }
        if (t <= 0f || t == float.MaxValue) return targetPos;
        return targetPos + targetVel * t;
    }

    Enemy FindTarget() {
        Enemy best = null;
        float bestDist = range;
        foreach (var e in Enemy.All) {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d <= bestDist) { best = e; bestDist = d; }
        }
        return best;
    }

    Big_Enemy FindBigTarget() {
        Big_Enemy best = null;
        float bestDist = range;
        foreach (var e in Big_Enemy.All) {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d <= bestDist) { best = e; bestDist = d; }
        }
        return best;
    }
}