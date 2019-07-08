using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FibonacciTask : Task {

    public Transform blockPrefab;
    public float animSpeed = 1;
    Transform currentBlock;
    int fibIndex = 1;
    int sideIndex;

    Bounds2D bounds;
    List<Transform> blocks;
    List<float> sizes;
    public Color[] cols;

    protected override void Run (string code) {
        base.Run (code);
    }

    protected override void StartNextTestIteration () {
        base.StartNextTestIteration ();
        print (fibIndex);
        float[] inputs = { fibIndex };
        GenerateOutputs (inputs);
        float size = 0;
        if (outputQueue.Count > 0) {
            size = outputQueue.Dequeue ().value;
        }

        StartCoroutine (AnimateNext (size));
    }

    IEnumerator AnimateNext (float size) {
        // Spawn block
        int expectedSize = Fib (fibIndex);
        float angle = sideIndex * 90;
        currentBlock = Instantiate (blockPrefab);
        currentBlock.localScale = new Vector3 (size, size, 1);
        currentBlock.eulerAngles = Vector3.forward * angle;

        Vector2 centre = bounds.Centre;
        if (sideIndex == 0 || sideIndex == 2) {

            centre = bounds.Centre + Vector2.down * (bounds.HalfSize.y + size / 2f) * ((sideIndex == 0) ? 1 : -1);
        } else if (sideIndex == 1 || sideIndex == 3) {
            centre = bounds.Centre + Vector2.right * (bounds.HalfSize.x + size / 2f) * ((sideIndex == 1) ? 1 : -1);
        }
        currentBlock.position = centre;
        bounds.AddSquare (centre, size);
        blocks.Add (currentBlock);
        sizes.Add (size);

        sideIndex++;
        sideIndex %= 4;
        fibIndex++;

        float animPercent = 0;
        while (animPercent < 1) {

            animPercent += Time.deltaTime * animSpeed;
            currentBlock.GetChild (0).localScale = new Vector3 (1, Mathf.Clamp01 (animPercent), 1);
            var mat = blocks[blocks.Count - 1].GetComponentInChildren<MeshRenderer> ().material;
            mat.color = Color.Lerp (Color.black, cols[(blocks.Count - 1) % cols.Length], animPercent);
            yield return null;
        }

        if (Mathf.Abs (size - expectedSize) < 0.01f) {
            TestPassed ();
        } else {
            TestFailed ();
        }
    }

    void OnDrawGizmos () {
        if (bounds != null) {
            //Gizmos.color = Color.green;
            //Gizmos.DrawWireCube (bounds.Centre, bounds.HalfSize * 2);
        }
    }

    protected override void Clear () {
        base.Clear ();
        if (blocks != null) {
            for (int i = blocks.Count - 1; i >= 0; i--) {
                Destroy (blocks[i].gameObject);
            }
        }
        sideIndex = 0;
        fibIndex = 1;
        bounds = new Bounds2D ();
        blocks = new List<Transform> ();
        sizes = new List<float> ();
    }

    int Fib (int n) {
        int f = 1;
        int p = 0;

        for (int i = 1; i < n; i++) {
            int fp = f;
            f += p;
            p = fp;

        }
        return f;
    }

    public class Bounds2D {
        public Vector2 xMinMax;
        public Vector2 yMinMax;

        public void AddSquare (Vector2 centre, float size) {
            xMinMax.x = Mathf.Min (xMinMax.x, centre.x - size / 2);
            xMinMax.y = Mathf.Max (xMinMax.y, centre.x + size / 2);
            yMinMax.x = Mathf.Min (yMinMax.x, centre.y - size / 2);
            yMinMax.y = Mathf.Max (yMinMax.y, centre.y + size / 2);
        }

        public Vector2 Centre {
            get {
                return new Vector2 (xMinMax.x + xMinMax.y, yMinMax.x + yMinMax.y) / 2;
            }
        }

        public Vector2 HalfSize {
            get {
                return new Vector2 (xMinMax.y - xMinMax.x, yMinMax.y - yMinMax.x) / 2;
            }
        }
    }
}