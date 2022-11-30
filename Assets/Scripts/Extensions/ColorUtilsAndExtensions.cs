using UnityEngine;

namespace AppExtensions
{
    public static class ColorsExtensions
    {
       public static Color CopyColor(Color target, float persistingAlpha = -1)
        {
            return new Color(target.r, target.g, target.b, persistingAlpha == -1 ? target.a: persistingAlpha);
        }
    }
}
