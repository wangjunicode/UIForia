using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
#endif

namespace UIForia {

    public enum CompilationType {

        Dynamic,
        Precompiled

    }

    public class PrecompileSettings {

        public string assemblyName;
        public string codeOutputPath;
        public string styleOutputPath;
        public string rootTypeName;
        public string codeFileExtension;

        public PrecompileSettings() {
            this.assemblyName = "UIForia.Application";
            this.styleOutputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.codeOutputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.codeFileExtension = "generated.cs";
            this.rootTypeName = "UIForiaGeneratedApplication";
        }

    }

    public class CompilationResult { }


    public static class UIForiaRuntime {

        private static LightList<Application> s_Applications;
        private static TemplateParseCache s_ParseCache;

        public static void Initialize() {
#if UNITY_EDITOR
            if (SessionState.GetInt("UIForiaRuntime::DidInitialStyleParse", 0) == 0) {
                SessionState.SetInt("UIForiaRuntime::DidInitialStyleParse", 1);
                // IEnumerable<string> z = Directory.EnumerateFiles(Path.Combine(UnityEngine.Application.dataPath, ".."), "*.style", SearchOption.AllDirectories);
            }
#endif
        }

        internal static void Reset() {
            // kill all apps, kill all caches
        }

        private static string GetUniqueApplicationName(string applicationId) {
            // todo -- figure this out
            return applicationId;
        }

        private static Dictionary<ProcessedType, UIModule> BuildModuleMap(List<UIModule> moduleDefinitions) {
            Stopwatch sw = Stopwatch.StartNew();

            Dictionary<ProcessedType, UIModule> moduleMap = new Dictionary<ProcessedType, UIModule>();

            // probably need to convert type map to array for thread safety
            // makes it easier to thread this too if needed

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {
                string elementPath = kvp.Value.elementPath;

                if (string.IsNullOrEmpty(elementPath)) {
                    continue;
                }

                bool found = false;
                for (int i = 0; i < moduleDefinitions.Count; i++) {
                    if (elementPath.StartsWith(moduleDefinitions[i].location, StringComparison.Ordinal)) {
                        moduleMap[kvp.Value] = moduleDefinitions[i];

                        string tagName = kvp.Value.tagName;

                        if (moduleDefinitions[i].tagNameMap.TryGetValue(tagName, out ProcessedType type)) {
                            // has critical error = true
                            throw new Exception($"Duplicate tag name `{tagName}` in module at {moduleDefinitions[i].path}");
                        }

                        moduleDefinitions[i].tagNameMap.Add(tagName, kvp.Value);
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    Debug.Log("Unable to associate element " + kvp.Value.rawType.GetTypeName() + " with a module");
                }
            }

            Debug.Log("map modules: " + sw.Elapsed.TotalMilliseconds.ToString("F3"));
            return moduleMap;
        }

        public static GameApplication2 CreateGameApplication(string applicationId, Type entryType) {
            Initialize();

            GameApplication2 application = new GameApplication2(GetUniqueApplicationName(applicationId));
            if (entryType == null || entryType.IsGenericType || entryType.IsAbstract || !(typeof(UIElement).IsAssignableFrom(entryType))) {
                application.IsValid = false;
                throw new Exception("invalid type");
            }
            // else if (entryType.GetCustomAttribute<EntryPointAttribute>() == null) {
            //     application.IsValid = false;
            //     throw new Exception("Missing [EntryPoint] attribute");
            // }
            else {
                TypeScanner.Scan();
                TypeResolver.Initialize();
                CreateProcessedElementTypes();

                Compilation compilation = new Compilation();
                compilation.entryType = TypeProcessor.GetProcessedType(entryType);
                compilation.parseCache = new TemplateParseCache();
                compilation.compilationType = CompilationType.Dynamic;
                compilation.application = application;
                compilation.parseCache.Initialize();

                // this should be done PER application so that if one app is running and we refresh another,
                // the running one won't need to adjust itself until it gets refreshed
                List<UIModule> moduleList = FindModules();
                compilation.moduleMap = BuildModuleMap(moduleList);
                compilation.ResolveTagNames();
                // ParseStyles()

                compilation.Compile();
                
            }

            return application;
        }

        private static void CreateProcessedElementTypes() {

            // this should only run once on startup, the type map might get appended to during compilation but the types will never change
            // while an application is running

            if (TypeProcessor.typeMap.Count > 0) {
                return;
            }
            Stopwatch sw = Stopwatch.StartNew();

            // todo -- this is totally threadsafe as long as I give each thread its own diagnostics and change the TypeProcessor.typeMap to be a concurrent dictionary
            Diagnostics diagnostics = new Diagnostics();
            IList<Type> elements = TypeScanner.elementTypes;

            for (int i = 0; i < elements.Count; i++) {
                Type currentType = elements[i];

                if (currentType.IsAbstract) continue;

                ProcessedType processedType = ProcessedType.CreateFromType(currentType, diagnostics);

                // CreateFromType handles logging diagnostics, can just move on if we failed
                if (processedType == null) {
                    continue;
                }

                // I don't think we need an element location unless we need to resolve either a style or template based on that element
                // except that in order to resolve by tag name, we do need to have a unique tag name per module

                if (string.IsNullOrEmpty(processedType.elementPath)) {
                    if (processedType.rawType.Assembly != typeof(UIElement).Assembly) {
                        diagnostics.LogError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} requires a location providing attribute." +
                                             $" Please use [{nameof(RecordFilePathAttribute)}], [{nameof(TemplateAttribute)}], " +
                                             $"[{nameof(ImportStyleSheetAttribute)}], [{nameof(StyleAttribute)}]" +
                                             $" or [{nameof(TemplateTagNameAttribute)}] on the class. If you intend not to provide a template you can also use [{nameof(ContainerElementAttribute)}].");
                        continue;
                    }
                }

                // todo -- this needs to be concurrent dictionary to be threadsafe
                TypeProcessor.typeMap[processedType.rawType] = processedType;
            }

