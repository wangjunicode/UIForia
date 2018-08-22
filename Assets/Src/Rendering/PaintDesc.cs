using Src;
using UnityEngine;

namespace Rendering {

    public struct Paint {

        public  Color backgroundColor;
        public  Color borderColor;
        public  Texture2D backgroundImage;
        
        public static readonly Paint Unset = new Paint(
            UIStyle.UnsetColorValue, 
            UIStyle.UnsetColorValue
        );

        // todo -- can compress these by merging units & accessing via property bit mask
//        public readonly UIMeasurement borderRadiusTopLeft;
//        public readonly UIMeasurement borderRadiusTopRight;
//        public readonly UIMeasurement borderRadiusBottomLeft;
//        public readonly UIMeasurement borderRadiusBottomRight;

        public Paint(Color backgroundColor, Color borderColor, Texture2D backgroundImage = null) {
            this.backgroundColor = backgroundColor;
            this.borderColor = borderColor;
            this.backgroundImage = backgroundImage;
//            borderRadiusTopLeft = UIStyle.UnsetMeasurementValue; 
//            borderRadiusTopRight = UIStyle.UnsetMeasurementValue;
//            borderRadiusBottomLeft = UIStyle.UnsetMeasurementValue;
//            borderRadiusBottomRight = UIStyle.UnsetMeasurementValue;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Paint && this == ((Paint) obj);
        }

        public bool Equals(Paint other) {
            return backgroundColor.Equals(other.backgroundColor) && borderColor.Equals(other.borderColor) && Equals(backgroundImage, other.backgroundImage);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = backgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ borderColor.GetHashCode();
                hashCode = (hashCode * 397) ^ (backgroundImage != null ? backgroundImage.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Paint self, Paint other) {
            return self.backgroundColor == other.backgroundColor
                   && self.backgroundImage == other.backgroundImage
                   && self.borderColor == other.borderColor;
        }

        public static bool operator !=(Paint self, Paint other) {
            return !(self == other);
        }

    }

}