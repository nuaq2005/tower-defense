using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacer : MonoBehaviour
{
    public GameObject[] towerPrefab; //[0] = tower1, [1] = tower2, etc
    public string[] towerNames = {"Tower", "Castle"}; // Names for the towers
    public LayerMask mapMask;
    public Camera cam;
    public TMPro.TextMeshProUGUI instructionText;

    int selected = 0;
    void Start() { 
        if (cam == null) cam = Camera.main; 
        UpdateText();
    }

    void Update() {
        if (GameManager.Instance.gameOver) return;
        if (Mouse.current == null || Keyboard.current == null || cam == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) { selected = 0; UpdateText(); }
        if (Keyboard.current.digit2Key.wasPressedThisFrame && towerPrefab.Length > 1) { selected = 1; UpdateText(); }

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, mapMask)) {
                if (GameManager.Instance.TryUseTower())
                    Instantiate(towerPrefab[selected], hit.point, Quaternion.identity);
                else
                    Debug.Log("No towers left!");
            }
        }
    }
    void UpdateText() {
        if (instructionText == null) return;
        string name = selected < towerNames.Length ? towerNames[selected] : "Tower";
        instructionText.text =
            $"Press 1 or 2 to choose a tower\nLeft-click the map to place it\nSelected: {name}\nTowers left: {GameManager.Instance.TowersRemaining}";
    }
}
