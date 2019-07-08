using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility {

    public static bool LineSegmentsIntersect (Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
        float d = (b2.x - b1.x) * (a1.y - a2.y) - (a1.x - a2.x) * (b2.y - b1.y);
        if (d == 0)
            return false;
        float t = ((b1.y - b2.y) * (a1.x - b1.x) + (b2.x - b1.x) * (a1.y - b1.y)) / d;
        float u = ((a1.y - a2.y) * (a1.x - b1.x) + (a2.x - a1.x) * (a1.y - b1.y)) / d;

        return t >= 0 && t <= 1 && u >= 0 && u <= 1;
    }

    public static Vector2 PointOfLineLineIntersection (Vector2 a1, Vector2 a2, Vector2 a3, Vector2 a4) {
        float d = (a1.x - a2.x) * (a3.y - a4.y) - (a1.y - a2.y) * (a3.x - a4.x);
        if (d == 0) {
            Debug.LogError ("Lines are parallel, please check that this is not the case before calling line intersection method");
            return Vector2.zero;
        } else {
            float n = (a1.x - a3.x) * (a3.y - a4.y) - (a1.y - a3.y) * (a3.x - a4.x);
            float t = n / d;
            return a1 + (a2 - a1) * t;
        }
    }
}