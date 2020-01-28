using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

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
    
    // todo -- the automatically detected template path does not work here; cannot remove the template attribute
    [Template("Demo/MMODemo/MMORoot.xml")]
    public class MMORoot : UIElement {

        public string characterName = "Dumbledore";

        public IList<InventoryItem> inventoryItems = new List<InventoryItem>() {
                new InventoryItem { id = "sw1", slotType = SlotType.Hand, name = "Beast Sword" },          
                new InventoryItem { id = "ax1", slotType = SlotType.Hand, name = "Bloody Axe" },          
                new InventoryItem { id = "ar1", slotType = SlotType.Body, name = "Dragon Armour" },          
                new InventoryItem { id = "er1", slotType = SlotType.Accessory, name = "Earring" },           
                new InventoryItem { id = "mt1", slotType = SlotType.Legs, name = "Mythrill Kneepads" },           
                new InventoryItem { id = "pt1", slotType = SlotType.Item, name = "Healing Potion" },           
                new InventoryItem { id = "sh1", slotType = SlotType.Feet, name = "Unicorn Slippers" },           
        };

    }
}
