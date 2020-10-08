using System;
using System.Collections.Generic;
using UIForia.Elements;
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

        private static readonly LightList<ProcessedType> templateTypes = new LightList<ProcessedType>(64);
        public static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();

        private static int currentTypeId;

        internal static ProcessedType GetProcessedType(Type type) {
            typeMap.TryGetValue(type, out ProcessedType retn);
            return retn;
        }

        public static LightList<ProcessedType> GetTemplateTypes() {
            return templateTypes;
        }

        // Namespace resolution
        //    if there is only one element with a name then no namespace is needed
        //    if there are multiple elements with a name
        //        namespace is required in order to match the correct one
        //    using declarations can provide implicit namespaces
        public static ProcessedType ResolveTagName(string tagName, string namespacePrefix, IReadOnlyList<string> namespaces) {
            // namespaces = namespaces ?? EmptyNamespaceList;
            //
            // if (string.IsNullOrEmpty(namespacePrefix)) namespacePrefix = null;
            // if (string.IsNullOrWhiteSpace(namespacePrefix)) namespacePrefix = null;
            //
            // if (templateTypeMap.TryGetValue(tagName, out TypeList typeList)) {
            //     // if this is null we resolve using just the tag name
            //     if (namespacePrefix == null) {
            //         // if only one type has this tag name we can safely return it
            //         if (typeList.types == null) {
            //             return typeList.mainType.Reference();
            //         }
            //
            //         // if there are multiple tags with this name, we need to search our namespaces 
            //         // if only one match is found, we can return it. If multiple are found, throw
            //         // and ambiguous reference exception
            //         LightList<ProcessedType> resultList = LightList<ProcessedType>.Get();
            //         for (int i = 0; i < namespaces.Count; i++) {
            //             for (int j = 0; j < typeList.types.Length; j++) {
            //                 string namespaceName = namespaces[i];
            //                 ProcessedType testType = typeList.types[j];
            //                 // if (namespaceName == testType.namespaceName) {
            //                 //     resultList.Add(testType);
            //                 // }
            //             }
            //         }
            //
            //         if (resultList.size == 1) {
            //             ProcessedType retn = resultList[0];
            //             resultList.Release();
            //             return retn.Reference();
            //         }
            //
            //         // List<string> list = resultList.Select((s) => s.namespaceName).ToList();
            //         // throw new ParseException("Ambiguous TagName reference: " + tagName + ". References found in namespaces " + StringUtil.ListToString(list, ", "));
            //     }
            //
            //     if (typeList.types == null) {
            //         if (namespacePrefix == typeList.mainType.namespaceName) {
            //             return typeList.mainType.Reference();
            //         }
            //     }
            //     else {
            //         // if prefix is not null we can only return a match for that namespace
            //         for (int j = 0; j < typeList.types.Length; j++) {
            //             ProcessedType testType = typeList.types[j];
            //             if (namespacePrefix == testType.namespaceName) {
            //                 return testType.Reference();
            //             }
            //         }
            //     }
            //
            //     return null;
            // }
            //
            // if (s_GenericMap.TryGetValue(tagName, out ProcessedType processedType)) {
            //     return processedType;
            // }

            return null;
        }

        private static ProcessedType s_SlotDefine;
        private static ProcessedType s_SlotForward;
        private static ProcessedType s_SlotOverride;
        private static ProcessedType s_TextElement;
        private static ProcessedType s_MetaElement;

        public static ProcessedType GetSlotDefine() {
            if (s_SlotDefine != null) {
                return s_SlotDefine;
            }

            typeMap.TryGetValue(typeof(UISlotDefinition), out s_SlotDefine);

            return s_SlotDefine;
        }

        public static ProcessedType GetSlotOverride() {
            if (s_SlotOverride != null) {
                return s_SlotOverride;
            }

            typeMap.TryGetValue(typeof(UISlotOverride), out s_SlotOverride);

            return s_SlotOverride;
        }

        public static ProcessedType GetSlotForward() {
            if (s_SlotForward != null) {
                return s_SlotForward;
            }

            typeMap.TryGetValue(typeof(UISlotForward), out s_SlotForward);

            return s_SlotForward;
        }

        public static ProcessedType GetMetaElement() {
            if (s_MetaElement != null) {
                return s_MetaElement;
            }

            typeMap.TryGetValue(typeof(UISlotForward), out s_MetaElement);

            return s_MetaElement;
        }

        public static ProcessedType GetTextElement() {
            if (s_TextElement != null) {
                return s_TextElement;
            }

            typeMap.TryGetValue(typeof(UITextElement), out s_TextElement);

            return s_TextElement;
        }

        public static ProcessedType GetOrCreateGeneric(ProcessedType baseType, Type createdType) {

            if (typeMap.TryGetValue(createdType, out ProcessedType retn)) {
                return retn;
            }
            
            retn = ProcessedType.CreateGeneric(baseType, createdType);
            typeMap[createdType] = retn;
            return retn;
        }

    }

    public class TypeResolutionException : Exception {

        public TypeResolutionException(string message) : base(message) { }

    }

}