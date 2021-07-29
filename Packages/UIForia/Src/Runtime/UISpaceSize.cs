using System;

namespace UIForia {

    public enum LayoutFillOrder : byte {

        Forward = 0,
        Reverse = 1

    }

    [Flags]
    public enum SpaceCollapse : byte {

        None = 0,
        RemoveInner = 1 << 2,
        RemoveOuter = 1 << 3,
        CollapseOuter = 1 << 4,
        CollapseInner = 1 << 5,
        
        Collapse = CollapseInner | CollapseOuter,
        Remove = RemoveInner | RemoveOuter,

    }

    [AssertSize(8)]
    public struct UISpaceSize : IEquatable<UISpaceSize> {

        public readonly float fixedValue;
        public readonly ushort stretch;
        public readonly UISpaceSizeUnit unit;

        public UISpaceSize(float fixedValue, UISpaceSizeUnit unit = UISpaceSizeUnit.Pixel, ushort stretch = 0) {
            this.fixedValue = fixedValue;
            this.stretch = stretch;
            this.unit = unit;
        }

        public UISpaceSize(float fixedValue, ushort stretch = 0) {
            this.fixedValue = fixedValue;
            this.stretch = stretch;
            this.unit = UISpaceSizeUnit.Pixel;
        }

        public static implicit operator UISpaceSize(float val) {
            return new UISpaceSize(val, 0);
        }

        public bool Equals(UISpaceSize other) {
            return fixedValue.Equals(other.fixedValue) && stretch == other.stretch && unit == other.unit;
        }

        public override bool Equals(object obj) {
            return obj is UISpaceSize other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = fixedValue.GetHashCode();
                hashCode = (hashCode * 397) ^ stretch.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) unit;
                return hashCode;
            }
        }

    }

    public enum UISpaceSizeUnit : ushort {

        Unset = 0,
        Pixel = Unit.Pixel,
        Em = Unit.Em,
        Stretch = Unit.Stretch,
        ViewportWidth = Unit.ViewportWidth,
        ViewportHeight = Unit.ViewportHeight,
        ApplicationWidth = Unit.ApplicationWidth,
        ApplicationHeight = Unit.ApplicationHeight,

    }

}