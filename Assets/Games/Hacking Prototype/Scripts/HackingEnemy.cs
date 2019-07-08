using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingEnemy : HackingEntity {
    public float speed = 1;
    Transform target;

    [HideInInspector]
    public bool active;

    void Start () {
        target = FindObjectOfType<HackingPlayer> ().transform;
        transform.eulerAngles = Vector3.up * 180;
    }

    void Update () {
        if (active && target != null) {
            Vector3 dirToTarget = Vector3.Scale (target.position - transform.position, new Vector3 (1, 0, 1)).normalized;
            transform.eulerAngles = Vector3.up * Mathf.Atan2 (dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;
            transform.position += dirToTarget * speed * Time.deltaTime;
            if ((target.position - transform.position).magnitude < (target.localScale.x / 2 + transform.localScale.x / 2)) {
                target.GetComponent<HackingEntity> ().Hit (transform.position, 0);
            }
        }
    }
}