using UnityEngine;
using System.Collections;

public static class RectExtensions {
    public static bool Contains (this RectInt a, RectInt b) {
        return a.xMin < b.xMin && a.xMax > b.xMax && a.yMin < b.yMin && a.yMax > b.yMax;
    }

    public static bool Intersects (this RectInt a, RectInt b) {
        return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
    }

}