using UnityEngine;

namespace ThisOtherThing.UI {

    public struct OutlineShapeProperties {

        public Color fillColor;
        public bool DrawFill;
        public bool DrawFillShadow;

        public bool DrawOutline;
        public Color32 OutlineColor;
        public bool DrawOutlineShadow;

        public static OutlineShapeProperties CreateDefault() {
            return new OutlineShapeProperties() {
                DrawFill = true,
                DrawFillShadow = true,
                DrawOutline = false,
                DrawOutlineShadow = false,
                OutlineColor = new Color32(0, 0, 0, 255),
                fillColor = new Color32(255, 255, 255, 255),
            };
        }

    }

}