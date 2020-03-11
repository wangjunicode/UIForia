using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UIForia.Parsing {

    public static class TypeProcessor {

        internal static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<string, Type> renderBoxTypeMap = new Dictionary<string, Type>();
        internal static readonly Dictionary<string, Type> layoutBoxTypeMap = new Dictionary<string, Type>();
        internal static readonly List<ProcessedType> dynamicTypes = new List<ProcessedType>();

        private static bool initialized;

        public static void Initialize() {
            if (initialized) return;
            initialized = true;

            TypeResolver.Initialize();

            // todo -- provide a non editor implementation that scans assemblies
            IList<Type> moduleTypes;
            IList<Type> elements;
            IList<Type> painters;
            IList<Type> layouts;

        #if UNITY_EDITOR
            ScanFast(out moduleTypes, out elements, out layouts, out painters);
        #else
            ScanSlow(out moduleTypes, out elements, out layouts, out painters);
        #endif

            Module.InitializeModules(moduleTypes);

            for (int i = 0; i < elements.Count; i++) {
                Type currentType = elements[i];

                if (currentType.IsAbstract) continue;

                ProcessedType processedType = ProcessedType.CreateFromType(currentType);

                if (string.IsNullOrEmpty(processedType.elementPath)) {
                    if (processedType.rawType.Assembly != typeof(UIElement).Assembly) {
                        Debug.LogError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} requires a location providing attribute." +
                                       $" Please use [{nameof(RecordFilePathAttribute)}], [{nameof(TemplateAttribute)}], " +
                                       $"[{nameof(ImportStyleSheetAttribute)}], [{nameof(StyleAttribute)}]" +
                                       $" or [{nameof(TemplateTagNameAttribute)}] on the class. If you intend not to provide a template you can also use [{nameof(ContainerElementAttribute)}].");
                        continue;
                    }
                }

                if (!Module.TryGetModule(processedType, out Module module)) {
                    Debug.LogError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} at {processedType.elementPath} was not inside a module hierarchy.");
                    continue;
                }

                // todo -- need to check that a reserved tag name was not taken!
                
                try {
                    module.tagNameMap.Add(processedType.tagName, processedType);
                }
                catch (ArgumentException) {
                    Debug.LogError($"UIForia does not support multiple elements with the same tag name within the same module. Tried to register type {TypeNameGenerator.GetTypeName(processedType.rawType)} for `{processedType.tagName}` " +
                                   $" in module {TypeNameGenerator.GetTypeName(module.GetType())} at {module.location} " +
                                   $"but this tag name was already taken by type {TypeNameGenerator.GetTypeName(module.tagNameMap[processedType.tagName].rawType)}. " +
                                   "For generic overload types with multiple arguments you need to supply a unique [TagName] attribute");
                    continue;
                }

                typeMap[currentType] = processedType;
            }

            Module.CreateBuiltInTypeArray();

            for (int i = 0; i < painters.Count; i++) {
                Type type = painters[i];

                if (type == typeof(RenderBox) || type == typeof(StandardRenderBox) || type == typeof(ImageRenderBox) || type == typeof(TextRenderBox)) {
                    continue;
                }

                CustomPainterAttribute painter = type.GetCustomAttribute<CustomPainterAttribute>();

                if (painter == null) {
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) == null) {
                    Debug.LogError($"Classes marked with [{nameof(CustomPainterAttribute)}] must provide a parameterless constructor" +
                                   $" and the class must extend {nameof(RenderBox)}. Ensure that {type.FullName} conforms to these rules");
                    continue;
                }

                renderBoxTypeMap.Add(painter.name, type);
                Application.RegisterPainter(type, painter.name);
            }

            // todo -- implement this
            //for (int i = 0; i < layouts.Count; i++) {
            // Type type = layouts[i];
            // layoutBoxTypeMap.Add(type.FullName, type);                
            //}
        }

        internal static ProcessedType GetProcessedType(Type type) {
            typeMap.TryGetValue(type, out ProcessedType retn);
            // we dont really care about thread safety for this, just need it incremented, the value doesnt matter
            if (retn != null) {
                retn.references++;
            }

            return retn;
        }

        // Namespace resolution
        //    There can be only one tag name per module.
        //    Built in elements reserve their tag names and can bes resolved from anywhere.
        //    A template can 'static' include with a using statement to avoid module prefixing. If there is a name collision,
        //    the current module wins over dependencies. If the conflict is between dependencies that are both statically included,
        //    an error is thrown.
        public static ProcessedType ResolveTagName(string tagName, string namespacePrefix, IReadOnlyList<string> namespaces) {
            if (string.IsNullOrEmpty(namespacePrefix)) namespacePrefix = null;
            if (string.IsNullOrWhiteSpace(namespacePrefix)) namespacePrefix = null;

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
            //                 if (namespaceName == testType.namespaceName) {
            //                     resultList.Add(testType);
            //                 }
            //             }
            //         }
            //
            //         if (resultList.size == 1) {
            //             ProcessedType retn = resultList[0];
            //             resultList.Release();
            //             return retn.Reference();
            //         }
            //
            //         List<string> list = resultList.Select((s) => s.namespaceName).ToList();
            //         throw new ParseException("Ambiguous TagName reference: " + tagName + ". References found in namespaces " + StringUtil.ListToString(list, ", "));
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

        public static ProcessedType AddResolvedGenericElementType(Type newType, ProcessedType generic) {
            if (!typeMap.TryGetValue(newType, out ProcessedType retn)) {
                retn = ProcessedType.ResolveGeneric(newType, generic);
                typeMap.Add(retn.rawType, retn);
            }

            if (retn != null) {
                retn.references++;
            }

            return retn;
        }

        // todo -- would be good to have this be an instance property because we need to clear dynamics every time we compile
        public static void AddDynamicElementType(ProcessedType processedType) {
            typeMap[processedType.rawType] = processedType;
            dynamicTypes.Add(processedType);
        }

        public static void ClearDynamics() {
            for (int i = 0; i < dynamicTypes.Count; i++) {
                typeMap.Remove(dynamicTypes[i].rawType);
            }
        }

        private static void ScanFast(out IList<Type> moduleTypes, out IList<Type> elementTypes, out IList<Type> layoutTypes, out IList<Type> renderTypes) {
            moduleTypes = TypeCache.GetTypesDerivedFrom<Module>();
            elementTypes = TypeCache.GetTypesDerivedFrom<UIElement>();
            layoutTypes = TypeCache.GetTypesDerivedFrom<AwesomeLayoutBox>();
            renderTypes = TypeCache.GetTypesDerivedFrom<RenderBox>();
        }

        private static void ScanSlow(out IList<Type> moduleTypes, out IList<Type> elementTypes, out IList<Type> layoutTypes, out IList<Type> renderTypes) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            moduleTypes = new List<Type>(16);
            elementTypes = new List<Type>(128);
            layoutTypes = new List<Type>(8);
            renderTypes = new List<Type>(8);

            for (int i = 0; i < assemblies.Length; i++) {
                Assembly assembly = assemblies[i];

                if (assembly.IsDynamic || assembly.ReflectionOnly) {
                    continue;
                }

                Type[] types = assembly.GetExportedTypes();

                for (int j = 0; j < types.Length; j++) {
                    Type currentType = types[j];
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (currentType == null || !currentType.IsClass || currentType.IsAbstract) {
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(Module))) {
                        moduleTypes.Add(currentType);
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(UIElement))) {
                        elementTypes.Add(currentType);
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(RenderBox))) {
                        renderTypes.Add(currentType);
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(AwesomeLayoutBox))) {
                        layoutTypes.Add(currentType);
                    }
                }
            }
        }

    }

}