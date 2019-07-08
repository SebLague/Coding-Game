using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingTurret : MonoBehaviour {
    public Transform muzzle;
    public HackingBullet missilePrefab;
    public Transform swivel;
    public float timeBetweenShots;
    public float smoothRot = 8;
    float lastShotTime;
    [HideInInspector]
    public bool active;
    float time;
    public LayerMask targetMask;
    float aimError;
    bool aimed;

    public void Reset () {
        swivel.rotation = Quaternion.identity;
        lastShotTime = time;
    }

    void Update () {
        if (active && aimed) {
            time += Time.deltaTime;
            if (time > lastShotTime + timeBetweenShots && aimError < 2) {
                lastShotTime = time;
                Shoot ();
            }

            RaycastHit hit;
            if (Physics.Raycast (muzzle.position, muzzle.forward * 100, out hit, targetMask)) {
                Debug.DrawLine (muzzle.position, hit.point, Color.red);
            }
        }

        aimed = false;
    }

    void Shoot () {
        var b = Instantiate (missilePrefab, muzzle.position, muzzle.rotation);
        StartCoroutine (AnimShot ());
    }

    IEnumerator AnimShot () {
        muzzle.gameObject.SetActive (true);
        yield return new WaitForSeconds (0.1f);
        muzzle.gameObject.SetActive (false);
    }

    public void Aim (float x, float z) {
        Vector3 aim = new Vector3 (x, 0, z);
        Quaternion lookRot = Quaternion.LookRotation ((aim - swivel.position).normalized, Vector3.up);
        swivel.rotation = Quaternion.Slerp (swivel.rotation, lookRot, Time.deltaTime * smoothRot);
        aimError = Quaternion.Angle (swivel.rotation, lookRot);
        //swivel.LookAt (aim);
        //print(aimError);
        aimed = true;
    }
}