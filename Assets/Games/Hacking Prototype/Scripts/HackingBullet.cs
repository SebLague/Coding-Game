using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingBullet : MonoBehaviour {
    public float speed = 10;
    public LayerMask targetMask;

    void Update () {
        Vector3 targPos = transform.position + transform.forward * speed * Time.deltaTime;
        //Debug.Log (speed);

        RaycastHit hit;
        if (Physics.SphereCast (transform.position - transform.forward * Time.deltaTime * speed * .1f, .3f, (targPos - transform.position).normalized, out hit, speed * Time.deltaTime + 0.5f, targetMask)) {
            // Debug.Log(hit.collider.name);
            if (hit.collider.gameObject.GetComponent<HackingEntity> ()) {
                hit.collider.gameObject.GetComponent<HackingEntity> ().Hit (transform.position, speed);
            }
            Destroy (gameObject);
        }

        transform.position = targPos;
    }
}