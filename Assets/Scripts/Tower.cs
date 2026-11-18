using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform head;                // optional: only this part rotates
    public float aimHeightOffset = 0.5f;

    float cooldown;

    void Update() {
        cooldown -= Time.deltaTime;

        Enemy target = FindTarget();
        if (target == null) return;

        Transform origin = firePoint != null ? firePoint : transform;
        Vector3 aimPoint = target.transform.position + Vector3.up * aimHeightOffset;

        if (head != null) {
            Vector3 toTarget = aimPoint - head.position;
            if (toTarget.sqrMagnitude > 0.001f)
                head.rotation = Quaternion.LookRotation(toTarget);
        }

        if (cooldown <= 0f) {
            Vector3 dir = (aimPoint - origin.position).normalized;
            var p = Instantiate(projectilePrefab, origin.position, Quaternion.LookRotation(dir));
            p.GetComponent<Projectile>().Launch(dir, target.transform, aimHeightOffset);
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
}