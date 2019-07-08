using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TurretGame {

    [ExecuteInEditMode]
    public class TurretGrid : MonoBehaviour {
        public Vector2Int numTiles;
        public float tileSize = 1;
        [Range (0, 1)]
        public float border = 0.1f;

        public Material activeMat;
        public Material inactiveMat;
        bool needsUpdate;

        public List<Path> paths;

        void Awake () {
            Generate ();
        }

        void Update () {
            if (!Application.isPlaying && needsUpdate) {
                needsUpdate = false;
                Generate ();
            }
        }

        void Generate () {
            DestroyChildren ();

            List<Vector3> activeTiles = new List<Vector3> ();

            Vector3 topLeft = -(Vector2) numTiles / 2 * tileSize;
            Vector2Int numCellsOnEitherSide = new Vector2Int ((numTiles.x - 1) / 2, (numTiles.y - 1) / 2);
            for (int y = -numCellsOnEitherSide.y; y <= numCellsOnEitherSide.y; y++) {
                for (int x = -numCellsOnEitherSide.x; x <= numCellsOnEitherSide.x; x++) {
                    Vector3 centre = new Vector3 (x, 0, y) * tileSize;

                    Transform tile = GameObject.CreatePrimitive (PrimitiveType.Cube).transform;
                    tile.localScale = new Vector3 (tileSize * (1 - border), .1f, tileSize * (1 - border));
                    tile.position = centre;
                    tile.parent = transform;

                    MeshRenderer renderer = tile.gameObject.GetComponent<MeshRenderer> ();
                    if (x == 0 || y == 0 || Mathf.Abs (x) == Mathf.Abs (y)) {
                        renderer.sharedMaterial = activeMat;
                        if (x != 0 || y != 0) {
                            activeTiles.Add (centre);
                        }
                    } else {
                        renderer.sharedMaterial = inactiveMat;
                    }
                }
            }

            var tilesByDirection = new Dictionary<Vector2Int, List<Vector3>> ();
            var keys = new List<Vector2Int> ();
            foreach (Vector3 tile in activeTiles) {
                Vector2Int dir = new Vector2Int (System.Math.Sign (tile.x), System.Math.Sign (tile.z));
                if (!tilesByDirection.ContainsKey (dir)) {
                    keys.Add (dir);
                    tilesByDirection.Add (dir, new List<Vector3> ());
                    tilesByDirection[dir].Add (Vector3.zero);
                }
                tilesByDirection[dir].Add (tile);
            }

            paths = new List<Path> ();
            foreach (var key in keys) {
                var path = tilesByDirection[key];
                path.Sort ((a, b) => (b.sqrMagnitude.CompareTo (a.sqrMagnitude)));
                paths.Add (new Path () { points = path });
            }
        }

        void DestroyChildren () {
            int num = transform.childCount;
            for (int i = num - 1; i >= 0; i--) {
                DestroyImmediate (transform.GetChild (i).gameObject, false);
            }
        }

        void OnValidate () {
            if (numTiles.x % 2 == 0) {
                numTiles.x++;
            }
            if (numTiles.y % 2 == 0) {
                numTiles.y++;
            }

            needsUpdate = true;
        }
    }

    public class Path {
        public List<Vector3> points;

        public Vector3 this [int i] {
            get {
                return points[i];
            }
        }

        public int Length {
            get {
                return points.Count;
            }
        }
    }

}