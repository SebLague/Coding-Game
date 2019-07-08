using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingPlayer : HackingEntity {
    public float speed = 1;
    public float turnSmoothT;
    float angle;
    float angleSmoothV;
    Rigidbody rb;

    [HideInInspector]
    public Vector2 dir;

    void Start () {
        rb = GetComponent<Rigidbody> ();
    }

    void Update () {
        Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
        dir = input.normalized;
        if (input.magnitude != 0) {
            float targAngle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
            transform.eulerAngles = Vector3.up * targAngle;

        }
        //transform.position += new Vector3 (dir.x, 0, dir.y) * speed * Time.deltaTime;
    }

    void FixedUpdate () {
        //rb.MovePosition (rb.position + transform.forward * Time.deltaTime * speed * dir.magnitude);
        rb.velocity = transform.forward * speed * dir.magnitude;
    }
}