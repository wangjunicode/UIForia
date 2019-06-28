using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Extensions;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing.Expression {

    public static class TypeProcessor {

        public struct TypeData {

            public readonly Type type;
            public readonly string tagName;

            public TypeData(Type type, string tagName) {
                this.type = type;
                this.tagName = tagName;
            }

        }

        public static bool processedTypes;
        private static readonly StructList<ProcessedType> templateTypes = new StructList<ProcessedType>(64);
        public static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        public static readonly Dictionary<string, ProcessedType> templateTypeMap = new Dictionary<string, ProcessedType>();
        public static readonly Dictionary<string, LightList<Assembly>> s_NamespaceMap = new Dictionary<string, LightList<Assembly>>();
        private static readonly string[] s_SingleNamespace = new string[1];


        private static void FilterAssemblies() {
            if (processedTypes) return;
            processedTypes = true;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            int count = 0;
            for (int i = 0; i < assemblies.Length; i++) {
                Assembly assembly = assemblies[i];

                if (assembly == null || assembly.IsDynamic) {
                    continue;
                }

                bool filteredOut = !FilterAssembly(assembly);
                bool shouldProcessTypes = ShouldProcessTypes(assembly, filteredOut);

                if (!shouldProcessTypes) {
                    continue;
                }

                count++;

                try {
                    Type[] types = assembly.GetTypes();

                    for (int j = 0; j < types.Length; j++) {
                        Type currentType = types[j];
                        // can be null if assembly referenced is unavailable
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (currentType == null) {
                            continue;
                        }

                        if (!filteredOut && currentType.IsClass) {
                            Attribute[] attrs = Attribute.GetCustomAttributes(currentType, false);
                            Application.ProcessClassAttributes(currentType, attrs);

                            if (typeof(UIElement).IsAssignableFrom(currentType)) {
                                string tagName = currentType.Name;
                                TemplateAttribute templateAttr = null;

                                for (int index = 0; index < attrs.Length; index++) {
                                    Attribute attr = attrs[index];

                                    if (attr is TemplateTagNameAttribute templateTagNameAttr) {
                                        tagName = templateTagNameAttr.tagName;
                                    }

                                    if (attr is TemplateAttribute templateAttribute) {
                                        templateAttr = templateAttribute;
                                    }
                                }

                                ProcessedType processedType = new ProcessedType(currentType, templateAttr);

                                if (templateAttr != null) {
                                    templateTypes.Add(processedType);
                                }

                                if (templateTypeMap.ContainsKey(tagName)) {
                                    Debug.Log($"Tried to add template key `{tagName}` from type {currentType} but it was already defined by {templateTypeMap.GetOrDefault(tagName).rawType}");
                                }

                                templateTypeMap.Add(tagName, processedType);
                                typeMap[currentType] = processedType;
                            }
                        }

                        if (filteredOut && !currentType.IsPublic) {
                            continue;
                        }

                        if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> list)) {
                            list = new LightList<Assembly>(2);
                            s_NamespaceMap.Add(currentType.Namespace ?? "null", list);
                        }

                        if (!list.Contains(assembly)) {
                            list.Add(assembly);
                        }
                    }
                }
                catch (ReflectionTypeLoadException) {
                    Debug.Log($"{assembly.FullName}");
                    throw;
                }
            }

            watch.Stop();
            Debug.Log($"Loaded types in: {watch.ElapsedMilliseconds} ms from {count} assemblies");
            GC.Collect();
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

        private static Type[] ResolveGenericTypes(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null) {
            Type[] results = new Type[typeLookup.generics.Length];

            for (int i = 0; i < typeLookup.generics.Length; i++) {
                results[i] = ResolveType(typeLookup.generics[i], namespaces);

                if (results[i] == null) {
                    throw new TypeResolutionException($"Failed to find a type from string {typeLookup.generics[i]}");
                }
            }

            return results;
        }

        private static Type ResolveBaseTypePath(TypeLookup typeLookup, IReadOnlyList<string> namespaces) {
            Type retn = ResolveSimpleType(typeLookup.typeName);

            if (retn != null) {
                return retn;
            }

            string baseTypeName = "." + typeLookup.typeName; // save some string concat
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

                throw new TypeResolutionException($"Unable to resolve type {typeLookup}");
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

                throw new TypeResolutionException($"Unable to resolve type {typeLookup}");
            }
        }

        public static Type ResolveType(string typeName, string namespaceName) {
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(new TypeLookup(typeName), s_SingleNamespace);
        }

        public static Type ResolveType(TypeLookup typeLookup, string namespaceName) {
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(typeLookup, s_SingleNamespace);
        }

        public static Type ResolveType(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null) {
            FilterAssemblies();

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces);

            if (typeLookup.generics != null && typeLookup.generics.Length != 0) {
                if (!baseType.IsGenericTypeDefinition) {
                    throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
                }

                Type[] generics = ResolveGenericTypes(typeLookup, namespaces);
                return ReflectionUtil.CreateGenericType(baseType, generics);
            }

            return baseType;
        }

        // todo -- remove this when old expression compiler is no longer used
        public static Type ResolveType(Type originType, string name, IReadOnlyList<string> namespaces) {
            string subtypeName = originType.FullName + "+" + name;
            subtypeName = subtypeName + ", " + originType.Assembly.FullName;
            Type retn = Type.GetType(subtypeName);

            if (retn != null) {
                return retn;
            }

            FilterAssemblies();

            LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(originType.Namespace ?? "null");
            if (assemblies != null) {
                string typeName = originType.Namespace ?? "null" + "." + name + ", ";
                for (int i = 0; i < assemblies.Count; i++) {
                    Assembly assembly = assemblies[i];
                    string fullTypeName = typeName + assembly.FullName;

                    retn = Type.GetType(fullTypeName);

                    if (retn != null) {
                        return retn;
                    }
                }
            }

            if (originType.FullName.Contains("+")) {
                Assembly assembly = originType.Assembly;
                string[] parentTypePath = originType.FullName.Split('+');
                string typeName = string.Empty;
                string assemblyName = ", " + assembly.FullName;
                for (int i = 0; i < parentTypePath.Length - 1; i++) {
                    typeName += parentTypePath[i] + "+";
                    string fullTypeName = typeName + name + assemblyName;
                    retn = Type.GetType(fullTypeName);
                    if (retn != null) {
                        return retn;
                    }
                }
            }

            return ResolveType(name, namespaces);
        }

        // todo -- handle generics too 
        // todo -- handle nested types
        public static Type ResolveType(string name, IReadOnlyList<string> namespaces) {
            FilterAssemblies();

            for (int i = 0; i < namespaces.Count; i++) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                if (assemblies == null) {
                    continue;
                }

                string typeName = namespaces[i] + "." + name + ", ";
                foreach (Assembly assembly in assemblies) {
                    string fullTypeName = typeName + assembly.FullName;

                    Type retn = Type.GetType(fullTypeName);

                    if (retn != null) {
                        return retn;
                    }
                }
            }

            return null;
        }

