using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public bool homing = true;
    public float lifetime = 5f;
    public float hitRadius = 1f;   // how close counts as a hit

    Vector3 direction;
    Transform target;
    float aimOffset;

    public void Launch(Vector3 dir, Transform t, float offset, float projectileSpeed, int dmg) {
        direction = dir;
        target = t;
        aimOffset = offset;
        speed = projectileSpeed;
        damage = dmg;
        Destroy(gameObject, lifetime);
    }

    void Update() {
        if (homing && target != null) {
            direction = (target.position + Vector3.up * aimOffset - transform.position).normalized;
            transform.forward = direction;
        }
        transform.position += direction * speed * Time.deltaTime;

        // Distance-based hit check: no colliders, tags or Rigidbody needed
        foreach (var e in Enemy.All) {
            Vector3 center = e.transform.position + Vector3.up * aimOffset;
            if (Vector3.Distance(transform.position, center) <= hitRadius) {
                e.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        foreach (var e in Big_Enemy.All) {
            Vector3 center = e.transform.position + Vector3.up * aimOffset;
            if (Vector3.Distance(transform.position, center) <= hitRadius) {
                e.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
    }
}