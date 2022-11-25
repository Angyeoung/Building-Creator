using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions {

    // Vector3(x, y, z) => Vector2(x, z)
    public static Vector2 ToXZ(this Vector3 v3) {
        return new Vector2(v3.x, v3.z);
    }

}
