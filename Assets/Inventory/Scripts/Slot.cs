using UnityEngine;

namespace _Inventory {
    [RequireComponent(typeof(RectTransform))]
    public class Slot : MonoBehaviour {
        public Vector2Int slotPos;

        public float GetDistanceTo(Vector2 pos) {
            var rectTransform = transform as RectTransform;

            return (GetPos() - pos).magnitude;
        }

        public Vector2 GetPos() {
            return ((RectTransform) transform).anchoredPosition + ((RectTransform) transform.parent).anchoredPosition;
        }
    }
}