//        public static ProcessedType GetProcessedType(string typeName, List<ImportDeclaration> importPaths = null) {
//            FilterAssemblies();
//            
//            if (typeMap.TryGetValue(typeName, out ProcessedType processedType)) {
//                return processedType;
//            }
//
////            for (int i = 0; i < TemplateParser.IntrinsicElementTypes.Length; i++) {
////                if (typeName == TemplateParser.IntrinsicElementTypes[i].name) {
////                    return new ProcessedType(TemplateParser.IntrinsicElementTypes[i].type);
////                }
////            }
//
//            Type type = Type.GetType(typeName);
//
////            if (type == null) {
//////                for (int i = 0; i < loadedTypes.Count; i++) {
//////                    if (loadedTypes[i].Name == typeName) {
//////                        type = loadedTypes[i];
//////                        break;
//////                    }
//////                }
////            }
//
//            Assert.IsNotNull(type, $"type != null, unable to find type {typeName}");
//            typeMap.TryGetValue(type.Name, out ProcessedType retn);
//            return retn;
//        }

//        // todo -- handle imports
//        public static ProcessedType GetTemplateType(string tagName) {
//
//            ProcessedType processedType;
//            if (templateTypeMap.TryGetValue(tagName, out processedType)) {
//                return processedType;
//            }
//
//            throw new ParseException("Unable to find type for tag name: " + tagName);
//        }

        public static ProcessedType GetProcessedType(Type type) {
            FilterAssemblies();
            if (typeMap.TryGetValue(type, out ProcessedType retn)) {
                return retn;
            }

            if (!(typeof(UIElement).IsAssignableFrom(type))) {
                throw new Exception($"Cannot get a ProcessedType from a non {nameof(UIElement)} type. Tried to call with {type}");
            }

            Debug.Assert(type.IsGenericType);

            Type openType = type.GetGenericTypeDefinition();
            ProcessedType baseProcessedType = typeMap[openType];

            retn = new ProcessedType(type, baseProcessedType.templateAttr);
            return retn;
        }

        private static bool ShouldProcessTypes(Assembly assembly, bool wasFilteredOut) {
            string name = assembly.FullName;
            return !wasFilteredOut || (name.StartsWith("System") || name.StartsWith("Unity") || name.Contains("mscorlib"));
        }

        private static bool FilterAssembly(Assembly assembly) {
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

        public static StructList<ProcessedType> GetTemplateTypes() {
            FilterAssemblies();
            return templateTypes;
        }

        private static readonly char[] s_GenericSplitter = {'-', '-'};

        public static ProcessedType ResolveTagName(string tagName, IReadOnlyList<string> namespaces) {
            if (templateTypeMap.TryGetValue(tagName, out ProcessedType retnType)) {
                return retnType;
            }

            if (tagName.Contains("--")) {
                string[] nameParts = tagName.Split(s_GenericSplitter, StringSplitOptions.RemoveEmptyEntries);
                string genericTagName = nameParts[0] + "`1"; // todo support more than 1 level generics
                ProcessedType genericProcessedType = templateTypeMap.GetOrDefault(genericTagName);

                if (!genericProcessedType.rawType.IsGenericType) {
                    throw new Exception($"Tried to make an element with tag {tagName} but {nameParts[0]} is not a generic element type");
                }

                Type genericType = ResolveType(new TypeLookup(nameParts[1]), namespaces);

                if (genericType == null) {
                    throw new Exception($"Tried to make an element with tag {tagName} but {nameParts[1]} could not be resolved");
                }

                Type elementType = ReflectionUtil.CreateGenericType(genericProcessedType.rawType, genericType);
                ProcessedType retn = new ProcessedType(elementType, genericProcessedType.templateAttr);
                templateTypeMap[tagName] = retn;
                typeMap[retn.rawType] = retn;
                return retn;
            }

            throw new Exception("Unable to resolve tag name: " + tagName);
        }

        public static bool IsNamespace(string toCheck) {
            FilterAssemblies();
            return s_NamespaceMap.ContainsKey(toCheck);
        }

    }

    public class TypeResolutionException : Exception {

        public TypeResolutionException(string message) : base(message) { }

    }

}