using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace Demo.MMODemo {

    public enum SlotType {
        Head,
        Hand,
        Body,
        Legs,
        Feet,
        Item,
        Accessory,
    }

    public class InventoryItem {
        public string id;
        public string name;
        public SlotType slotType;
        public int amount = 1;
    }

    public class InventoryItemDragEvent : DragEvent {

        public readonly InventoryItem item;
        
        public InventoryItemDragEvent(UIElement origin, InventoryItem item) {
            this.item = item;
        }

        public override void Update() {
            origin.style.SetTransformPosition(MousePosition, StyleState.Normal);
            origin.style.SetBackgroundColor(Color.red, StyleState.Normal);
            origin.style.SetAlignmentTargetX(AlignmentTarget.Screen, StyleState.Normal);
            origin.style.SetAlignmentTargetY(AlignmentTarget.Screen, StyleState.Normal);
        }

        public override void OnComplete() {
            origin.style.SetAlignmentTargetX(AlignmentTarget.LayoutBox, StyleState.Normal);
            origin.style.SetAlignmentTargetY(AlignmentTarget.LayoutBox, StyleState.Normal);
            origin.style.SetTransformPosition(Vector2.zero, StyleState.Normal);
            origin.style.SetBackgroundColor(Color.yellow, StyleState.Normal);
        }
    }

    // todo -- the automatically detected template path does not work here; cannot remove the template attribute
    [Template("MMODemo/MMORoot.xml")]
    public class MMORoot : UIElement {

        public string characterName = "Dumbledore";

        public string headSlot = "";

        public IList<InventoryItem> inventoryItems = new List<InventoryItem>() {
                new InventoryItem { id = "he1", slotType = SlotType.Head, name = "Basecap" },          
                new InventoryItem { id = "sw1", slotType = SlotType.Hand, name = "Beast Sword" },          
                new InventoryItem { id = "ax1", slotType = SlotType.Hand, name = "Bloody Axe" },          
                new InventoryItem { id = "ar1", slotType = SlotType.Body, name = "Dragon Armour" },          
                new InventoryItem { id = "er1", slotType = SlotType.Accessory, name = "Earring" },           
                new InventoryItem { id = "mt1", slotType = SlotType.Legs, name = "Mythrill Kneepads" },           
                new InventoryItem { id = "pt1", slotType = SlotType.Item, name = "Healing Potion" },           
                new InventoryItem { id = "sh1", slotType = SlotType.Feet, name = "Unicorn Slippers" },           
        };

        public InventoryItemDragEvent StartDragInventoryItem(UIElement origin, InventoryItem item) {
            return new InventoryItemDragEvent(origin, item);
        }

        public void DropItem(InventoryItemDragEvent evt) {
            if (evt.item.slotType == SlotType.Head) {
                headSlot = evt.item.id;
            }
        }
    }
}
