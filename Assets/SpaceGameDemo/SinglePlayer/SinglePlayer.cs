using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SpaceGameDemo.SinglePlayer {
    public class ShipData {
        public int id;
        public string name;

        public ShipData(int id, string name) {
            this.id = id;
            this.name = name;
        }
    }

    [Template("SpaceGameDemo/SinglePlayer/SinglePlayer.xml")]
    public class SinglePlayer : UIElement {
        public int selectedShip = 1;
        public List<ShipData> ships;
        // Property
        public float progress;

        // Property
        public string progressString;

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
        }

        public void SelectShip(int id) {
            selectedShip = id;
        }
    }
}