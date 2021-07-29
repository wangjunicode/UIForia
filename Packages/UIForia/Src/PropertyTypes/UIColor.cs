using System;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    public struct UIColor : IEquatable<UIColor> {

        public half r;
        public half g;
        public half b;
        public half a;

        public UIColor(half r, half g, half b, half a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public bool Equals(UIColor other) {
            // todo memcp instead
            return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
        }

        public override bool Equals(object obj) {
            return obj is UIColor other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = r.GetHashCode();
                hashCode = (hashCode * 397) ^ g.GetHashCode();
                hashCode = (hashCode * 397) ^ b.GetHashCode();
                hashCode = (hashCode * 397) ^ a.GetHashCode();
                return hashCode;
            }
        }
        public static implicit operator UIColor(Color32 color) {
            return new UIColor() {
                r = (half)(color.r / 255f),
                g = (half)(color.g / 255f),
                b = (half)(color.b / 255f),
                a = (half)(color.a / 255f)
            };
        }
        
        public static implicit operator UIColor(Color color) {
            return new UIColor() {
                r = (half)color.r,
                g = (half)color.g,
                b = (half)color.b,
                a = (half)color.a
            };
        }
        
        public static implicit operator Color(UIColor color) {
            return new Color() {
                r = (float)color.r,
                g = (float)color.g,
                b = (float)color.b,
                a = (float)color.a
            };
        }

        public override string ToString() {
            return ((Color) this).ToString();
        }

    }

}