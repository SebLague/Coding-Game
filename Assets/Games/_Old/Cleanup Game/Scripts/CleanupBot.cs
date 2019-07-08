using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanupBot : MonoBehaviour {

    public enum Command { Left, Right, Up, Down };
    public Vector2Int coord { get; private set; }

    Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
    Vector3[, ] worldGrid;
    float tileSize;

    public void Init (Vector3[, ] worldGrid, Vector2Int startCoord) {
        this.worldGrid = worldGrid;
        this.coord = startCoord;
        this.tileSize = Mathf.Abs (worldGrid[1, 0].x - worldGrid[0, 0].x);

        transform.position = worldGrid[coord.x, coord.y];
    }

    public void ExecuteCommand (Command command, System.Action callback) {
        StartCoroutine (Move (command, callback));
    }

    IEnumerator Move (Command command, System.Action callback) {

        Vector2Int direction = directions[(int) command];
        Vector2Int targetCoord = coord + direction;
        Vector3 targetPos = transform.position + new Vector3 (direction.x, 0, direction.y) * tileSize;
        Vector3 startPos = transform.position;

        float t = 0;
        while (t <= 1) {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp (startPos, targetPos, t);
            yield return null;
        }

        callback ();
    }

}