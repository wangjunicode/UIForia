using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UIForia.Parsing;
using UIForia.Util;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace UIForia.Editor {
    public static class TypeProcessorEditorTest {
        [MenuItem("UIForia Dev/Test Assembly Load Time")]
        public static void TestAssemblyLoadTime() {
            Dictionary<string, ProcessedType> outGenericMap = new Dictionary<string, ProcessedType>();
            Dictionary<Type,ProcessedType> outTypeMap = new Dictionary<Type, ProcessedType>();
            Dictionary<string,TypeProcessor.TypeList> outTemplateTypeMap = new Dictionary<string, TypeProcessor.TypeList>();
            LightList<ProcessedType> outTemplateTypes = new LightList<ProcessedType>();
            Dictionary<string,LightList<Assembly>> outNamespaceMap = new Dictionary<string, LightList<Assembly>>();

            // patch custom painters for test.
            Dictionary<string, Type> oldPainters = Application.s_CustomPainters;
            Application.s_CustomPainters = new Dictionary<string, Type>();

            try {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int count = TypeProcessor.FilterAssembliesInternal(outTemplateTypes, outTypeMap, outTemplateTypeMap, outGenericMap, outNamespaceMap);
                stopwatch.Stop();
                Debug.Log($"TEST: Loaded types in {stopwatch.ElapsedMilliseconds} ms from {count} assemblies");
            } finally {
                Application.s_CustomPainters = oldPainters;
            }
        }
    }
}