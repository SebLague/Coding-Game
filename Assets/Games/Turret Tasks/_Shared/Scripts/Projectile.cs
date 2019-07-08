using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class Projectile : MonoBehaviour {
    float speed;
    Rigidbody rb;
    System.Action<GameObject> hitCallback;

    public void Init (float speed, System.Action<GameObject> hitCallback) {
        rb = GetComponent<Rigidbody> ();
        this.speed = speed;
        this.hitCallback = hitCallback;
        rb.AddForce (transform.forward * speed, ForceMode.VelocityChange);
    }

    void OnTriggerEnter (Collider c) {
        if (c.GetComponent<Target> ()) {
            c.GetComponent<Target> ().Hit (rb.position - transform.forward * Time.deltaTime * speed * 2, speed);
            
            if (hitCallback != null) {
                hitCallback (c.gameObject);
            }

            Destroy (gameObject);
        }

    }

    void FixedUpdate () {
        //rb.position += transform.forward * Time.deltaTime * speed;
    }
}