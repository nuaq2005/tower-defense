using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    public static List<Enemy> All = new List<Enemy>();

    public float speed = 2f;
    public int hp = 3;

    SplineContainer path;
    float length;
    float distance;

    public Vector3 Velocity => transform.forward * speed;

    public void Init(SplineContainer p) {
        path = p;
        length = path.CalculateLength();
    }

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update() {
        if (path == null || length <= 0.01f) return;

        distance += speed * Time.deltaTime;
        float t = distance / length;

        if (t >= 1f) {
            GameManager.Instance.LoseLife();
            Destroy(gameObject);
            return;
        }

        transform.position = path.EvaluatePosition(t);
        Vector3 forward = path.EvaluateTangent(t);
        if (forward.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(forward);
    }

    public void TakeDamage(int dmg) {
        hp -= dmg;
        Debug.Log($"{name} took {dmg} damage, hp now {hp}", this);
        if (hp <= 0) Destroy(gameObject);
    }
}