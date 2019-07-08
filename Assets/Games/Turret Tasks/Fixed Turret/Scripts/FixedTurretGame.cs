using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedTurretGame : Task {

    public float t;
    float testShootTime;
    [Header ("Enemy")]
    public Target enemyPrefab;
    public Transform topLeft, topRight, bottomLeft, bottomRight;
    public Transform[] intersecionBoundsX;
    public float enemySpeedMin;
    public float enemySpeedMax;
    public float accelMin;
    public float accelMax;
    public float traverseTimeMin;
    public float traverseTimeMax;

    [Header ("Turret")]
    public Turret turret;

    [Header ("Visual tweaks")]
    public float debrisForceFromMovement;
    public float debrisForceFromProjectile;

    Target currentEnemy;
    float enemySpeed;
    float enemyAccel;
    Vector3 targetPos;
    float minSqrDstFromTarg;

    protected override void Start () {
        base.Start ();
    }

    protected override void Run (string code) {
        base.Run (code);
    }

    protected override void Clear () {
        base.Clear ();
        if (currentEnemy != null) {
            Destroy (currentEnemy.gameObject);
        }
        turret.Clear ();
    }

    Vector3 GetRandomPos () {
        int side = Random.Range (0, 3);
        Vector3 a = topLeft.position;
        Vector3 b = topRight.position;
        if (side == 1) {
            a = topRight.position;
            b = bottomRight.position;
        } else if (side == 2) {
            a = bottomLeft.position;
            b = bottomRight.position;
        }

        return Vector3.Lerp (a, b, Random.value);
    }

    void Update () {
        if (running) {
            t += Time.deltaTime;
            if (currentEnemy == null) {
                TestPassed ();
            } else {
                if (turret.activeProjectiles != null && turret.activeProjectiles.Count > 0) {
                    var lastProjectile = turret.activeProjectiles[turret.activeProjectiles.Count - 1];
                    if (lastProjectile != null) {
                        if (lastProjectile.transform.position.x > topRight.position.x) {
                            TestFailed ();
                        }
                    }
                }
                float sqrDst = (currentEnemy.transform.position - targetPos).sqrMagnitude;
                if (sqrDst > minSqrDstFromTarg) {
                    Destroy (currentEnemy.gameObject);
                    TestFailed ();
                } else {
                    minSqrDstFromTarg = sqrDst;
                }
            }
        }
    }

    protected override void StartNextTestIteration () {
        base.StartNextTestIteration ();

        t = 0;
        Vector3 spawnPos = Vector3.zero;

        // Find acceptable target pos
        int safety = 0;
        while (true) {
            spawnPos = GetRandomPos ();
            targetPos = GetRandomPos ();
            var a = new Vector2 (spawnPos.x, spawnPos.z);
            var b = new Vector2 (targetPos.x, targetPos.z);
            Vector2 turretPos = new Vector2 (turret.muzzles[0].position.x, turret.muzzles[0].position.z);
            Vector2 turretEndPos = turretPos + Vector2.right * 100;
            if (MathUtility.LineSegmentsIntersect (a, b, turretPos, turretEndPos)) {
                Vector2 intersection = MathUtility.PointOfLineLineIntersection (a, b, turretPos, turretEndPos);
                if (intersection.x > intersecionBoundsX[0].position.x && intersection.x < intersecionBoundsX[1].position.x) {
                    break;
                }
                //Debug.DrawLine (spawnPos, targetPos, Color.red, 3);
            }
            safety++;
            if (safety > 100) {
                print ("safety exceeded");
                break;
            }
        }

        // Find acceptable velocity and acceleration
        Vector3 dir = (targetPos - spawnPos).normalized;
        float dst = (spawnPos - targetPos).magnitude;
        minSqrDstFromTarg = (spawnPos - targetPos).sqrMagnitude;
        safety = 0;
        while (true) {
            enemySpeed = Random.Range (enemySpeedMin, enemySpeedMax);
            enemyAccel = Random.Range (accelMin, accelMax);
            float vf = Mathf.Sqrt (enemySpeed * enemySpeed + 2 * enemyAccel * dst);
            float t = (2 * dst) / (enemySpeed + vf);
            if (t > traverseTimeMin && t < traverseTimeMax) {
                // Calc solution
                // inputs
                float vx = dir.x * enemySpeed;
                float vy = dir.z * enemySpeed;
                float posX = spawnPos.x;
                float posY = spawnPos.z;

                ///
                float speedX = Mathf.Abs (vx);
                float speedY = Mathf.Abs (vy);
                float speed = Mathf.Sqrt (speedX * speedX + speedY * speedY);
                float ax = enemyAccel * speedX / speed;
                float ay = enemyAccel * speedY / speed;
                float dstY = Mathf.Abs (posY);

                float speedYAtInt = Mathf.Sqrt (speedY * speedY + 2 * ay * dstY);
                float tToInt = (2 * dstY) / (speedY + speedYAtInt);
                float speedXAtInt = speedX + ax * tToInt;
                float dx = (speedX + speedXAtInt) / 2 * tToInt;
                float xAtInt = posX + dx * Mathf.Sign (vx);
                float projTime = Mathf.Abs (xAtInt - turret.muzzles[0].position.x) / turret.projectileSpeed;

                float shootTime = tToInt - projTime;

                if (shootTime > 0) {
                    testShootTime = shootTime;
                    break;
                }
            }
            safety++;
            if (safety > 250) {
                print ("safety exceeded");
                break;
            }
        }

        float angle = Mathf.Atan2 (dir.x, dir.z) * Mathf.Rad2Deg;

        currentEnemy = Instantiate (enemyPrefab, spawnPos, Quaternion.Euler (Vector3.up * angle));
        currentEnemy.Init (enemySpeed, enemyAccel, debrisForceFromProjectile, debrisForceFromMovement);

        //float turretX = 0;
        //float turretY = 0;

        float targetX = spawnPos.x - turret.muzzles[0].position.x;
        float targetY = spawnPos.z - turret.muzzles[0].position.z;

        // inputs:
        float[] inputs = {
            targetX,
            targetY,
            dir.x,
            dir.z,
            enemySpeed,
            enemyAccel
        };
        GenerateOutputs (inputs);
        while (outputQueue.Count != 0) {
            var func = outputQueue.Dequeue ();
            turret.Shoot (func.value, null, 0);
        }
    }

}