using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacer : MonoBehaviour
{
    public GameObject towerPrefab;
    public LayerMask mapMask;
    public Camera cam;

    void Start() { if (cam == null) cam = Camera.main; }

    void Update() {
        if (GameManager.Instance.gameOver) return;
        if (Mouse.current == null || cam == null || towerPrefab == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, mapMask)) {
                if (GameManager.Instance.TryUseTower())
                    Instantiate(towerPrefab, hit.point, Quaternion.identity);
                else
                    Debug.Log("No towers left!");
            }
        }
    }
}