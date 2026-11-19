using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;    // LittleSquirrel_Angry
    public GameObject bigEnemyPrefab; // BigCat_Angry
    public SplineContainer path;
    public float spawnInterval = 3f;
    public int enemiesPerWave = 10;

    public bool waitForWaveClear = true;  // wait until wave 1 is dead or has left the path
    public float waveDelay = 5f;          // extra pause before wave 2 starts

    IEnumerator Start() {
        int spawned = 0;
        int totalToSpawn = enemiesPerWave * 2;

        while (!GameManager.Instance.gameOver && spawned < totalToSpawn) {

            // Wave boundary: pause before the big enemies begin
            if (spawned == enemiesPerWave) {
                if (waitForWaveClear) {
                    while (Enemy.All.Count > 0 && !GameManager.Instance.gameOver)
                        yield return null;
                }
                yield return new WaitForSeconds(waveDelay);
                if (GameManager.Instance.gameOver) yield break;
            }

            bool big = spawned >= enemiesPerWave;
            GameObject prefab = big ? bigEnemyPrefab : enemyPrefab;

            var go = Instantiate(prefab, path.EvaluatePosition(0f), Quaternion.identity);
            if (big) go.GetComponent<Big_Enemy>().Init(path);
            else     go.GetComponent<Enemy>().Init(path);

            spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}