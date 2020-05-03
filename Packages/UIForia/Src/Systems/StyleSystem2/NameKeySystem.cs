using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;

namespace UIForia {

    [DebuggerDisplay("{Resolve()}")]
    public struct NameKey {

        public int key;

        public NameKey(int value) {
            this.key = value;
        }

        [BurstDiscard]
        public string Resolve() {
            return NameKeySystem.Resolve(this);
        }

        public static implicit operator NameKey(int key) {
            return new NameKey(key);
        }

        public static implicit operator int(NameKey nameKey) {
            return nameKey.key;
        }

    }

    public static class NameKeySystem {

        private static readonly Dictionary<string, int> s_StringInternTable = new Dictionary<string, int>(64);
        private static readonly Dictionary<int, string> s_StringInternTableResolve = new Dictionary<int, string>(64);

        public static string Resolve(NameKey nameKey) {
            return !s_StringInternTableResolve.TryGetValue(nameKey.key, out string retn) ? "name-key-not-found" : retn;
        }

        public static NameKey GetNameKey(string name) {
            if (!s_StringInternTable.TryGetValue(name, out int nameKey)) {
                nameKey = s_StringInternTable.Count + 1;
                s_StringInternTable.Add(name, nameKey);
                s_StringInternTableResolve.Add(nameKey, name);
            }

            return new NameKey(nameKey);
        }

        public static bool TryGetNameKey(string name, out NameKey nameKey) {
            if (!s_StringInternTable.TryGetValue(name, out int key)) {
                nameKey = default;
                return false;
            }

            nameKey = new NameKey(key);
            return true;
        }

    }

}