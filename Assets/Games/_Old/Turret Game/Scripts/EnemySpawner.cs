using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TurretGame {

    public class EnemySpawner : MonoBehaviour {

        public Enemy enemyPrefab;
        TurretGrid grid;

        void Start () {
            grid = FindObjectOfType<TurretGrid> ();

            StartCoroutine (Spawn ());
        }

        IEnumerator Spawn () {
            for (int i = 0; i < 10; i++) {
                var path = grid.paths[Random.Range (0, grid.paths.Count)];

                Enemy enemy = Instantiate (enemyPrefab, path[0], Quaternion.identity);
                enemy.Init (path);
                yield return new WaitForSeconds (Enemy.timeBetweenMoves);
            }
        }
    }

}