using System;
using System.Reflection;

namespace Src.Compilers.AliasSource {

    public class MethodAliasSource : IAliasSource {

        private readonly string name;
        private readonly MethodInfo info;

        public MethodAliasSource(string name, MethodInfo info) {
            this.name = name;
            this.info = info;
        }

        public object ResolveAlias(string alias, object data = null) {
            if (alias == name) {
                Type[] parameterTypes = data as Type[];
                if (parameterTypes == null) return null;
                
                // todo -- this is NOT safe long term, should check types but will need to handle downcasting / compatibility
                if (parameterTypes.Length == info.GetParameters().Length) {
                    return info;
                }
            }

            return null;
        }

    }

}