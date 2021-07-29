using System;

namespace UIForia {

    public struct UISizeConstraint : IEquatable<UISizeConstraint> {

        public readonly float value;
        public readonly UISizeConstraintUnit unit;

        public UISizeConstraint(float value, UISizeConstraintUnit unit = UISizeConstraintUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public static implicit operator UISizeConstraint(float value) {
            return new UISizeConstraint(value);
        }

        public bool Equals(UISizeConstraint other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            return obj is UISizeConstraint other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        public static string ToStyleString(UISizeConstraint size) {
            string unit;
            
            switch (size.unit) {

                case UISizeConstraintUnit.Pixel:
                    unit = "px";
                    break;

                case UISizeConstraintUnit.Em:
                    unit = "em";
                    break;

                case UISizeConstraintUnit.Content:
                    unit = "cnt";
                    break;

                case UISizeConstraintUnit.MaxChild:
                    unit = "mx";
                    break;

                case UISizeConstraintUnit.MinChild:
                    unit = "mn";
                    break;

                case UISizeConstraintUnit.BackgroundImageWidth:
                    unit = "bw";
                    break;

                case UISizeConstraintUnit.BackgroundImageHeight:
                    unit = "bh";
                    break;

                case UISizeConstraintUnit.ApplicationWidth:
                    unit = "aw";
                    break;

                case UISizeConstraintUnit.ApplicationHeight:
                    unit = "ah";
                    break;

                case UISizeConstraintUnit.ViewportWidth:
                    unit = "vw";
                    break;

                case UISizeConstraintUnit.ViewportHeight:
                    unit = "vh";
                    break;

                case UISizeConstraintUnit.ParentSize:
                    unit = "psz";
                    break;
                case UISizeConstraintUnit.Percent:
                    unit = "%";
                    break;
                default:
                    unit = "unknown(px)";
                    break;
            }

            return size.value + unit;
        }

    }

    public enum UISizeConstraintUnit {

        Pixel = Unit.Pixel,
        Em = Unit.Em,
        LineHeight = Unit.LineHeight,
        
        Content = Unit.Content,
        MaxChild = Unit.MaxChild,
        MinChild = Unit.MinChild,

        BackgroundImageWidth = Unit.BackgroundImageWidth,
        BackgroundImageHeight = Unit.BackgroundImageHeight,
        ApplicationWidth = Unit.ApplicationWidth,
        ApplicationHeight = Unit.ApplicationHeight,
        ViewportWidth = Unit.ViewportWidth,
        ViewportHeight = Unit.ViewportHeight,
        
        ParentSize = Unit.ParentSize,
        Percent = Unit.Percent,

    }

}