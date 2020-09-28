using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Rendering;

namespace UIForia.Compilers {

    public static class TypeScanner {

        internal static readonly Dictionary<Type, ProcessedType> genericTypeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<string, Type> renderBoxTypeMap = new Dictionary<string, Type>();
        internal static readonly Dictionary<string, Type> layoutBoxTypeMap = new Dictionary<string, Type>();

        internal static IList<Type> moduleTypes;
        internal static IList<Type> elementTypes;
        internal static IList<Type> renderTypes;
        internal static IList<MethodInfo> changeHandlers;

        internal static readonly List<ProcessedType> dynamicTypes = new List<ProcessedType>();
        // internal static IList<TemplateParserDefinition> templateParserDefinitions;

        private static Stats stats;
        
        public static Stats Scan() {

#if UNITY_EDITOR
            ScanFast();
#else
            throw new NotImplementedException("Cannot Scan for types outside of the editor");
#endif

            return stats;
        }

        private static void ScanFast() {
#if UNITY_EDITOR
            Stopwatch stopwatch = Stopwatch.StartNew();
            moduleTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<UIModule>();
            elementTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<UIElement>();
            renderTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<RenderBox>();
            changeHandlers = UnityEditor.TypeCache.GetMethodsWithAttribute<OnPropertyChanged>();
            stats.totalScanTime = stopwatch.Elapsed.TotalMilliseconds;
            stats.moduleCount = moduleTypes.Count;
            stats.elementTypes = elementTypes.Count;
            stats.renderTypes = renderTypes.Count;
            stats.changeHandlerCount = changeHandlers.Count;

#endif
            //  TypeCache.TypeCollection templateParsers = TypeCache.GetTypesWithAttribute(typeof(TemplateParserAttribute));
            // templateParserDefinitions = new List<TemplateParserDefinition>(templateParsers.Count);
            //
            // for (int i = 0; i < templateParsers.Count; i++) {
            //
            //     Type type = templateParsers[i];
            //
            //     if (!type.IsSubclassOf(typeof(TemplateParser))) {
            //         Debug.LogError($"{type.GetTypeName()} is marked with [{nameof(TemplateParserAttribute)}] but does not extend {TypeNameGenerator.GetTypeName(typeof(TemplateParser))}");
            //         continue;
            //     }
            //
            //     TemplateParserAttribute attr = type.GetCustomAttribute<TemplateParserAttribute>();
            //
            //     for (int j = 0; j < templateParserDefinitions.Count; j++) {
            //         if (templateParserDefinitions[j].extension == attr.extension) {
            //             Debug.LogError($"Duplicate parser for template extension {attr.extension}. Both {type.GetTypeName()} and {templateParserDefinitions[j].type.GetTypeName()} handle .{attr.extension} files");
            //         }
            //     }
            //
            //     templateParserDefinitions.Add(new TemplateParserDefinition() {
            //         extension = attr.extension,
            //         type = type
            //     });
            //
            // }

        }

        public struct Stats {

            public double totalScanTime;
            public int moduleCount;
            public int elementTypes;
            public int renderTypes;
            public int changeHandlerCount;

        }

    }

}