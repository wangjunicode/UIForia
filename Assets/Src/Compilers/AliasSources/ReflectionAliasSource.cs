using System;
using System.Reflection;

namespace Src.Compilers.AliasSource {

    public class ReflectionAliasSource : IAliasSource {

        private readonly Type type;
        private FieldInfo[] fields;
        private MethodInfo[] methods;

        public ReflectionAliasSource(Type type) {
            this.type = type;
        }

        public object ResolveAlias(string alias, object data = null) {
            if (fields == null || methods == null) {
                fields = type.GetFields(ReflectionUtil.InterfaceBindFlags);
                methods = type.GetMethods(ReflectionUtil.InterfaceBindFlags);
            }

            for (int i = 0; i < fields.Length; i++) {
                if (fields[i].Name == alias) {
                    return fields[i];
                }
            }

            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name == alias) {
                    return methods[i];
                }
            }

            return null;
        }

    }

}