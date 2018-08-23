using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

namespace Rendering {

    public struct BorderRadius {

        public readonly float topLeft;
        public readonly float topRight;
        public readonly float bottomLeft;
        public readonly float bottomRight;

        public BorderRadius(float radius) {
            this.topLeft = radius;
            this.topRight = radius;
            this.bottomRight = radius;
            this.bottomLeft = radius;
        }

        public BorderRadius(float topLeftRight, float bottomLeftRight) {
            this.topLeft = topLeftRight;
            this.topRight = topLeftRight;
            this.bottomRight = bottomLeftRight;
            this.bottomLeft = bottomLeftRight;
        }

        public BorderRadius(float topLeft, float topRight, float bottomRight, float bottomLeft) {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
        }

        [PublicAPI]
        public bool HasTopLeft => FloatUtil.IsDefined(topLeft);

        [PublicAPI]
        public bool HasTopRight => FloatUtil.IsDefined(topRight);

        [PublicAPI]
        public bool HasBottomLeft => FloatUtil.IsDefined(bottomLeft);

        [PublicAPI]
        public bool HasBottomRight => FloatUtil.IsDefined(bottomRight);

        [PublicAPI]
        [DebuggerStepThrough]
        public bool IsDefined() {
            return HasTopLeft || HasTopRight || HasBottomRight || HasBottomLeft;
        }

        [PublicAPI]
        [DebuggerStepThrough]
        public bool Equals(BorderRadius other) {
            return topLeft == other.topLeft
                   && topRight == other.topRight
                   && bottomRight == other.bottomRight
                   && bottomLeft == other.bottomLeft;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BorderRadius && Equals((BorderRadius) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = topLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ topRight.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomRight.GetHashCode();
                return hashCode;
            }
        }

        [PublicAPI]
        public static BorderRadius Unset => new BorderRadius(FloatUtil.UnsetFloatValue);

        [DebuggerStepThrough]
        public static implicit operator BorderRadius(Vector4 vec4) {
            return new BorderRadius(vec4.x, vec4.y, vec4.z, vec4.w);
        }

        [DebuggerStepThrough]
        public static implicit operator Vector4(BorderRadius radius) {
            return new Vector4(
                FloatUtil.IsDefined(radius.topLeft) ? radius.topLeft : 0,
                FloatUtil.IsDefined(radius.topRight) ? radius.topRight : 0,
                FloatUtil.IsDefined(radius.bottomRight) ? radius.bottomRight : 0,
                FloatUtil.IsDefined(radius.bottomLeft) ? radius.bottomLeft : 0
            );
        }

        [DebuggerStepThrough]
        public static bool operator ==(BorderRadius self, BorderRadius other) {
            return self.Equals(other);
        }

        [DebuggerStepThrough]
        public static bool operator !=(BorderRadius self, BorderRadius other) {
            return !self.Equals(other);
        }

        public float[] ToFloatArray() {
            return new [] {topLeft, topRight, bottomRight, bottomLeft};
        }

    }

}