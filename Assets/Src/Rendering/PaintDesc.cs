using UnityEngine;

namespace Rendering {

    public class PaintDesc {

        public Color backgroundColor;
        public Color borderColor;
        public Texture2D backgroundImage;

        public PaintDesc() {
            borderColor = UIStyle.UnsetColorValue;
            backgroundColor = UIStyle.UnsetColorValue;
        }
        
        public PaintDesc Clone() {
            PaintDesc clone = new PaintDesc();
            clone.backgroundImage = backgroundImage;
            clone.backgroundColor = backgroundColor;
            return clone;
        }

    }

}