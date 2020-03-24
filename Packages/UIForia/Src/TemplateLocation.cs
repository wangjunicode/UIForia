using System;

namespace UIForia {

    public struct TemplateLocation {

        public readonly string filePath;
        public readonly string templateId;

        public TemplateLocation(string filePath, string templateId = null) {
            this.filePath = filePath;
            this.templateId = templateId;
        }

        public static implicit operator TemplateLocation(string value) {
            return new TemplateLocation(value);
        }

    }
    
    internal struct ResolvedTemplateLocation : IComparable<ResolvedTemplateLocation>, IEquatable<ResolvedTemplateLocation> {

        internal readonly Module module;
        internal readonly string filePath;

        public ResolvedTemplateLocation(Module module, string filePath) {
            this.module = module;
            this.filePath = filePath;
        }

        public int CompareTo(ResolvedTemplateLocation other) {
            return string.Compare(filePath, other.filePath, StringComparison.Ordinal);
        }

        public bool Equals(ResolvedTemplateLocation other) {
            return Equals(module, other.module) && filePath == other.filePath;
        }

        public override bool Equals(object obj) {
            return obj is ResolvedTemplateLocation other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return ((module != null ? module.GetHashCode() : 0) * 397) ^ (filePath != null ? filePath.GetHashCode() : 0);
            }
        }

    }

}