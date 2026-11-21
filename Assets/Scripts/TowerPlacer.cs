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
    public LayerMask mapMask;             // GROUND ONLY (no trees, rocks, towers)
    public Camera cam;
    public TMPro.TextMeshProUGUI instructionText;

    [Header("Placement rules")]
    public SplineContainer path;          // same spline the EnemySpawner uses
    public float pathClearance = 1.5f;    // min distance from the path
    public float extraSpacing = 0.25f;    // extra gap between tower footprints (on top of their sizes)
    public float footprintScale = 1f;     // tweak if the auto-detected footprint is too big/small

    [Header("Obstacles (trees, rocks, props)")]
    public LayerMask obstacleMask;        // layer(s) holding trees/props. Must have colliders.
    public float obstacleCheckHeight = 2f; // how tall the "no obstacles" zone above the point is

    [Header("Placement ghost")]
    public Material ghostMaterial;
    public Color validColor = new Color(0f, 1f, 0f, 0.5f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    struct PlacedTower { public Vector3 pos; public float radius; }

    int selected = 0;
    GameObject ghost;
    Material ghostMat;
    float selectedRadius = 0.5f;          // footprint radius of the selected tower
    readonly List<PlacedTower> placed = new List<PlacedTower>();

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
                if (ghostMat != null)
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
                placed.Add(new PlacedTower { pos = hit.point, radius = selectedRadius });
                UpdateText();
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
        // 1) Too close to the path?
        if (path != null) {
            // GetNearestPoint works in the spline's local space
            float3 local = path.transform.InverseTransformPoint(point);
            SplineUtility.GetNearestPoint(path.Spline, local, out float3 nearestLocal, out float _);
            Vector3 nearestWorld = path.transform.TransformPoint(nearestLocal);
            if (Vector3.Distance(point, nearestWorld) < pathClearance) return false;
        }

        // 2) Overlapping a tree / rock / other prop?
        if (obstacleMask.value != 0) {
            Vector3 bottom = point + Vector3.up * selectedRadius;
            Vector3 top = point + Vector3.up * Mathf.Max(selectedRadius, obstacleCheckHeight);
            if (Physics.CheckCapsule(bottom, top, selectedRadius, obstacleMask, QueryTriggerInteraction.Ignore))
                return false;
        }

        // 3) Footprints overlapping another tower/castle?
        foreach (var p in placed) {
            if (FlatDist(point, p.pos) < selectedRadius + p.radius + extraSpacing) return false;
        }

        return true;
    }

    static float FlatDist(Vector3 a, Vector3 b) {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // Footprint radius = half the widest horizontal extent of the tower's renderers
    float ComputeRadius(GameObject go) {
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return 0.5f;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return Mathf.Max(b.extents.x, b.extents.z) * footprintScale;
    }

    void RebuildGhost() {
        if (ghost != null) Destroy(ghost);
        ghost = null;

        ghost = Instantiate(towerPrefab[selected]);   // Awake has run, so heightScale is applied

        // Measure the footprint while the ghost is active at its spawn position
        selectedRadius = ComputeRadius(ghost);

        // Stop the ghost from shooting, targeting, etc.
        foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        // Colliders would interfere with raycasts
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) Destroy(c);

        // Swap every material for one shared translucent material
        if (ghostMaterial != null) {
            ghostMat = new Material(ghostMaterial);
            foreach (var r in ghost.GetComponentsInChildren<Renderer>()) {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = ghostMat;
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
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