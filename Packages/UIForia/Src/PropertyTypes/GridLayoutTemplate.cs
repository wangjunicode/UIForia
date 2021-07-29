using System;
namespace UIForia {

    internal struct GridTemplate {

        public int offset;
        public int cellCount;

    }

    public struct GridLayoutTemplate : IEquatable<GridLayoutTemplate> {

        public readonly ushort templateId;

        public GridLayoutTemplate(ushort templateId) {
            this.templateId = templateId;
        }

        public bool Equals(GridLayoutTemplate other) {
            return templateId == other.templateId;
        }

        public override bool Equals(object obj) {
            return obj is GridLayoutTemplate other && Equals(other);
        }

        public override int GetHashCode() {
            return templateId.GetHashCode();
        }

    }

}