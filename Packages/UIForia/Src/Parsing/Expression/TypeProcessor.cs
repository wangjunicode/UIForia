using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class GenericElementTypeResolvedByAttribute : Attribute {

        public readonly string propertyName;

        public GenericElementTypeResolvedByAttribute(string propertyName) {
            this.propertyName = propertyName;
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ResolveGenericTemplateArguments : Attribute { }

    public static class TypeProcessor {

        internal struct TypeList {

            public ProcessedType mainType;
            public ProcessedType[] types;

        }

        public static bool processedTypes;
        private static readonly LightList<ProcessedType> templateTypes = new LightList<ProcessedType>(64);
        internal static readonly Dictionary<string, TypeList> templateTypeMap = new Dictionary<string, TypeList>();
        public static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        public static readonly Dictionary<string, ProcessedType> s_GenericMap = new Dictionary<string, ProcessedType>();
        private static readonly List<ProcessedType> dynamicTypes = new List<ProcessedType>();

        private static int currentTypeId;
        private static int NextTypeId => currentTypeId++;
        
        // todo! I totally broke this, need to circle back and fix it later
        private static void FilterAssemblies() {
            // if (processedTypes) return;
            // processedTypes = true;
            //
            // Stopwatch watch = new Stopwatch();
            // watch.Start();
            //
            // Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // int count = 0;
            // for (int i = 0; i < assemblies.Length; i++) {
            //     Assembly assembly = assemblies[i];
            //
            //     if (assembly == null || assembly.IsDynamic) {
            //         continue;
            //     }
            //
            //     bool filteredOut = !FilterAssembly(assembly);
            //     bool shouldProcessTypes = ShouldProcessTypes(assembly, filteredOut);
            //
            //     if (!shouldProcessTypes) {
            //         continue;
            //     }
            //
            //     count++;
            //
            //     try {
            //         Type[] types = assembly.GetTypes();
            //
            //         for (int j = 0; j < types.Length; j++) {
            //             Type currentType = types[j];
            //             // can be null if assembly referenced is unavailable
            //             // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            //             if (currentType == null) {
            //                 continue;
            //             }
            //
            //             if (!filteredOut && currentType.IsClass && currentType.Name[0] != '<' && currentType.IsGenericTypeDefinition) {
            //                 if (currentType.IsSubclassOf(typeof(UIElement))) {
            //                     Attribute[] attrs = Attribute.GetCustomAttributes(currentType, false);
            //                     string tagName = GetTemplateAttribute(currentType, attrs, out TemplateAttribute templateAttr);
            //
            //                     // todo -- support namespaces in the look up map
            //                     tagName = tagName.Split('`')[0];
            //
            //                     ProcessedType processedType = new ProcessedType(currentType, templateAttr, tagName);
            //                     processedType.IsUnresolvedGeneric = true;
            //                     try {
            //                         s_GenericMap.Add(tagName, processedType);
            //                     }
            //                     catch (Exception) {
            //                         Debug.LogError($"UIForia does not support multiple elements with the same tag name. Tried to register type {processedType.rawType} for `{tagName}` " +
            //                                        $"but this tag name was already taken by type {s_GenericMap[tagName].rawType}. For generic overload types with multiple arguments you need to supply a unique [TagName] attribute");
            //                         continue;
            //                     }
            //
            //                     typeMap[currentType] = processedType;
            //
            //                     if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> namespaceList)) {
            //                         namespaceList = new LightList<Assembly>(2);
            //                         s_NamespaceMap.Add(currentType.Namespace ?? "null", namespaceList);
            //                     }
            //
            //                     if (!namespaceList.Contains(assembly)) {
            //                         namespaceList.Add(assembly);
            //                     }
            //
            //                     continue;
            //                 }
            //             }
            //
            //             if (!filteredOut && currentType.IsClass && !currentType.IsGenericTypeDefinition) {
            //                 Attribute[] attrs = Attribute.GetCustomAttributes(currentType, false);
            //                 Application.ProcessClassAttributes(currentType, attrs);
            //
            //                 if (typeof(UIElement).IsAssignableFrom(currentType)) {
            //                     string tagName = GetTemplateAttribute(currentType, attrs, out TemplateAttribute templateAttr);
            //
            //                     ProcessedType processedType = new ProcessedType(currentType, templateAttr, tagName);
            //
            //                     if (templateAttr != null) {
            //                         templateTypes.Add(processedType);
            //                     }
            //
            //                     // if (templateTypeMap.ContainsKey(tagName)) {
            //                     //     Debug.Log($"Tried to add template key `{tagName}` from type {currentType} but it was already defined by {templateTypeMap.GetOrDefault(tagName).rawType}");
            //                     // }
            //
            //                     if (templateTypeMap.TryGetValue(tagName, out TypeList typeList)) {
            //                         if (typeList.types != null) {
            //                             Array.Resize(ref typeList.types, typeList.types.Length + 1);
            //                             typeList.types[typeList.types.Length - 1] = processedType;
            //                         }
            //                         else {
            //                             typeList.types = new ProcessedType[2];
            //                             typeList.types[0] = typeList.mainType;
            //                             typeList.types[1] = processedType;
            //                         }
            //                     }
            //                     else {
            //                         typeList.mainType = processedType;
            //                         templateTypeMap[tagName] = typeList;
            //                     }
            //
            //                     // templateTypeMap.Add(tagName, processedType);
            //                     processedType.id = NextTypeId;
            //                     typeMap[currentType] = processedType;
            //                 }
            //             }
            //
            //             if (filteredOut && !currentType.IsPublic) {
            //                 continue;
            //             }
            //
            //             if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> list)) {
            //                 list = new LightList<Assembly>(2);
            //                 s_NamespaceMap.Add(currentType.Namespace ?? "null", list);
            //             }
            //
            //             if (!list.Contains(assembly)) {
            //                 list.Add(assembly);
            //             }
            //         }
            //     }
            //     catch (ReflectionTypeLoadException) {
            //         Debug.Log($"{assembly.FullName}");
            //         throw;
            //     }
            // }
            //
            // watch.Stop();
            // Debug.Log($"Loaded types in: {watch.ElapsedMilliseconds} ms from {count} assemblies");
        }

        private static string GetTemplateAttribute(Type currentType, Attribute[] attrs, out TemplateAttribute templateAttr) {
            string tagName = currentType.Name;
            templateAttr = null;

            for (int index = 0; index < attrs.Length; index++) {
                Attribute attr = attrs[index];

                if (attr is TemplateTagNameAttribute templateTagNameAttr) {
                    tagName = templateTagNameAttr.tagName;
                }

                if (attr is TemplateAttribute templateAttribute) {
                    templateAttr = templateAttribute;
                }
            }

            // if no template attribute is defined, assume the default scheme
            if (templateAttr == null) {
                templateAttr = new TemplateAttribute(TemplateType.DefaultFile, null);
            }

            return tagName;
        }

      

        internal static ProcessedType GetProcessedType(Type type) {
            FilterAssemblies();
            typeMap.TryGetValue(type, out ProcessedType retn);
            if (retn != null) {
                retn.references++;
            }

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

        public static LightList<ProcessedType> GetTemplateTypes() {
            FilterAssemblies();
            return templateTypes;
        }

        private static readonly List<string> EmptyNamespaceList = new List<string>();


        // Namespace resolution
        //    if there is only one element with a name then no namespace is needed
        //    if there are multiple elements with a name
        //        namespace is required in order to match the correct one
        //    using declarations can provide implicit namespaces
        public static ProcessedType ResolveTagName(string tagName, string namespacePrefix, IReadOnlyList<string> namespaces) {
            FilterAssemblies();

            namespaces = namespaces ?? EmptyNamespaceList;

            if (string.IsNullOrEmpty(namespacePrefix)) namespacePrefix = null;
            if (string.IsNullOrWhiteSpace(namespacePrefix)) namespacePrefix = null;

            if (templateTypeMap.TryGetValue(tagName, out TypeList typeList)) {
                // if this is null we resolve using just the tag name
                if (namespacePrefix == null) {
                    // if only one type has this tag name we can safely return it
                    if (typeList.types == null) {
                        return typeList.mainType.Reference();
                    }

                    // if there are multiple tags with this name, we need to search our namespaces 
                    // if only one match is found, we can return it. If multiple are found, throw
                    // and ambiguous reference exception
                    LightList<ProcessedType> resultList = LightList<ProcessedType>.Get();
                    for (int i = 0; i < namespaces.Count; i++) {
                        for (int j = 0; j < typeList.types.Length; j++) {
                            string namespaceName = namespaces[i];
                            ProcessedType testType = typeList.types[j];
                            if (namespaceName == testType.namespaceName) {
                                resultList.Add(testType);
                            }
                        }
                    }

                    if (resultList.size == 1) {
                        ProcessedType retn = resultList[0];
                        resultList.Release();
                        return retn.Reference();
                    }

                    List<string> list = resultList.Select((s) => s.namespaceName).ToList();
                    throw new ParseException("Ambiguous TagName reference: " + tagName + ". References found in namespaces " + StringUtil.ListToString(list, ", "));
                }

                if (typeList.types == null) {
                    if (namespacePrefix == typeList.mainType.namespaceName) {
                        return typeList.mainType.Reference();
                    }
                }
                else {
                    // if prefix is not null we can only return a match for that namespace
                    for (int j = 0; j < typeList.types.Length; j++) {
                        ProcessedType testType = typeList.types[j];
                        if (namespacePrefix == testType.namespaceName) {
                            return testType.Reference();
                        }
                    }
                }

                return null;
            }

            if (s_GenericMap.TryGetValue(tagName, out ProcessedType processedType)) {
                return processedType;
            }

            return null;
        }
        

        public static ProcessedType AddResolvedGenericElementType(Type newType, TemplateAttribute templateAttr, string tagName) {
            ProcessedType retn = null;
            if (!typeMap.TryGetValue(newType, out retn)) {
                retn = new ProcessedType(newType, templateAttr, tagName);
                retn.id = NextTypeId;
                typeMap.Add(retn.rawType, retn);
            }

            if (retn != null) {
                retn.references++;
            }

            return retn;
        }

        // todo -- would be good to have this be an instance property because we need to clear dynamics every time we compile
        public static void AddDynamicElementType(ProcessedType processedType) {
            processedType.id = NextTypeId;
            typeMap[processedType.rawType] = processedType;
            dynamicTypes.Add(processedType);
            // templateTypes.Add(processedType);
            // todo -- maybe add to namespace map?
        }

        public static void ClearDynamics() {
            for (int i = 0; i < dynamicTypes.Count; i++) {
                typeMap.Remove(dynamicTypes[i].rawType);
            }
        }
        
    }

}