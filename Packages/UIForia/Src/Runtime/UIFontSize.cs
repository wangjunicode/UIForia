using System;
using UIForia.Layout;

namespace UIForia {

    public struct UIFontSize : IEquatable<UIFontSize> {

        public readonly float value;
        public readonly UIFontSizeUnit unit;

        public UIFontSize(float value, UIFontSizeUnit unit = UIFontSizeUnit.Pixel) {
            if (value < 0) value = 0;
            this.value = value;
            this.unit = unit;
        }

        public static implicit operator UIFontSize(float value) {
            return new UIFontSize(value);
        }

        public bool Equals(UIFontSize other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            return obj is UIFontSize other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

    }

}

namespace UIForia.Layout {

    public enum UIFontSizeUnit {

        Default = Unit.Unset,
        Pixel = Unit.Pixel,
        Point = Unit.FontPoint,
        Em = Unit.Em

    }
}