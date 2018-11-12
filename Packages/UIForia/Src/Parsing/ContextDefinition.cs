using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Compilers.AliasSource;

namespace UIForia {

    public class ContextDefinition {

        private const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly List<ExpressionAlias<Type>> runtimeTypeAliases;
        private readonly List<IAliasSource> constAliasSources;

        public readonly Type rootType;

        public ContextDefinition(Type rootType) {
            this.rootType = rootType;
            this.runtimeTypeAliases = new List<ExpressionAlias<Type>>();
            this.constAliasSources = new List<IAliasSource>();
//            this.constAliasSources.Add(new ReflectionAliasSource(rootType));
        }

        public object ResolveConstAlias(string name, object data = null) {
            for (int i = 0; i < constAliasSources.Count; i++) {
                object value = constAliasSources[i].ResolveAlias(name, data);
                if (value != null) {
                    return value;
                }
            }

            return null;
        }

        // todo -- maybe add a priority here for sorting
        public void AddConstAliasSource(IAliasSource source) {
            if (constAliasSources.Contains(source)) return;
            constAliasSources.Add(source);
        }

        public void RemoveConstAliasSource(IAliasSource source) {
            constAliasSources.Remove(source);
        }

        public void AddRuntimeAlias(string alias, Type type) {
            for (int i = 0; i < runtimeTypeAliases.Count; i++) {
                if (runtimeTypeAliases[i].name == alias) {
                    runtimeTypeAliases[i] = new ExpressionAlias<Type>(alias, type);
                    return;
                }
            }

            runtimeTypeAliases.Add(new ExpressionAlias<Type>(alias, type));
        }

        public void RemoveRuntimeAlias(string alias) {
            for (int i = 0; i < runtimeTypeAliases.Count; i++) {
                if (runtimeTypeAliases[i].name == alias) {
                    runtimeTypeAliases.RemoveAt(i);
                    return;
                }
            }
        }

        public Type ResolveRuntimeAliasType(string alias) {
            for (int i = 0; i < runtimeTypeAliases.Count; i++) {
                if (runtimeTypeAliases[i].name == alias) {
                    return runtimeTypeAliases[i].value;
                }
            }

            FieldInfo fieldInfo = rootType.GetField(alias, bindFlags);

            if (fieldInfo != null) {
                return fieldInfo.FieldType;
            }

            return null;
        }

    }

}