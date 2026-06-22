using UnityEngine;
using UnityEngine.UI;

namespace NDTB.UI.Helpers
{
    public static class UIHelper
    {
        public static bool IsOverlapping(RectTransform rect1, RectTransform rect2)
        {
            Rect r1 = GetWorldRect(rect1);
            Rect r2 = GetWorldRect(rect2);
            return r1.Overlaps(r2);
        }

        private static Rect GetWorldRect(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector2 size = corners[2] - corners[0];
            return new Rect(corners[0], size);
        }

        public static Rect GetPartialRect(RectTransform rt, Vector2 size)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector3 topLeft = corners[1];

            return new Rect(topLeft.x, topLeft.y - size.y, size.x, size.y);
        }

        public static bool IsPartialOverlap(RectTransform rt1, RectTransform rt2, float checkSize = 100f)
        {
            Rect r1 = GetPartialRect(rt1, new Vector2(checkSize, checkSize));
            Rect r2 = GetWorldRect(rt2);

            return r1.Overlaps(r2);
        }
    }
}
