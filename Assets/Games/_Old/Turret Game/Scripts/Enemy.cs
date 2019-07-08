using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TurretGame {
    public class Enemy : MonoBehaviour {

        public const float timeBetweenMoves = 1;

        Path path;
        int pathIndex;

        public void Init (Path path) {
            this.path = path;

            StartCoroutine (Move ());
        }

        IEnumerator Move () {
            transform.position = path[0];

            for (int i = 1; i < path.Length; i++) {
                yield return new WaitForSeconds (timeBetweenMoves);
                transform.position = path[i];
            }
        }
    }
}