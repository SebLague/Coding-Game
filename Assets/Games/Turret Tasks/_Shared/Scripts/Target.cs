using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    public FragmentSpawner frag;

    float speed;
    float accel;
    Rigidbody rb;

    public void Init (float speed, float accel, float fragProjectileInheritForce = 1, float fragInheritMovementForce = 1) {
        this.speed = speed;
        this.accel = accel;
        frag.forceMain = fragProjectileInheritForce;
        frag.forceResid = fragInheritMovementForce;
        rb = GetComponent<Rigidbody> ();
    }

    void Update () {
        speed += Time.deltaTime * accel;

    }

    void FixedUpdate () {
        rb.MovePosition (rb.position + transform.forward * speed * Time.deltaTime);
    }

    public void Hit (Vector3 hitPoint, float speed) {
        var f = Instantiate (frag, transform.position, Quaternion.Euler (Vector3.up * 45 * Random.Range (0, 4)));
        f.Spawn (transform.forward * speed, hitPoint, speed);
        Destroy (gameObject);
    }
}