            Debug.Log("Created element type meta data in: " + sw.Elapsed.TotalMilliseconds.ToString("F3"));

            if (diagnostics.HasErrors()) {
                diagnostics.Dump();
            }
        }

        private static List<UIModule> FindModules() {
#if UNITY_EDITOR
            List<UIModule> moduleList = new List<UIModule>();

            Stopwatch sw = Stopwatch.StartNew();

            string[] assets = AssetDatabase.FindAssets(".uimodule t:TextAsset");

            string projectPath = UnityEngine.Application.dataPath;
            projectPath = projectPath.Replace("/Assets", "");

            for (int i = 0; i < assets.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                TextAsset source = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                UIModuleDefinition p = JsonUtility.FromJson<UIModuleDefinition>(source.text);
                path = Path.GetFullPath(Path.Combine(projectPath, path));
                string fileName = Path.GetFileName(path);
                UIModule module = new UIModule() {
                    location = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar,
                    path = path,
                    data = p,
                    name = fileName.Substring(0, fileName.Length - ".uimodule.json".Length)
                };
                module.Initialize();
                moduleList.Add(module);
            }

            Debug.Log("Found modules in: " + sw.Elapsed.TotalMilliseconds.ToString("F3"));

            return moduleList;
#else
            throw new Execption("UIForia dynamic apps are only supported in the editor for now");
#endif
        }

        public static EditorApplication CreateEditorApplication(string applicationId, Type entryType) {
            Initialize();

            EditorApplication application = new EditorApplication(GetUniqueApplicationName(applicationId));

            if (entryType.IsGenericType || entryType.IsAbstract || !(typeof(UIElement).IsAssignableFrom(entryType))) {
                application.IsValid = false;
            }
            else { }

            return application;
        }

        public static TestApplication CreateTestApplication(string applicationId, Type entryType) {
            Initialize();

            TestApplication application = new TestApplication(GetUniqueApplicationName(applicationId));

            if (entryType.IsGenericType || entryType.IsAbstract || !(typeof(UIElement).IsAssignableFrom(entryType))) {
                application.IsValid = false;
            }
            else { }

            return application;
        }

        public static CompilationResult PrecompileGameApplication(string applicationId, PrecompileSettings precompileSettings) {
            Initialize();

            return new CompilationResult();
        }

        public static CompilationResult PrecompileEditorApplication(string applicationId, PrecompileSettings precompileSettings) {
            Initialize();

            return new CompilationResult();
        }

        public static CompilationResult PrecompileTestApplication(string applicationId, PrecompileSettings precompileSettings) {
            Initialize();

            return new CompilationResult();
        }

        public static GameApplication2 LoadGameApplication(string applicationId, PrecompileSettings precompileSettings) {
            GameApplication2 application = new GameApplication2(GetUniqueApplicationName(applicationId));

            return application;
        }

    }

}