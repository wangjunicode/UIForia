using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Exceptions;
using UIForia.Util;

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
        public static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<string, TypeList> templateTypeMap = new Dictionary<string, TypeList>();
        public static readonly Dictionary<string, ProcessedType> s_GenericMap = new Dictionary<string, ProcessedType>();
        private static readonly List<ProcessedType> dynamicTypes = new List<ProcessedType>();

        private static int currentTypeId;
        private static int NextTypeId => currentTypeId++;

        // private static string GetTemplateAttribute(Type currentType, Attribute[] attrs, out TemplateAttribute templateAttr) {
        //     string tagName = currentType.Name;
        //     templateAttr = null;
        //
        //     for (int index = 0; index < attrs.Length; index++) {
        //         Attribute attr = attrs[index];
        //
        //         if (attr is TemplateTagNameAttribute templateTagNameAttr) {
        //             tagName = templateTagNameAttr.tagName;
        //         }
        //
        //         if (attr is TemplateAttribute templateAttribute) {
        //             templateAttr = templateAttribute;
        //         }
        //     }
        //
        //     // if no template attribute is defined, assume the default scheme
        //     if (templateAttr == null) {
        //         templateAttr = new TemplateAttribute(TemplateType.DefaultFile, null);
        //     }
        //
        //     return tagName;
        // }

        internal static ProcessedType GetProcessedType(Type type) {
            typeMap.TryGetValue(type, out ProcessedType retn);
            if (retn != null) {
                retn.references++;
            }

            return retn;
        }

        public static LightList<ProcessedType> GetTemplateTypes() {
            return templateTypes;
        }

        private static readonly List<string> EmptyNamespaceList = new List<string>();

        // Namespace resolution
        //    if there is only one element with a name then no namespace is needed
        //    if there are multiple elements with a name
        //        namespace is required in order to match the correct one
        //    using declarations can provide implicit namespaces
        public static ProcessedType ResolveTagName(string tagName, string namespacePrefix, IReadOnlyList<string> namespaces) {

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

    public class TypeResolutionException : Exception {

        public TypeResolutionException(string message) : base(message) { }

    }

}