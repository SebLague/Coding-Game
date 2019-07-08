using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour {
    public Projectile projectilePrefab;
    public float projectileSpeed;
    public Transform[] muzzles;

    [HideInInspector]
    public List<Projectile> activeProjectiles = new List<Projectile> ();

    public void Clear () {
        for (int i = activeProjectiles.Count - 1; i >= 0; i--) {
            if (activeProjectiles[i] != null) {
                Destroy (activeProjectiles[i].gameObject);
            }
        }
        activeProjectiles = new List<Projectile> ();
        StopAllCoroutines ();
    }

    public void Shoot (float delay, System.Action<GameObject> hitCallback, int index = 0) {
        StartCoroutine (ShootRoutine (delay, index, hitCallback));
    }

    IEnumerator ShootRoutine (float delay, int index, System.Action<GameObject> hitCallback) {
        yield return new WaitForSeconds (delay);
        muzzles[index].gameObject.SetActive (true);

        var projectile = Instantiate (projectilePrefab, muzzles[index].position, muzzles[index].rotation);
        activeProjectiles.Add (projectile);
        projectile.Init (projectileSpeed, hitCallback);

        yield return new WaitForSeconds (0.065f);
        muzzles[index].gameObject.SetActive (false);
    }

}