using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;   // LittleSquirrel_Angry
    public SplineContainer path;     // Path
    public float spawnInterval = 3f;

    IEnumerator Start() {
        while (!GameManager.Instance.gameOver) {
            var go = Instantiate(enemyPrefab, path.EvaluatePosition(0f), Quaternion.identity);
            go.GetComponent<Enemy>().Init(path);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}