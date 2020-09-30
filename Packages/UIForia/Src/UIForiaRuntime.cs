using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using UIForia.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntryPointAttribute : Attribute { }

    public struct DiagnosticEntry {

        public string message;
        public string filePath;
        public string category;
        public DateTime timestamp;
        public Exception exception;
        public int lineNumber;
        public int columnNumber;
        public DiagnosticType diagnosticType;

    }

    public enum DiagnosticType : ushort {

        Error,
        Exception,
        Info,
        Warning

    }

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

    internal class UIModule2 {

        public string location;
        public UIModuleDefinition data;

        public Func<TemplateLookup, TemplateLocation> templateLocator;
        public Func<string, string, string> styleLocator;
        public Dictionary<string, ProcessedType> tagNameMap;
        public string path;

        public UIModule2() {
            tagNameMap = new Dictionary<string, ProcessedType>();
        }

        public void Initialize() {

            if (!string.IsNullOrEmpty(data.TemplateLocator)) {

                if (TypeScanner.templateLocators.TryGetValue(data.TemplateLocator, out Func<TemplateLookup, TemplateLocation> locator)) {
                    this.templateLocator = locator;
                }

            }

            if (!string.IsNullOrEmpty(data.StyleLocator)) {
                if (TypeScanner.styleLocators.TryGetValue(data.StyleLocator, out Func<string, string, string> locator)) {
                    this.styleLocator = locator;
                }
            }

        }

        public TemplateLocation ResolveTemplate(TemplateLookup lookup) {
            return templateLocator?.Invoke(lookup) ?? DefaultLocators.LocateTemplate(lookup);
        }

        public string ResolveStyle(string elementPath, string stylePath) {
            return styleLocator?.Invoke(elementPath, stylePath) ?? DefaultLocators.LocateStyle(elementPath, stylePath);
        }

    }

    internal class UIModuleDefinition {

        public string Prototype;
        public string TemplateLocator;
        public string StyleLocator;
        public string AssetLocator;
        public string[] DefaultNamespaces;
        public string[] ImplicitStyleReferences;
        public string[] ImplicitModuleReferences;

    }

    public static class UIForiaRuntime2 {

        private static LightList<Application> s_Applications;

        internal static void Reset() {
            // kill all apps, kill all caches
        }

        public static void Initialize() {

            // todo -- consider pre-warming style cache on startup
            // basically suck in all style files ONCE
            // parse them all
            // never do this again until next editor session
            // could probably do the same for templates, except I can't look for xml files since we'd parse unwanted shit, unless we check for <UITemplate> in each
            // probably not worth it
            // store results ...somewhere

#if UNITY_EDITOR
            if (SessionState.GetInt("UIForiaRuntime::DidInitialStyleParse", 0) == 0) {
                SessionState.SetInt("UIForiaRuntime::DidInitialStyleParse", 1);
                // IEnumerable<string> z = Directory.EnumerateFiles(Path.Combine(UnityEngine.Application.dataPath, ".."), "*.style", SearchOption.AllDirectories);
                // int cnt = 0;
                // foreach(var f in z) {
                //     cnt++;
                // }
                // Debug.Log(cnt);
            }
#endif

        }

        private static string GetUniqueApplicationName(string applicationId) {
            // todo -- figure this out
            return applicationId;
        }

        private static Dictionary<ProcessedType, UIModule2> BuildModuleMap(List<UIModule2> moduleDefinitions) {

            Stopwatch sw = Stopwatch.StartNew();

            Dictionary<ProcessedType, UIModule2> moduleMap = new Dictionary<ProcessedType, UIModule2>();

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
                            return null;
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

                // this should be done PER application so that if one app is running and we refresh another,
                // the running one won't need to adjust itself until it gets refreshed
                List<UIModule2> moduleList = FindModules();
                Dictionary<ProcessedType, UIModule2> map = BuildModuleMap(moduleList);
                MapElementsToTemplates(map);
                BuildTagNameMaps();
                CompileApplication(entryType);

            }

            return application;
        }

        private static void BuildTagNameMaps() { }

        private static void MapElementsToTemplates(Dictionary<ProcessedType, UIModule2> map) {
            Stopwatch sw = Stopwatch.StartNew();

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {

                ProcessedType processedType = kvp.Value;
                if (!string.IsNullOrEmpty(processedType.templatePath)) {

                    if (map.TryGetValue(processedType, out UIModule2 module)) {

                        TemplateLookup lookup = new TemplateLookup() {
                            elementLocation = processedType.elementPath,
                            elementType = processedType.rawType,
                            moduleLocation = module.location,
                            modulePath = module.path,
                            templatePath = processedType.templatePath,
                            templateId = processedType.templateId
                        };
                        
                        TemplateLocation templatePath = module.ResolveTemplate(lookup);

                        // look up the template in cache
                        // if its not there, add to the parse list
                        // its timestamp changed, add to the parse list

                        if (!File.Exists(templatePath.filePath)) {
                            Debug.Log(templatePath.filePath + " was not found " + processedType.rawType.GetTypeName());
                        }

                    }

                }
            }

            Debug.Log("map elements to template: " + sw.Elapsed.TotalMilliseconds.ToString("F3"));

        }

        private static void CompileApplication(Type entryType) {
            // scan all templates
            // resolve all the tags that we can
            // go wide compiling those tags

            // challenge #1
            // find all non generic element tags and compile their hydrations

            ProcessedType processedType = TypeProcessor.typeMap[entryType];

            // processedType.resolvedTemplateLocation;

            // how do I find what templates are included for an entry point?
            // get the template for the entry point
            // traverse node tree
            // resolve tag

            // still need to map element to template

        }

        private static void CreateProcessedElementTypes() {

            // this should only run once on startup, the type map might get appended to during compilation but the types will never change
            // while an application is running

            if (TypeProcessor.typeMap.Count > 0) {
                return;
            }

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

            if (diagnostics.HasErrors()) {
                diagnostics.Dump();
            }
        }

        private static List<UIModule2> FindModules() {
            IEnumerable<string> z = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..")), "*.uimodule.json", SearchOption.AllDirectories);

            List<UIModule2> moduleList = new List<UIModule2>();
            foreach (string f in z) {
                UIModuleDefinition p = JsonUtility.FromJson<UIModuleDefinition>(File.ReadAllText(f));
                UIModule2 module = new UIModule2() {
                    location = Path.GetDirectoryName(f) + Path.DirectorySeparatorChar,
                    path = f,
                    data = p
                };
                module.Initialize();
                moduleList.Add(module);
            }

            return moduleList;
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