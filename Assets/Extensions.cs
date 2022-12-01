using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions {

    // Vector3(x, y, z) => Vector2(x, z)
    public static Vector2 ToXZ(this Vector3 v3) {
        return new Vector2(v3.x, v3.z);
    }

    // List<Vector3> => List<Vector2> (XZ)
    public static List<Vector2> ToXZ(this List<Vector3> v3) {
        List<Vector2> v2 = new List<Vector2>();
        for (int i = 0; i < v3.Count; i++) {
            v2.Add(new Vector2(v3[i].x, v3[i].z));
        }
        return v2;
    }

    // Given a Vector3 list, return the area of the polygon, and the winding orderer (indicated by sign)
    // If return value is positive, the winding order is clockwise, otherwise counter-clockwise
    // Assumes polygons have no intersecting lines + verts[i] is connected to verts[i+1] and verts[i-1]
    // Assumes there are no intersecting edges
    public static float FindArea2D(this List<Vector3> verts3) {
        // to 2D array
        List<Vector2> verts = verts3.ToXZ();
        // Loop verts & calculate areas
        float area = 0;
        for (int i = 0; i < verts.Count; i++) {
            Vector2 thisVert = verts[i];
            Vector2 nextVert = verts[(i+1) % verts.Count];
            // A = dX * dY
            area += (nextVert.x - thisVert.x) * (thisVert.y + nextVert.y)/2;
        }
        return area;

    }

    // public static int[] Triangulate(this List<Vector3> v3) {
    //     float windingOrder = Mathf.Sign(v3.FindArea2D());
    //     List<Vector2> v2 = v3.ToXZ();
    //     List<int> result = new List<int>();

    //     for (int i = 0; i < v3.Count; i++) {
    //         Vector2 prevPoint = v2[(i-1) % v2.Count];
    //         Vector2 thisPoint = v2[i];
    //         Vector2 nextPoint = v2[(i+1) % v2.Count];



    //     }

    // }

}
