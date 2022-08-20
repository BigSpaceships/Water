using UnityEngine;
using UnityEngine.EventSystems;

namespace _Inventory {
    [RequireComponent(typeof(RectTransform))]
    public class Item : MonoBehaviour, IDragHandler, IEndDragHandler {
        private RectTransform _rectTransform;

        private Slot _slot;

        private void Start() {
            _rectTransform = transform as RectTransform;

            _slot = InventoryManager.GetInventoryManager().GetNearestSlot(_rectTransform.anchoredPosition);
            _rectTransform.anchoredPosition = _slot.GetPos();
        }
        
        public void OnDrag(PointerEventData data) {
            _rectTransform.anchoredPosition += data.delta;
        }

        public void OnEndDrag(PointerEventData data) {
            _slot = InventoryManager.GetInventoryManager().GetNearestSlot(_rectTransform.anchoredPosition);

            _rectTransform.anchoredPosition = _slot.GetPos();
        }
    }
}