using System;

namespace UIForia {

    public struct ModuleAndNameKey : IEquatable<ModuleAndNameKey> {

        public readonly string moduleName;
        public readonly string name;

        public ModuleAndNameKey(string moduleName, string name) {
            this.moduleName = moduleName;
            this.name = name;
        }

        public bool Equals(ModuleAndNameKey other) {
            return moduleName == other.moduleName && name == other.name;
        }

        public override bool Equals(object obj) {
            return obj is ModuleAndNameKey other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return ((moduleName != null ? moduleName.GetHashCode() : 0) * 397) ^ (name != null ? name.GetHashCode() : 0);
            }
        }

    }

}