using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Helpers
{
    public static class TransformExtensions
    {
        public static void ClearChildren(this Transform t)
        {
            for (var i = 0; i < t.childCount; i++)
            {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }
    }
}