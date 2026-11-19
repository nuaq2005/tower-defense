using UnityEngine;

public class Castle : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform head;                // optional: only this part rotates
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

        Enemy smallTarget = FindTarget();
        Big_Enemy bigTarget = FindBigTarget();

        if (smallTarget != null) target = smallTarget.transform;
        if (bigTarget != null)   target = bigTarget.transform;   // big enemy takes priority

        if (target == null) return;

        Transform origin = firePoint != null ? firePoint : transform;
        Vector3 aimPoint = target.position + Vector3.up * aimHeightOffset;

        if (head != null) {
            Vector3 toTarget = aimPoint - head.position;
            if (toTarget.sqrMagnitude > 0.001f)
                head.rotation = Quaternion.LookRotation(toTarget);
        }

        if (cooldown <= 0f) {
            Vector3 dir = (aimPoint - origin.position).normalized;
            var p = Instantiate(projectilePrefab, origin.position, Quaternion.LookRotation(dir));
            p.GetComponent<Projectile>().Launch(dir, target, aimHeightOffset, projectileSpeed, damage);
            cooldown = 1f / fireRate;
        }
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