using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleTurretTask : Task {

    [Header ("Task settings")]
    public float timeBetweenShots = 1;
    public float enemyTravelTimeMin = .5f;
    public float enemyTravelTimeMax = 3;

    [Header ("References")]
    public Turret turret;
    public Transform chargeBar;
    public Target enemyPrefab;
    public float enemyHalfWidth = .5f;

    public Transform spawnA;
    public Transform spawnB;

    [Header ("Visual tweaks")]
    public float debrisForceFromMovement;
    public float debrisForceFromProjectile;

    Target enemyA;
    Target enemyB;
    public float speedA;
    public float speedB;
    float lastShotTime;

    System.Random prng;

    protected override void Run (string code) {
        base.Run (code);

        prng = new System.Random ();
    }

    void LateUpdate () {
        float chargePercent = Mathf.Max (0, timeBetweenShots - TimeUntilCanShootAgain) / timeBetweenShots;
        chargeBar.localScale = new Vector3 (chargeBar.localScale.x, chargeBar.localScale.y, chargePercent);
        if (running) {

            if (enemyA == null) {
                enemyA = Instantiate (enemyPrefab, spawnA.position, spawnA.rotation);
                speedA = CalcSpeed (enemyB, speedB);
                enemyA.Init (speedA, 0, debrisForceFromProjectile, debrisForceFromMovement);
            }

            if (enemyB == null) {
                enemyB = Instantiate (enemyPrefab, spawnB.position, spawnB.rotation);
                speedB = CalcSpeed (enemyA, speedA);
                enemyB.Init (speedB, 0, debrisForceFromProjectile, debrisForceFromMovement);
            }

            if (TimeUntilCanShootAgain <= 0) {
                float[] inputValues = { DstA, DstB, speedA, speedB };
                GenerateOutputs (inputValues);

                if (outputQueue.Count > 0) {
                    Function output = outputQueue.Dequeue ();
                    if (output.name == taskInfo.outputs[0].name) {
                        lastShotTime = Time.time;
                        turret.Shoot (0, OnHit, 0);
                    } else if (output.name == taskInfo.outputs[1].name) {
                        lastShotTime = Time.time;
                        turret.Shoot (0, OnHit, 1);
                    }
                }
            }

            if (DstA <= 0) {
                //print ("Failed: " + currentIteration);
                TestFailed ();
                Destroy (enemyA.gameObject);
            }

            if (DstB <= 0) {
                //print ("Failed: " + currentIteration);
                TestFailed ();
                Destroy (enemyB.gameObject);
            }
        }
    }

    void OnHit (GameObject g) {
        TestPassed ();
    }

    float CalcSpeed (Target otherEnemy, float otherSpeed) {
        float spawnX = spawnA.position.x;
        float bufferTime = 0.1f;
        if (otherEnemy == null) {
            //print (((otherEnemy == enemyA) ? "B: " : "A: ") + " null");
            float time = Mathf.Lerp (Mathf.Max (TimeUntilCanShootAgain + bufferTime, enemyTravelTimeMin), enemyTravelTimeMax, SmallestRandom (1));
            return TravelDistance / time;
        }
        float otherEnemyTimeToTurret = Mathf.Abs (otherEnemy.transform.position.x - enemyHalfWidth - turret.muzzles[0].position.x) / otherSpeed;

        float maxSpeed = TravelDistance / enemyTravelTimeMin;
        float minSpeed = TravelDistance / enemyTravelTimeMax;

        // Enough time for turret to shoot this enemy and reload to shoot other
        if (otherEnemyTimeToTurret > TimeUntilCanShootAgain + timeBetweenShots + bufferTime) {
            float posOtherByTimeReadyToShoot = otherEnemy.transform.position.x - otherSpeed * TimeUntilCanShootAgain;
            float dstToReachOtherByShootTime = Mathf.Abs (posOtherByTimeReadyToShoot - spawnX);
            float speedToReachOtherByShootTime = dstToReachOtherByShootTime / Mathf.Max (0.00001f, TimeUntilCanShootAgain);
            if (speedToReachOtherByShootTime < maxSpeed && speedToReachOtherByShootTime > minSpeed) {
                //print ("dont quite catch up");
                return speedToReachOtherByShootTime * (1 - SmallestRandom (5));
            }
            //print ("race");
            float minTimeToReachTurret = TimeUntilCanShootAgain + bufferTime;
            float speedToReachTurretFirst = TravelDistance / Mathf.Lerp (minTimeToReachTurret, enemyTravelTimeMax, SmallestRandom (3));
            return Mathf.Clamp (speedToReachTurretFirst, minSpeed, maxSpeed);
        }
        // Has to shoot other enemy first, otherwise will die
        // print ("must shoot other first");

        float minTime = TimeUntilCanShootAgain + timeBetweenShots + bufferTime;
        minTime = Mathf.Max (enemyTravelTimeMin, minTime);

        float travelTime = Mathf.Lerp (minTime, enemyTravelTimeMax, SmallestRandom (2));

        return TravelDistance / travelTime;

    }

    protected override void Clear () {
        base.Clear ();
        turret.Clear ();
        if (enemyA != null) {
            Destroy (enemyA.gameObject);
        }
        if (enemyB != null) {
            Destroy (enemyB.gameObject);
        }
        lastShotTime = -timeBetweenShots;
    }

    float DstA {
        get {
            return (enemyA.transform.position.x - enemyHalfWidth - turret.muzzles[0].position.x);
        }
    }

    float DstB {
        get {
            return (enemyB.transform.position.x - enemyHalfWidth - turret.muzzles[1].position.x);
        }
    }

    float TimeUntilCanShootAgain {
        get {
            float timeSinceLast = Time.time - lastShotTime;
            return Mathf.Max (0, timeBetweenShots - timeSinceLast);
        }
    }

    float SmallestRandom (int iterations) {
        float r = 1;
        for (int i = 0; i < iterations; i++) {
            r = Mathf.Min (r, (float) prng.NextDouble ());
        }
        return r;
    }

    float TravelDistance {
        get {
            return Mathf.Abs (spawnA.position.x - enemyHalfWidth - turret.muzzles[0].position.x);
        }
    }

}