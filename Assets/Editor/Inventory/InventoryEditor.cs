using _Inventory;
using UnityEditor;
using UnityEngine;

namespace Editor.Inventory {
    [CustomEditor(typeof(_Inventory.Inventory))]
    public class InventoryEditor : UnityEditor.Editor {
        private Vector2 _oldSlotStartPos;
        private Vector2Int _oldNumberOfSlots;
        
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var inventoryManager = (_Inventory.Inventory) target;

            if (inventoryManager.SlotStartPos != _oldSlotStartPos ||
                inventoryManager.numberOfSlots != _oldNumberOfSlots) {
                inventoryManager.OnSlotsChanged();
            } 
        }
    }
}