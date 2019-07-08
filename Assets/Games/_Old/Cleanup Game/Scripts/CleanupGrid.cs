using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CleanupGrid : MonoBehaviour {
    public Vector2Int numTiles;
    public float tileSize = 1;
    [Range (0, 1)]
    public float border = 0.1f;
    public Material mat;

    public Vector3[, ] grid;

    bool needsUpdate;

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

        grid = new Vector3[numTiles.x, numTiles.y];

        Vector2Int numCellsOnEitherSide = new Vector2Int ((numTiles.x - 1) / 2, (numTiles.y - 1) / 2);
        for (int y = -numCellsOnEitherSide.y; y <= numCellsOnEitherSide.y; y++) {
            for (int x = -numCellsOnEitherSide.x; x <= numCellsOnEitherSide.x; x++) {
                Vector3 centre = new Vector3 (x, 0, y) * tileSize;
                grid[x + numCellsOnEitherSide.x, y + numCellsOnEitherSide.y] = centre;

                Transform tile = GameObject.CreatePrimitive (PrimitiveType.Cube).transform;
                tile.localScale = new Vector3 (tileSize * (1 - border), .1f, tileSize * (1 - border));
                tile.position = centre;
                tile.parent = transform;

                MeshRenderer renderer = tile.gameObject.GetComponent<MeshRenderer> ();
                renderer.sharedMaterial = mat;

            }
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