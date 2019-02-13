using System;

namespace UIForia.Compilers.AliasSource {

    public class EnumAliasSource<T> : IAliasSource where T : IConvertible {

        private readonly string[] enumKeys;

        public EnumAliasSource() {
            enumKeys = Enum.GetNames(typeof(T));
        }

        public object ResolveAlias(string alias, object data = null) {
            for (int i = 0; i < enumKeys.Length; i++) {
                string key = enumKeys[i];
                if (key == alias) {
                    return Enum.Parse(typeof(T), alias);
                }
            }

            return null;
        }

    }
    
    public class ExternalEnumAliasSource<T> : IAliasSource where T : IConvertible {

        private readonly string[] enumKeys;

        public ExternalEnumAliasSource() {
            enumKeys = Enum.GetNames(typeof(T));
        }

        public object ResolveAlias(string alias, object data = null) {
            for (int i = 0; i < enumKeys.Length; i++) {
                string key = enumKeys[i];
                if (key == alias) {
                    return Enum.Parse(typeof(T), alias);
                }
            }

            return null;
        }

    }

}