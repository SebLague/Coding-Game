using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentSpawner : MonoBehaviour {

    public float forceMain = 1;
    public float forceResid = 1;
    public Color targetCol;
    public float colChangeTime = 3;

    Material[] mats;
    [HideInInspector]
    public Color col;
    float t;

    void Update () {
        t += Time.deltaTime;
        for (int i = 0; i < mats.Length; i++) {
            mats[i].color = Color.Lerp (col, targetCol, t / colChangeTime);
        }

    }

    public void Spawn (Vector3 velocity, Vector3 contact, float contactSpeed) {
        contact += Vector3.left * .5f;
        var rbs = GetComponentsInChildren<Rigidbody> ();
        mats = new Material[rbs.Length];
        for (int i = 0; i < rbs.Length; i++) {
            var rb = rbs[i];
            Vector3 dir = (rb.transform.position - contact).normalized;
           
            dir = new Vector3 (dir.x, 0, dir.z);
            rb.AddForce (velocity.normalized * forceResid + dir * forceMain, ForceMode.VelocityChange);
            mats[i] = rb.gameObject.GetComponent<MeshRenderer> ().material;
        }
        col = mats[0].color;
    }
}