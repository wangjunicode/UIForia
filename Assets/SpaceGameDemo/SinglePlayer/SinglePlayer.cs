using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SpaceGameDemo.SinglePlayer {
    public class ShipData {
        public int id;
        public string name;

        public ShipData(int id, string name) {
            this.id = id;
            this.name = name;
        }
    }

    public class InventoryItem {
        public string name;
        public int width;
        public int height;

        public InventoryItem(string name, int width, int height) {
            this.name = name;
            this.width = width;
            this.height = height;
        }
    }

    public enum TabItem {
        Ships, Items, Stats
    }

    [Template("SpaceGameDemo/SinglePlayer/SinglePlayer.xml")]
    public class SinglePlayer : UIElement {
        public int selectedShip = 1;
        public List<ShipData> ships;
        // Property
        public float progress;

        // Property
        public string progressString;
        
        // Property
        public TabItem tab = TabItem.Ships;
        
        // Property
        public List<InventoryItem> availableItems;
        
        // Property
        public List<InventoryItem> inventoryItems;

        // Property
        public int upgradePoints = 39;
        
        // Property
        public string nickname;

        private float speed = 0.1f;

        public override void OnUpdate() {
            progress += Time.deltaTime * speed;
            if (progress > 1) {
                progress = 0;
            }

            progressString = (progress * 100).ToString("F1");
        }

        public override void OnCreate() {
            ships = new List<ShipData>() {
                new ShipData(1, "Cruiser"),
                new ShipData(2, "Bomber"),
                new ShipData(3, "Shuttle"),
                new ShipData(4, "Fighter"),
            };
            
            availableItems = new List<InventoryItem>() {
                new InventoryItem("R1 Rockets", 2, 1),
                new InventoryItem("Beam Laser", 2, 1),
                new InventoryItem("Drone System", 4, 4),
                new InventoryItem("Cool Drinks", 2, 1),
                new InventoryItem("Radar", 2, 2),
            };
            inventoryItems = new List<InventoryItem>();
        }

        public void SelectShip(int id) {
            selectedShip = id;
        }

        public DragEvent OnDragCreateItem(MouseInputEvent evt, InventoryItem item) {
            return new InventoryItemDragEvent(evt, item);
        }

        public void SelectTab(TabItem tabItem) {
            tab = tabItem;
        }

        public void DropItemIntoList(DragEvent evt) {
            if (evt is InventoryItemDragEvent inventoryEvent) {
                InventoryItem item = inventoryEvent.item;
                if (!availableItems.Contains(item)) {
                    inventoryItems.Remove(item);
                    availableItems.Add(item);
                }
            }
        }

        public void DropItemIntoInventory(DragEvent evt) {
            if (evt is InventoryItemDragEvent inventoryEvent) {
                InventoryItem item = inventoryEvent.item;
                if (!inventoryItems.Contains(item)) {
                    inventoryItems.Add(inventoryEvent.item);
                    availableItems.Remove(inventoryEvent.item);
                }
            }
        }
        
    }

    public class InventoryItemDragEvent : DragEvent {

        public InventoryItem item;

        private UIElement itemElement;

        private Vector2 offset;
        
        public InventoryItemDragEvent(MouseInputEvent evt, InventoryItem item) {
            this.item = item;
            itemElement = evt.element;
            offset = evt.MousePosition - itemElement.layoutResult.screenPosition;
            itemElement.style.SetAlignmentTargetX(AlignmentTarget.Mouse, StyleState.Normal);
            itemElement.style.SetAlignmentTargetY(AlignmentTarget.Mouse, StyleState.Normal);
            itemElement.style.SetAlignmentOffsetX(-offset.x, StyleState.Normal);
            itemElement.style.SetAlignmentOffsetY(-offset.y, StyleState.Normal);
        }
        
        public override void Update() { }

        public override void Drop(bool success) {
            itemElement.style.SetAlignmentTargetX(AlignmentTarget.Unset, StyleState.Normal);
            itemElement.style.SetAlignmentTargetY(AlignmentTarget.Unset, StyleState.Normal);
            itemElement.style.SetAlignmentOffsetX(null, StyleState.Normal);
            itemElement.style.SetAlignmentOffsetY(null, StyleState.Normal);
        }
    }
}