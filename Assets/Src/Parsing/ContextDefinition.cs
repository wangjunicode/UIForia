using System;
using System.Collections.Generic;
using System.Reflection;

namespace Src {

    public class ContextDefinition {

        private const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private List<Alias<Type>> typeAliases;
        private List<Alias<MethodInfo>> methodAliases;

        public readonly ProcessedType processedType;

        public ContextDefinition(Type type) {
            this.typeAliases = new List<Alias<Type>>();
            this.methodAliases = new List<Alias<MethodInfo>>();
            this.processedType = TypeProcessor.GetType(type);
        }

        public Type rootType => processedType.rawType;

        // todo -- figure out correct binding flags and attributes to support
        public void SetMethodAlias(string alias, MethodInfo info) {
            RemoveAlias(alias);
            
            if (alias[0] != '$') {
                MethodInfo rootInfo = rootType.GetMethod(alias, ReflectionUtil.InstanceBindFlags);
                if (rootInfo != null) {
                    throw new AmbiguousMatchException($"The root type {rootType.Name} defines '{alias}' but an alias also exists for '{alias}'. Use the $ operator to disambiguate");

                }
                FieldInfo fieldInfo = rootType.GetField(alias);
                if (fieldInfo != null) {
                    throw new AmbiguousMatchException($"The root type {rootType.Name} defines '{alias}' but an alias also exists for '{alias}'. Use the $ operator to disambiguate");
                }
            }
            
            for (int i = 0; i < methodAliases.Count; i++) {
                if (methodAliases[i].name == alias) {
                    methodAliases[i] = new Alias<MethodInfo>(alias, info);
                    return;
                }
            }
            methodAliases.Add(new Alias<MethodInfo>(alias, info));
        }

        public void RemoveMethodAlias(string alias) {
            for (int i = 0; i < methodAliases.Count; i++) {
                if (methodAliases[i].name == alias) {
                    methodAliases.RemoveAt(i);
                    return;
                }
            }
        }

        public void SetAliasToType(string alias, Type type) {
            for (int i = 0; i < typeAliases.Count; i++) {
                if (typeAliases[i].name == alias) {
                    typeAliases[i] = new Alias<Type>(alias, type);
                    return;
                }
            }
            typeAliases.Add(new Alias<Type>(alias, type));
        }

        public void RemoveAlias(string alias) {
            for (int i = 0; i < typeAliases.Count; i++) {
                if (typeAliases[i].name == alias) {
                    typeAliases.RemoveAt(i);
                    return;
                }
            }
        }

        public Type ResolveType(string alias) {
            for (int i = 0; i < typeAliases.Count; i++) {
                if (typeAliases[i].name == alias) {
                    return typeAliases[i].value;
                }
            }

            FieldInfo fieldInfo = rootType.GetField(alias, bindFlags);

            if (fieldInfo != null) {
                return fieldInfo.FieldType;
            }

            return null;
        }

        // todo -- don't allow overloads
        public MethodInfo ResolveMethod(string methodName) {

            MethodInfo rootInfo = rootType.GetMethod(methodName, ReflectionUtil.InstanceBindFlags);

            Alias<MethodInfo> aliasedInfo = methodAliases.Find((a) => a.name == methodName);
            if (rootInfo != null && aliasedInfo.value != null) {
                throw new AmbiguousMatchException($"The root type {rootType.Name} defines '{methodName}' but an alias also exists for '{methodName}'. Use the $ operator to disambiguate");
            }

            return rootInfo != null ? rootInfo : aliasedInfo.value;
        }

        private struct Alias<T> {

            public readonly T value;
            public readonly string name;

            public Alias(string name, T value) {
                this.name = name;
                this.value = value;
            }

        }

    }

}