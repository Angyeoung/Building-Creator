using System;
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
            v2.Add(v3[i].ToXZ());
        }
        return v2;
    }

    // Given a Vector3 list, return the area of the polygon, and the winding orderer (indicated by sign)
    public static float FindArea2D(this List<Vector2> v2) {
        // to 2D array
        // Loop verts & calculate areas
        float area = 0;
        for (int i = 0; i < v2.Count; i++) {
            Vector2 thisVert = v2[i];
            Vector2 nextVert = v2[(i+1) % v2.Count];
            // A = dX * dY
            area += (nextVert.x - thisVert.x) * (thisVert.y + nextVert.y)/2;
        }
        return area;

    }

    // Get an item which has an index higher than the length of its list
    public static T GetItem<T>(this List<T> list, int index) {
            if (index >= list.Count)
                return list[index % list.Count];
            if (index < 0)
                return list[index % list.Count + list.Count];
            else
                return list[index];
    }

    // Custom JS-like .map() function =)
    public static List<T> Map<T>(this List<T> list, Func<T, T> func) {
        List<T> temp = new List<T>(list);
        for (int i = 0; i < temp.Count; i++) {
            temp[i] = func(temp[i]);
        }
        return temp;
    }

    // Cross product of 2 Vector2
    public static float Cross(this Vector2 a, Vector2 b) {
        return a.x * b.y - a.y * b.x;
    }

    // Returns true if point P is inside of triangle ABC, false otherwise
    public static bool InTriangle(this Vector2 p, Vector2 a, Vector2 b, Vector2 c) {
        
        float d1 = (b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y);
        float d2 = c.y - a.y;
        float w1 = (a.x * (c.y - a.y) + (p.y - a.y) * (c.x - a.x) - p.x * (c.y - a.y)) / d1;
        float w2 = (p.y - a.y - w1 * (b.y - a.y)) / d2;

        if (w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1)
            return true;
        return false;

    }

    // Given a list of points with custom indices, return the triangulated list of points with their custom indices
    public static List<int> Triangulate(this List<Vector2> points) {
        // Checks
        if (points == null || points.Count < 3)
            return null;

        // Fill indexList
        List<int> result = new List<int>();
        List<int> indexList = new List<int>();
        for(int i = 0; i < points.Count; i++){
            indexList.Add(i);
        }

        // Clip ears
        int attempts = 0;
        while(indexList.Count > 3 && attempts < points.Count * points.Count + 20) {
            for (int i = 0; i < indexList.Count; i++) {
                int a = indexList.GetItem(i);
                int b = indexList.GetItem(i - 1);
                int c = indexList.GetItem(i + 1);
                Vector2 va = points[a];
                Vector2 vb = points[b];
                Vector2 vc = points[c];
                Vector2 vavb = vb - va;
                Vector2 vavc = vc - va;

                // Is ear test vertex convex?
                if(vavb.Cross(vavc) < 0f) {
                    continue;
                }

                bool ear = true;

                // Does test ear contain any polygon vertices?
                for(int j = 0; j < points.Count; j++) {
                    if(j == a || j == b || j == c) {
                        continue;
                    }

                    Vector2 p = points.GetItem(j);

                    if(p.InTriangle(vb, va, vc)) {
                        ear = false;
                        break;
                    }                            
                }

                // If the ear is valid, clip it
                if(ear) {
                    int[] bac = {b, a, c};
                    result.AddRange(bac);
                    indexList.RemoveAt(i);
                    break;
                }
            }
            attempts++;
        }

        int[] last = {indexList[0], indexList[1], indexList[2]};
        result.AddRange(last);
        return result;
    }

}
