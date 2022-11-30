using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class ColorsExtensions
    {
       public static Color Copy(this Color color, Color target, bool preserveCurrentAlpha = false)
        {
            return new Color(target.r, target.g, target.b, preserveCurrentAlpha ? color.a: target.a);
        }
    }
}
