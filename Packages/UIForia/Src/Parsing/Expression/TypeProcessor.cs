using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace UIForia.Parsing {

    internal struct TemplateParserDefinition {

        public Type type;
        public string extension;

    }

    public static class TypeProcessor {

        internal static readonly Dictionary<Type, ProcessedType> genericTypeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<string, Type> renderBoxTypeMap = new Dictionary<string, Type>();
        internal static readonly Dictionary<string, Type> layoutBoxTypeMap = new Dictionary<string, Type>();
        internal static readonly List<ProcessedType> dynamicTypes = new List<ProcessedType>();
        internal static IList<TemplateParserDefinition> templateParserDefinitions;

        private static readonly object genericLock = new object();

        [ThreadStatic] private static List<CharSpan> strings;
        
        internal static IList<Type> moduleTypes;
        internal static IList<Type> elements;
        internal static IList<Type> painters;
        internal static IList<Type> layouts;
        private static Module[] modules;
        private static TemplateCache s_TemplateCache;
        
        static TypeProcessor() {

            TypeResolver.Initialize();
#if UNITY_EDITOR
            ScanFast(out moduleTypes, out elements, out layouts, out painters);
#else
            ScanSlow(out moduleTypes, out elements, out layouts, out painters);
#endif

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

        internal static IList<Type> GetModuleTypes() {
            return moduleTypes;
        }

        internal static IList<Type> GetElementTypes() {
            return elements;
        }

        public static IList<ProcessedType> GetTemplateElements() {
            return typeMap.Values.ToList();
        }

        public static ProcessedType AddResolvedGenericElementType(Type newType, ProcessedType generic) {
            ProcessedType retn;
            lock (genericLock) {
                if (!genericTypeMap.TryGetValue(newType, out retn)) {
                    retn = ProcessedType.ResolveGeneric(newType, generic);
                    genericTypeMap.Add(retn.rawType, retn);
                }
            }

            if (retn != null) {
                retn.references++;
                retn.templateRootNode = generic.templateRootNode;
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
            TypeCache.TypeCollection templateParsers = TypeCache.GetTypesWithAttribute(typeof(TemplateParserAttribute));

            templateParserDefinitions = new List<TemplateParserDefinition>(templateParsers.Count);

            for (int i = 0; i < templateParsers.Count; i++) {

                Type type = templateParsers[i];

                if (!type.IsSubclassOf(typeof(TemplateParser))) {
                    Debug.LogError($"{type.GetTypeName()} is marked with [{nameof(TemplateParserAttribute)}] but does not extend {TypeNameGenerator.GetTypeName(typeof(TemplateParser))}");
                    continue;
                }

                TemplateParserAttribute attr = type.GetCustomAttribute<TemplateParserAttribute>();

                for (int j = 0; j < templateParserDefinitions.Count; j++) {
                    if (templateParserDefinitions[j].extension == attr.extension) {
                        Debug.LogError($"Duplicate parser for template extension {attr.extension}. Both {type.GetTypeName()} and {templateParserDefinitions[j].type.GetTypeName()} handle .{attr.extension} files");
                    }
                }

                templateParserDefinitions.Add(new TemplateParserDefinition() {
                    extension = attr.extension,
                    type = type
                });

            }

        }

        private static void ScanSlow(out IList<Type> moduleTypes, out IList<Type> elementTypes, out IList<Type> layoutTypes, out IList<Type> renderTypes) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            moduleTypes = new List<Type>(16);
            elementTypes = new List<Type>(128);
            layoutTypes = new List<Type>(8);
            renderTypes = new List<Type>(8);
            templateParserDefinitions = new List<TemplateParserDefinition>(4);

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

                    if (currentType.IsSubclassOf(typeof(UIElement))) {
                        elementTypes.Add(currentType);
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(Module))) {
                        moduleTypes.Add(currentType);
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(RenderBox))) {
                        renderTypes.Add(currentType);
                        continue;
                    }

                    if (currentType.IsSubclassOf(typeof(AwesomeLayoutBox))) {
                        layoutTypes.Add(currentType);
                    }

                    if (currentType.IsSubclassOf(typeof(TemplateParser))) {

                        TemplateParserAttribute attr = currentType.GetCustomAttribute<TemplateParserAttribute>();

                        if (attr == null) {
                            continue;
                        }

                        for (int k = 0; k < templateParserDefinitions.Count; k++) {
                            if (templateParserDefinitions[k].extension == attr.extension) {
                                Debug.LogError($"Duplicate parser for template extension {attr.extension}. Both {currentType.GetTypeName()} and {templateParserDefinitions[k].type.GetTypeName()} handle .{attr.extension} files");
                            }
                        }

                        templateParserDefinitions.Add(new TemplateParserDefinition() {
                            extension = attr.extension,
                            type = currentType
                        });

                    }

                }
            }
        }

        internal static TemplateParser[] CreateTemplateParsers() {

            TemplateParser[] retn = new TemplateParser[templateParserDefinitions.Count];

            // some sort of weird recursive type definition detection 
            // seems to be happening inside a managed job. lock to be sure we aren't racing.
            lock (templateParserDefinitions) {
                for (int i = 0; i < retn.Length; i++) {
                    retn[i] = (TemplateParser) Activator.CreateInstance(templateParserDefinitions[i].type);
                    retn[i].extension = templateParserDefinitions[i].extension;
                }
            }

            return retn;

        }

        public interface IDiagnosticProvider {

            void AddDiagnostic(string message);

        }

        public struct DiagnosticWrapper {

            private IDiagnosticProvider provider;

            public DiagnosticWrapper(TemplateShell templateShell) {
                this.provider = null; // todo !
            }

            public void AddDiagnostic(string message, int lineNumber = -1, int column = -1) {
                if (provider == null) return;
                
            }

        }

        internal static ProcessedType ResolveGenericElementType(ProcessedType generic, string genericTypeResolver, IReadOnlyList<string> referencedNamespaces, DiagnosticWrapper diagnostics) {

            Type[] arguments = generic.rawType.GetGenericArguments();
            Type[] resolvedTypes = new Type[arguments.Length];

            int ptr = 0;
            int rangeStart = 0;
            int depth = 0;

            strings = strings ?? new List<CharSpan>(8);
            strings.Clear();

            // todo -- is it better to just generate the Element< > wrapper and use TypeResolver.TryParseTypeName directly?

            while (ptr != genericTypeResolver.Length) {
                char c = genericTypeResolver[ptr];
                switch (c) {
                    case '<':
                        depth++;
                        break;

                    case '>':
                        depth--;
                        break;

                    case ',': {
                        if (depth == 0) {
                            strings.Add(new CharSpan(genericTypeResolver, rangeStart, ptr));
                            rangeStart = ptr;
                        }

                        break;
                    }
                }

                ptr++;
            }

            if (rangeStart != ptr) {
                strings.Add(new CharSpan(genericTypeResolver, rangeStart, ptr));
            }

            if (arguments.Length != strings.Count) {
                diagnostics.AddDiagnostic($"Expected {arguments.Length} arguments but was only provided {strings.Count} {genericTypeResolver}");
                return null;
            }

            for (int i = 0; i < strings.Count; i++) {
                if (TypeResolver.TryParseTypeName(strings[i], out TypeLookup typeLookup)) {
                    Type type = TypeResolver.Default.ResolveType(typeLookup, referencedNamespaces);

                    if (type == null) {
                        diagnostics.AddDiagnostic(TemplateCompileException.UnresolvedType(typeLookup, referencedNamespaces).Message);
                        return null;
                    }

                    resolvedTypes[i] = type;
                }
                else {
                    diagnostics.AddDiagnostic($"Failed to parse generic specifier {strings[i]}. Original expression = {genericTypeResolver}");
                    return null;
                }
            }
            
            return AddResolvedGenericElementType(generic, resolvedTypes);
        }

        public static ProcessedType AddResolvedGenericElementType(ProcessedType openType, Type[] resolvedTypes) {

            lock (genericLock) {

                Type createdType = openType.rawType.MakeGenericType(resolvedTypes);

                if (!genericTypeMap.TryGetValue(createdType, out ProcessedType retn)) {
                    retn = ProcessedType.ResolveGeneric(createdType, openType);
                    genericTypeMap.Add(retn.rawType, retn);
                }

                if (retn != null) {
                    retn.references++;
                }

                return retn;
            }
        }

    }

}