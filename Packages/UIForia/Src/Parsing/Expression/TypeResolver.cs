using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class TypeResolver {

        private static Dictionary<string, LightList<Assembly>> s_NamespaceMap;
        private static Stats stats;
        
       [ThreadStatic]  private static string[] s_SingleNamespace;

        public struct Stats {

            public int namespaceCount;
            public int assemblyCount;
            public int typeCount;
            public double namespaceScanTime;

        }

        public static Stats Initialize() {
            if (s_NamespaceMap != null) {
                return stats;
            }

            FilterAssemblies();
            return stats;
        }

        private static void FilterAssemblies() {
            // todo -- could be optimized by writing a tmp file with all non package/asset assemblies namespaces
            
            if (s_NamespaceMap != null) return;

            s_NamespaceMap = new Dictionary<string, LightList<Assembly>>();

            Stopwatch watch = Stopwatch.StartNew();

            // Note, I tried to do this in parallel but assembly.GetTypes() must be locked or something since
            // the performance of doing this in parallel was worse than doing it single threaded
            // this can be improved if we gather all types single threaded and scan namespaces multi threaded

            Assembly current = Assembly.GetExecutingAssembly();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies) {
                stats.assemblyCount++;

                if (assembly.IsDynamic || assembly.ReflectionOnly || assembly.FullName.StartsWith("nunit.framework", StringComparison.Ordinal)) {
                    continue;
                }

                try {

                    Type[] types = assembly == current ? assembly.GetTypes() : assembly.GetExportedTypes();

                    for (int j = 0; j < types.Length; j++) {
                        Type currentType = types[j];
                        // can be null if assembly referenced is unavailable
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse

                        // nested types are always in their parent namespace, no need to scan
                        if (currentType == null || !currentType.IsPublic || currentType.IsNested) {
                            continue;
                        }

                        stats.typeCount++;

                        if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> list)) {
                            list = new LightList<Assembly>(4);
                            s_NamespaceMap.Add(currentType.Namespace ?? "null", list);
                            stats.namespaceCount++;
                        }

                        if (!list.Contains(assembly)) {
                            list.Add(assembly);
                        }
                    }
                }
                catch (ReflectionTypeLoadException e) {
                    UnityEngine.Debug.LogError($"{assembly.FullName} -> {e.Message}");
                }
            }

            watch.Stop();
            stats.namespaceScanTime = watch.Elapsed.TotalMilliseconds;
        }

        public static Type ResolveTypeExpression(Type invokingType, IList<string> namespaces, string typeExpression) {

            if (TypeParser.TryParseTypeName(typeExpression, out TypeLookup typeLookup)) {
                return ResolveType(typeLookup, (IReadOnlyList<string>) namespaces, invokingType);
            }

            return null;
        }

        public static Type ResolveType(string typeName, string namespaceName) {
            s_SingleNamespace = s_SingleNamespace ?? new string[1];
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(new TypeLookup(typeName), s_SingleNamespace);
        }

        public static Type ResolveType(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            if (typeLookup.resolvedType != null) {
                return typeLookup.resolvedType;
            }

            FilterAssemblies();

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces);

            if (typeLookup.generics.array != null && typeLookup.generics.size != 0) {
                if (!baseType.IsGenericTypeDefinition) {
                    throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
                }

                baseType = ReflectionUtil.CreateGenericType(baseType, ResolveGenericTypes(typeLookup, namespaces, scopeType));
            }

            if (typeLookup.isArray) {
                baseType = baseType.MakeArrayType();
            }

            return baseType;
        }

        public static Type ResolveNestedGenericType(Type containingType, Type baseType, TypeLookup typeLookup, LightList<string> namespaces) {
            FilterAssemblies();

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (typeLookup.generics.array == null || typeLookup.generics.size == 0) {
                throw new TypeResolutionException($"Tried to resolve generic types from {baseType} but no generic types were given in the {nameof(typeLookup)} argument");
            }

            return ReflectionUtil.CreateNestedGenericType(containingType, baseType, ResolveGenericTypes(typeLookup, namespaces));
        }

        public static Type ResolveType(string typeName, IReadOnlyList<string> namespaces) {
            FilterAssemblies();

            for (int i = 0; i < namespaces.Count; i++) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                if (assemblies == null) {
                    continue;
                }

                string prefixedTypeName = namespaces[i] + "." + typeName + ", ";
                foreach (Assembly assembly in assemblies) {
                    string fullTypeName = prefixedTypeName + assembly.FullName;

                    Type retn = Type.GetType(fullTypeName);

                    if (retn != null) {
                        return retn;
                    }
                }
            }

            LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
            if (lastDitchAssemblies != null) {
                string typename = typeName;
                for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                    Type retn = lastDitchAssemblies[a].GetType(typename);
                    if (retn != null) {
                        return retn;
                    }
                }
            }

            return null;
        }

        private static Type[] ResolveGenericTypes(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            int count = typeLookup.generics.size;

            // new array because this is called from multiple threads
            Type[] array = new Type[count];
            Type[] generics = null;
            Type[] concreteArgs = null;
            if (scopeType != null) {
                if (scopeType.IsGenericType) {
                    generics = scopeType.GetGenericTypeDefinition().GetGenericArguments();
                    concreteArgs = scopeType.GetGenericArguments();
                }
            }

            for (int i = 0; i < count; i++) {
                if (generics != null) {
                    for (int j = 0; j < generics.Length; j++) {
                        if (typeLookup.generics[i].typeName == generics[j].Name) {
                            array[i] = concreteArgs[i];
                            break;
                        }
                    }
                }

                array[i] = array[i] ?? ResolveType(typeLookup.generics[i], namespaces);

                if (array[i] == null) {
                    throw new TypeResolutionException($"Failed to find a type from string {typeLookup.generics[i]}");
                }
            }

            return array;
        }

        private static Type ResolveSimpleType(string typeName) {
            switch (typeName) {
                case "bool": return typeof(bool);
                case "byte": return typeof(byte);
                case "sbyte": return typeof(sbyte);
                case "char": return typeof(char);
                case "decimal": return typeof(decimal);
                case "double": return typeof(double);
                case "float": return typeof(float);
                case "single": return typeof(float);
                case "int": return typeof(int);
                case "uint": return typeof(uint);
                case "long": return typeof(long);
                case "ulong": return typeof(ulong);
                case "object": return typeof(object);
                case "short": return typeof(short);
                case "ushort": return typeof(ushort);
                case "string": return typeof(string);
            }

            return null;
        }

        private static Type ResolveBaseTypePath(TypeLookup typeLookup, IReadOnlyList<string> namespaces) {
            Type retn = ResolveSimpleType(typeLookup.typeName);

            if (retn != null) {
                return retn;
            }

            string baseTypeName = "." + typeLookup.GetBaseTypeName(); // save some string concat
            if (!string.IsNullOrEmpty(typeLookup.namespaceName)) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(typeLookup.namespaceName);

                if (assemblies == null) {
                    throw new TypeResolutionException($"No loaded assemblies found for namespace {typeLookup.namespaceName}");
                }

                string typename = typeLookup.namespaceName + baseTypeName;

                for (int a = 0; a < assemblies.Count; a++) {
                    retn = assemblies[a].GetType(typename);
                    if (retn != null) {
                        return retn;
                    }
                }

                LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
                if (lastDitchAssemblies != null) {
                    typename = typeLookup.typeName;
                    for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                        retn = lastDitchAssemblies[a].GetType(typename);
                        if (retn != null) {
                            return retn;
                        }
                    }
                }
            }
            else {
                if (namespaces != null) {
                    for (int i = 0; i < namespaces.Count; i++) {
                        LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                        if (assemblies == null) {
                            continue;
                        }

                        string typename = namespaces[i] + baseTypeName;
                        for (int a = 0; a < assemblies.Count; a++) {
                            retn = assemblies[a].GetType(typename);
                            if (retn != null) {
                                return retn;
                            }
                        }
                    }
                }

                LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
                if (lastDitchAssemblies != null) {
                    string typename = typeLookup.typeName;
                    for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                        retn = lastDitchAssemblies[a].GetType(typename);
                        if (retn != null) {
                            return retn;
                        }
                    }
                }
            }

            if (namespaces != null && namespaces.Count > 0 && namespaces[0] != string.Empty) {
                string checkedNamespaces = string.Join(",", namespaces.ToArray());
                throw new TypeResolutionException($"Unable to resolve type {typeLookup}. Looked in namespaces: {checkedNamespaces}");
            }

            throw new TypeResolutionException($"Unable to resolve type {typeLookup}.");
        }

        public class TypeResolutionException : Exception {

            public TypeResolutionException(string message) : base(message) { }

        }

        public static bool IsNamespace(string check) {
            return s_NamespaceMap.ContainsKey(check);
        }

    }

}