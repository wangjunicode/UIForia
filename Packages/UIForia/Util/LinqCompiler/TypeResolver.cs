using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Parsing.Expressions;

namespace UIForia.Util {

    public class TypeResolver {

        private static readonly string[] s_SingleNamespace = new string[1];
        private readonly Dictionary<string, LightList<Assembly>> s_NamespaceMap = new Dictionary<string, LightList<Assembly>>();

        private static TypeResolver defaultResolver;

        private Assembly[] assemblies;
        private static Assembly[] s_AllAssemblies;

        // todo! I totally broke this, need to circle back and fix it later. should configurable to filter out assemblies, only gather public types, and invoke a type processor callback for class types

        public TypeResolver(IList<Assembly> assemblies = null) {
            if (assemblies == null) {
                this.assemblies = AppDomain.CurrentDomain.GetAssemblies().ToArray();
            }
            else {
                this.assemblies = assemblies.ToArray();
            }
        }

        public static TypeResolver Default {
            get {
                if (defaultResolver != null) {
                    return defaultResolver;
                }
                defaultResolver = new TypeResolver();
                return defaultResolver;
            }
        }

        private void FilterAssemblies() {
            if (s_NamespaceMap != null) return;
            
            // todo -- parallel this 
            
            for (int i = 0; i < assemblies.Length; i++) {
                
                Assembly assembly = assemblies[i];

                if (ShouldFilterAssembly(assembly)) {
                    continue;
                }

                try { }
                catch (Exception e) { }

            }
        }

        private bool ShouldFilterAssembly(Assembly assembly) {
            string name = assembly.FullName;

            if (assembly.IsDynamic ||
                name.StartsWith("System,") ||
                name.StartsWith("Accessibility") ||
                name.StartsWith("Boo") ||
                name.StartsWith("I18N") ||
                name.StartsWith("TextMeshPro") ||
                name.StartsWith("nunit") ||
                name.StartsWith("System.") ||
                name.StartsWith("Microsoft.") ||
                name.StartsWith("Mono") ||
                name.StartsWith("Unity.") ||
                name.StartsWith("ExCSS.") ||
                name.Contains("mscorlib") ||
                name.Contains("JetBrains") ||
                name.Contains("UnityEngine") ||
                name.Contains("UnityEditor") ||
                name.Contains("Jetbrains")) {
                return false;
            }

            return name.IndexOf("-firstpass", StringComparison.Ordinal) == -1;
        }

        public Type ResolveTypeExpression(Type invokingType, IList<string> namespaces, string typeExpression) {
            typeExpression = typeExpression.Replace("[", "<").Replace("]", ">");
            if (ExpressionParser.TryParseTypeName(typeExpression, out TypeLookup typeLookup)) {
                return ResolveType(typeLookup, (IReadOnlyList<string>) namespaces, invokingType);
            }

            return null;
        }

        public bool IsNamespace(string toCheck) {
            FilterAssemblies();
            return s_NamespaceMap.ContainsKey(toCheck);
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

        private LightList<Type> ResolveGenericTypes(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            int count = typeLookup.generics.size;

            LightList<Type> results = LightList<Type>.Get();
            results.EnsureCapacity(count);

            Type[] array = results.array;
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

            results.Count = typeLookup.generics.size;
            return results;
        }

        private Type ResolveBaseTypePath(TypeLookup typeLookup, IReadOnlyList<string> namespaces) {
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
            else {
                throw new TypeResolutionException($"Unable to resolve type {typeLookup}.");
            }
        }

        public Type ResolveType(string typeName, string namespaceName) {
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(new TypeLookup(typeName), s_SingleNamespace);
        }

        public Type ResolveType(TypeLookup typeLookup, string namespaceName) {
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(typeLookup, s_SingleNamespace);
        }

        public Type ResolveType(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            if (typeLookup.resolvedType != null) {
                return typeLookup.resolvedType;
            }

            FilterAssemblies();

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces);

            if (typeLookup.generics != null && typeLookup.generics.Count != 0) {
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

        public Type ResolveNestedGenericType(Type containingType, Type baseType, TypeLookup typeLookup, LightList<string> namespaces) {
            FilterAssemblies();

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (typeLookup.generics == null || typeLookup.generics.Count == 0) {
                throw new TypeResolutionException($"Tried to resolve generic types from {baseType} but no generic types were given in the {nameof(typeLookup)} argument");
            }

            return ReflectionUtil.CreateNestedGenericType(containingType, baseType, ResolveGenericTypes(typeLookup, namespaces));
        }

        public Type ResolveType(string typeName, IReadOnlyList<string> namespaces) {
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

    }

    public class TypeResolutionException : Exception {

        public TypeResolutionException(string message) : base(message) { }

    }

}