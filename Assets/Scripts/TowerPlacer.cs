using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public GameObject[] towerPrefab; //[0] = tower1, [1] = tower2, etc
    public string[] towerNames = {"Tower", "Castle"}; // Names for the towers
    public LayerMask mapMask;
    public Camera cam;
    public TMPro.TextMeshProUGUI instructionText;

    [Header("Placement rules")]
    public SplineContainer path;          // same spline the EnemySpawner uses
    public float pathClearance = 1.5f;    // min distance from the path
    public float minTowerSpacing = 3f;  // min distance between towers

    [Header("Placement ghost")]
    public Material ghostMaterial;       
    public Color validColor = new Color(0f, 1f, 0f, 0.5f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    int selected = 0;
    GameObject ghost;
    Material ghostMat;
    readonly List<Vector3> placed = new List<Vector3>();

    void Start() { 
        if (cam == null) cam = Camera.main; 
        RebuildGhost();
        UpdateText();
    }

    void Update() {
        if (GameManager.Instance.gameOver) {
            if (ghost) ghost.SetActive(false);
            return;
        }
        if (Mouse.current == null || Keyboard.current == null || cam == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectTower(0);
        
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectTower(1);

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit = default;
        bool onMap = !overUI && Physics.Raycast(ray, out hit, 500f, mapMask);

        if (ghost != null) {
            ghost.SetActive(onMap);
            if (onMap) {
                ghost.transform.position = hit.point;
                ghostMat.color = IsValidPlacement(hit.point) ? validColor : invalidColor;
            }
    }

        if (onMap && Mouse.current.leftButton.wasPressedThisFrame) {
            if (!IsValidPlacement(hit.point)) {
                Debug.Log("Can't place a tower here!");
                return;
            }
            if (GameManager.Instance.TryUseTower()) {
                Instantiate(towerPrefab[selected], hit.point, Quaternion.identity);
                placed.Add(hit.point);
            } else {
                Debug.Log("No towers left!");
            }
        }
    }
    void UpdateText() {
        if (instructionText == null) return;
        string name = selected < towerNames.Length ? towerNames[selected] : "Tower";
        instructionText.text = $"Click a tower button below to choose one\nLeft-click the map to place it\nGreen = valid, red = blocked\nSelected: {name}\nTowers left: {GameManager.Instance.TowersRemaining}";
    }

    bool IsValidPlacement(Vector3 point) {
        // Too close to the path?
        if (path != null) {
            // GetNearestPoint works in the spline's local space
            float3 local = path.transform.InverseTransformPoint(point);
            SplineUtility.GetNearestPoint(path.Spline, local, out float3 nearestLocal, out float _);
            Vector3 nearestWorld = path.transform.TransformPoint(nearestLocal);
            if (Vector3.Distance(point, nearestWorld) < pathClearance) return false;
        }

        // Too close to another tower
        foreach (var p in placed) {
            if (Vector3.Distance(point, p) < minTowerSpacing) return false;
        }

        return true;
    }

    void RebuildGhost() {
        if (ghost != null) Destroy(ghost);
        if (ghostMaterial == null) return;

        ghost = Instantiate(towerPrefab[selected]);

        // Stop the ghost from shooting, targeting, etc.
        foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        // Colliders would interfere with raycasts
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) Destroy(c);

        // Swap every material for one shared translucent material
        ghostMat = new Material(ghostMaterial);
        foreach (var r in ghost.GetComponentsInChildren<Renderer>()) {
            var mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = ghostMat;
            r.sharedMaterials = mats;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        ghost.SetActive(false);

    }

    public void SelectTower(int index) {
        if (index < 0 || index >= towerPrefab.Length) return;
        selected = index;
        RebuildGhost();
        UpdateText();
    }

}
