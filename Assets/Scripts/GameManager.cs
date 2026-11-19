using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public TMP_Text hudText;         // optional
    public int lives = 1;
    public int maxTowers = 5;
    public bool gameOver;

    int towersPlaced;
    public int TowersRemaining => maxTowers - towersPlaced;

    void Awake() { Instance = this; Time.timeScale = 1f; }
    void Start() { UpdateHUD(); }

    public bool TryUseTower() {
        if (gameOver || towersPlaced >= maxTowers) return false;
        towersPlaced++;
        UpdateHUD();
        return true;
    }

    public void LoseLife() {
        if (gameOver) return;
        lives--;
        UpdateHUD();
        if (lives <= 0) EndGame();
    }

    void EndGame() {
        gameOver = true;
        foreach (var e in new List<Enemy>(Enemy.All))
            Destroy(e.gameObject);
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateHUD() {
        if (hudText)
            hudText.text = $"Lives: {Mathf.Max(lives, 0)}\nTowers left: {TowersRemaining}";
    }
}