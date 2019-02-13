using UIForia.Compilers.AliasSource;

namespace UIForia.Compilers {

    public class ValueAliasSource<T> : IAliasSource {

        private readonly object boxedValue;
        private readonly string targetName;

        public ValueAliasSource(string aliasName, T value) {
            this.targetName = aliasName;
            this.boxedValue = value;
        }
        
        public object ResolveAlias(string alias, object data = null) {
            if (alias == targetName) {
                return boxedValue;
            }

            return null;
        }

    }

}