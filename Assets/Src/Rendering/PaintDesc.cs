using Src;
using UnityEngine;

namespace Rendering {

    public class PaintDesc {

        public Color backgroundColor;
        public Color borderColor;
        public Texture2D backgroundImage;
        
        public UIMeasurement borderRadiusTopLeft;
        public UIMeasurement borderRadiusTopRight;
        public UIMeasurement borderRadiusBottomLeft;
        public UIMeasurement borderRadiusBottomRight;
        
        public PaintDesc() {
            borderColor = UIStyle.UnsetColorValue;
            backgroundColor = UIStyle.UnsetColorValue;
            borderRadiusTopLeft = UIStyle.UnsetMeasurementValue;
            borderRadiusTopRight = UIStyle.UnsetMeasurementValue;
            borderRadiusBottomLeft = UIStyle.UnsetMeasurementValue;
            borderRadiusBottomRight = UIStyle.UnsetMeasurementValue;
        }
        
        public PaintDesc Clone() {
            PaintDesc clone = new PaintDesc();
            
            clone.backgroundImage = backgroundImage;
            clone.backgroundColor = backgroundColor;
            clone.borderColor = borderColor;
            
            clone.borderRadiusTopLeft = borderRadiusTopLeft;
            clone.borderRadiusTopRight = borderRadiusTopRight;
            clone.borderRadiusBottomLeft = borderRadiusBottomLeft;
            clone.borderRadiusBottomRight = borderRadiusBottomRight;
            
            return clone;
        }

    }

}