using UnityEngine;

namespace UIForia.Util {

    public struct OffsetRect {

        public readonly float top;
        public readonly float right;
        public readonly float bottom;
        public readonly float left;
        
        public OffsetRect(float top, float right, float bottom, float left) {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }

        public float Horizontal => left + right;

        public float Vertical => top + bottom;
        public bool IsZero => left + right + top + bottom == 0;

        public bool IsUniform {
            get { return top == right && top == left && top == bottom; }
        }

        public static implicit operator Vector4(OffsetRect rect) {
            return new Vector4(rect.top, rect.right, rect.bottom, rect.left);
        }
    }

}