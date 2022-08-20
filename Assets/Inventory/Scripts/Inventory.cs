using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Inventory {
    [ExecuteAlways]
    public class Inventory : MonoBehaviour {
        private Dictionary<Vector2Int, Slot> _slots; // only a dictionary bc of the auto generating code

        [Header("Stuff for slot generator")] 
        public GameObject slotTemplate;

        public Vector2 SlotStartPos => centered
            ? new Vector2(-(numberOfSlots.x - 1) / 2f * DistBetweenSlots,
                -(numberOfSlots.y - 1) / 2f * DistBetweenSlots)
            : slotStartPos;

        public Vector2 slotStartPos;
        public Vector2Int numberOfSlots = new(9, 3);
        private Vector2Int _oldNumberOfSlots;

        public bool centered = true;

        public float slotSize = 50;
        public float slotPadding = 1;

        public float DistBetweenSlots => slotPadding + slotSize;

        private void Reset() {
            slotTemplate = Resources.Load<GameObject>("Inventory/Slot");
        }

        public void Start() {
            OnSlotsChanged();

            SetSlots();
        }

        public Slot GetNearestSlot(Vector2 pos) {
            // gets the nearest slot to the given pos
            return _slots.Values.ToList().Aggregate(_slots[Vector2Int.zero], // just start at the first slot
                (oldSlot, slot) => oldSlot.GetDistanceTo(pos) > slot.GetDistanceTo(pos) ? slot : oldSlot); // if new slot is closer to pos than old slot use it
        }

        public void SetSlots() { // initializes the slot from the game objects 
            _slots = new Dictionary<Vector2Int, Slot>();

            for (int i = 0; i < transform.childCount; i++) {
                var slot = transform.GetChild(i).GetComponent<Slot>();

                _slots.Add(slot.slotPos, slot);
            }
        }

        public void OnSlotsChanged() { // updates the slot positions
            var slotsToRemove = new List<Vector2Int>();

            if (_slots == null) {
                SetSlots();
            }

            foreach (var keyValueSlot in _slots) {                
                if (keyValueSlot.Key.x > numberOfSlots.x || keyValueSlot.Key.y > numberOfSlots.y) {
                    slotsToRemove.Add(keyValueSlot.Key);
                }
            }

            foreach (var slot in slotsToRemove) {
                Destroy(_slots[slot].gameObject);
                _slots.Remove(slot);
            }

            for (int x = 0; x < numberOfSlots.x; x++) {
                for (int y = 0; y < numberOfSlots.y; y++) {
                    GameObject newSlot;
                    var slotIndex = new Vector2Int(x, y);

                    if (!_slots.ContainsKey(slotIndex)) {
                        newSlot = Instantiate(slotTemplate, transform);
                        _slots[slotIndex] = newSlot.GetComponent<Slot>();
                    }
                    else {
                        newSlot = _slots[slotIndex].gameObject;
                    }

                    var startPos = SlotStartPos + new Vector2(DistBetweenSlots * x, DistBetweenSlots * y);

                    var newSlotTransform = newSlot.transform as RectTransform;

                    newSlotTransform.anchoredPosition = startPos;
                    newSlotTransform.sizeDelta = new Vector2(slotSize, slotSize);

                    newSlot.GetComponent<Slot>().slotPos = slotIndex;
                }
            }
        }
    }
}