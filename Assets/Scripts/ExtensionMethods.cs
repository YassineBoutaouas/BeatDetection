using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public const double MinNormal = 2.2250738585072014E-308d;

    public static bool NearlyEquals(this float x, float y)
    {
        var diff = Math.Abs(x - y);

        if (x == y)
            return true;

        return diff <= 0.001f || diff <= Math.Max(Math.Abs(x), Math.Abs(y)) * 0.001f;
    }
}
