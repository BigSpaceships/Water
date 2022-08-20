using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Inventory {
    public class InventoryManager : MonoBehaviour {
        public List<Inventory> inventories;
        
        public static InventoryManager GetInventoryManager() {
            return GameObject.Find("Canvas").GetComponent<InventoryManager>();
        }

        public List<Inventory> GetInventories() {
            return inventories.Where(inventory => inventory.isActiveAndEnabled).ToList();
        }
        
        public Slot GetNearestSlot(Vector2 pos) {
            var closestSlots = GetInventories().Select<Inventory, Slot>(inventory => inventory.GetNearestSlot(pos));

            var slots = closestSlots as Slot[] ?? closestSlots.ToArray();
            return slots.ToList().Aggregate(slots.ToList()[0], (oldSlot, slot) => oldSlot.GetDistanceTo(pos) > slot.GetDistanceTo(pos) ? slot : oldSlot);
        }
    }
}