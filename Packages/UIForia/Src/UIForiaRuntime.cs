using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia {

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
        public string name;
        public UIModule2[] implicitModuleReferences;

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

    internal class TemplateParseCache {

        private LightList<string> missingTemplates;
        private LightList<string> parseUpdateList;

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

    internal class Compilation {

        public Application application;
        public CompilationType compilationType;
        public ProcessedType entryType;
        public UIForiaRuntime.TemplateParseCache parseCache;
        public Dictionary<ProcessedType, UIModule2> moduleMap;

    }

    public static class UIForiaRuntime {

        private static LightList<Application> s_Applications;
        private static TemplateParseCache s_ParseCache;
        
        internal static void Reset() {
            // kill all apps, kill all caches
        }

        public static void Initialize() {

#if UNITY_EDITOR
            if (SessionState.GetInt("UIForiaRuntime::DidInitialStyleParse", 0) == 0) {
                SessionState.SetInt("UIForiaRuntime::DidInitialStyleParse", 1);
                // IEnumerable<string> z = Directory.EnumerateFiles(Path.Combine(UnityEngine.Application.dataPath, ".."), "*.style", SearchOption.AllDirectories);
            }
#endif

            Debug.Log(FileUtil.GetUniqueTempPathInProject() + "_uiforia_template_cache.bytes");

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
                
                Compilation compilation = new Compilation();
                compilation.entryType = TypeProcessor.GetProcessedType(entryType);
                compilation.parseCache = new TemplateParseCache();
                compilation.compilationType = CompilationType.Dynamic;
                compilation.application = application;
                compilation.parseCache.Initialize();
                
                CreateProcessedElementTypes();

                // this should be done PER application so that if one app is running and we refresh another,
                // the running one won't need to adjust itself until it gets refreshed
                List<UIModule2> moduleList = FindModules();
                compilation.moduleMap = BuildModuleMap(moduleList);
                MapElementsToTemplates(compilation);
                BuildTagNameMaps(compilation);
                CompileApplication(entryType);

            }

            return application;
        }

        public class TemplateParseCache {

            private string cacheFilePath;

            private Dictionary<string, TemplateFileShell> templateMap;

            internal TemplateParseCache() {
                templateMap = new Dictionary<string, TemplateFileShell>(128);
            }
            
            public void Initialize(bool forceReparse = false) {

                if (cacheFilePath != null) {
                    return;
                }

                cacheFilePath = GetCacheFilePath();

                if (forceReparse || !File.Exists(cacheFilePath)) {
                    BuildCache();
                }
                else {
                    HydrateCache();
                }

            }

            private void HydrateCache() {
                byte[] bytes = File.ReadAllBytes(cacheFilePath);
                ManagedByteBuffer buffer = new ManagedByteBuffer(bytes);
                buffer.Read(out int templateCount);
                buffer.Read(out int version);

                if (version != UIForiaMLParser.Version) {
                    BuildCache();
                    return;
                }

                for (int i = 0; i < templateCount; i++) {
                    
                    TemplateFileShell shell = new TemplateFileShell();
                    
                    // must always deserialize so the buffer pointer knows how much data to skip
                    shell.Deserialize(ref buffer);
                    
                    // could consider doing these validation checks when resolving the template instead of here
                    // that reduces (maybe) the number of times we have to run the checks 
                    // we should also check per compilation run that the file hasn't changed since we started compiling
                    if (File.Exists(shell.filePath)) {
                        DateTime lastWrite = File.GetLastWriteTime(shell.filePath);
                        if (shell.lastWriteTime == lastWrite) {
                            templateMap[shell.filePath] = shell;
                        }
                    }

                }

                if (buffer.ptr != buffer.array.Length) {
                    Debug.LogWarning("Encountered an issue when loading UIForia templates from cache, reparsing files and rebuilding cache");
                    BuildCache();
                }
                
            }

            private void BuildCache() {

                templateMap.Clear();
                // todo -- could improve search time by only finding module locations and looking only at those xml files

                Diagnostics diagnostics = new Diagnostics();
                IEnumerable<string> itr = Directory.EnumerateFiles(Path.GetFullPath(UnityEngine.Application.dataPath), "*.xml", SearchOption.AllDirectories);

                UIForiaMLParser parser = new UIForiaMLParser(diagnostics);

                LightList<string> list = new LightList<string>(128);
                LightList<string> contentList = new LightList<string>(128);
                LightList<DateTime> timestamps = new LightList<DateTime>(128);

                foreach (string file in itr) {
                    string contents = File.ReadAllText(file);
                    DateTime timestamp = File.GetLastWriteTime(file);
                    
                    if (UIForiaMLParser.IsProbablyTemplate(contents)) {
                        list.Add(file);
                        contentList.Add(contents);
                        timestamps.Add(timestamp);
                    }
                }

                IEnumerable<string> itr2 = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages")), "*.xml", SearchOption.AllDirectories);

                foreach (string file in itr2) {
                    string contents = File.ReadAllText(file);
                    DateTime timestamp = File.GetLastWriteTime(file);
                    if (UIForiaMLParser.IsProbablyTemplate(contents)) {
                        list.Add(file);
                        contentList.Add(contents);
                        timestamps.Add(timestamp);
                    }
                }

                ManagedByteBuffer buffer = new ManagedByteBuffer(new byte[TypedUnsafe.Kilobytes(512)]);

                LightList<TemplateFileShell> files = new LightList<TemplateFileShell>(list.size);

                for (int i = 0; i < list.size; i++) {

                    DateTime timestamp = timestamps.array[i];
                    
                    if (parser.TryParse(list.array[i], contentList.array[i], out TemplateFileShell result)) {
                        result.lastWriteTime = timestamp;
                        files.Add(result);
                    }

                    diagnostics.Clear();

                }

                buffer.Write(files.size);
                buffer.Write(UIForiaMLParser.Version);

                for (int i = 0; i < files.size; i++) {
                    templateMap[files[i].filePath] = files[i];
                    files.array[i].Serialize(ref buffer);
                }

                // WriteCacheResult since it didnt exist before or we are initializing
                using (FileStream fStream = new FileStream(cacheFilePath, FileMode.OpenOrCreate)) {
                    fStream.Write(buffer.array, 0, buffer.ptr);
                }

            }

            private string GetCacheFilePath() {
                return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Temp", "__UIForiaTemplateCache__.bytes"));
            }

            public TemplateFileShell GetTemplate(ProcessedType type) {
                // todo -- lookup n stuff, parse if needed / missing
                return templateMap[type.resolvedTemplateLocation.Value.filePath];
            }

        }

        private static void BuildTagNameMaps(Compilation compilation) {

            TemplateFileShell shell = compilation.parseCache.GetTemplate(compilation.entryType);
            TemplateASTRoot template = shell.GetRootTemplateForType(compilation.entryType);

            int ptr = template.templateIndex;
            
            StructStack<int> stack = new StructStack<int>();
            // I can map every node to a processed type 
            // later we'll figure out which generics need to swapped in 
            // also producer / consumer pattern possible
            // 1 producer will traverse and enqueue all the types to be resolved
            
            
            stack.Push(ptr);
            
            // parser could also just do this, at least the 'traversal' part of module + tag name
            // cant do resolve though
            UIModule2 module = compilation.moduleMap[compilation.entryType];
            LightList<ProcessedType> tagBuffer = new LightList<ProcessedType>(128);
            
            while (stack.size > 0) {

                int idx = stack.PopUnchecked();
                
                TemplateASTNode node = shell.templateNodes[idx];
                
                if ((node.templateNodeType & TemplateNodeType.Slot) != 0) {
                    CharSpan moduleName = shell.GetCharSpan(node.moduleNameRange);
                    CharSpan tagName = shell.GetCharSpan(node.tagNameRange);
                    
                    
                    // need to know which modules to look in if non is provided
                    // start looking in 'this' module 
                    // if didnt find it, walk down the list of implicit modules in declaration order for the module
                    // if didnt find it, look in UIForia default
                    // if didnt find it, fail

                    ProcessedType tagType = ResolveTagName(module, moduleName, tagName);
                    
                    tagBuffer.Add(tagType); // might be null

                }
                else {
                    
                }
                // traverse all nodes
                // resolve tag names
                // if node requires descent, add to queue

                for (int i = 0; i < node.childCount; i++) {
                    
                }
                
            }
            
            shell.processedTypes

        }

        private static ProcessedType ResolveTagName(UIModule2 module, string moduleName, string tagName) {

            if (string.IsNullOrEmpty(moduleName)) {
                // module.tagNameMap.TryGetValue(tagName)
            }
            else {
                // todo -- scan module aliases in <using> directives
                // todo -- module names must be unique
                // todo -- modules cannot be nested for now

                // scan already resolved module mapping first? not sure yet if its worth caching or not
                
                if (module.implicitModuleReferences != null) {
                    for (int i = 0; i < module.implicitModuleReferences.Length; i++) {
                        UIModule2[] reference = module.implicitModuleReferences;
                    }
                }

                for (int i = 0; i < moduleList.size; i++) {
                    
                    UIModule2 checkModule = moduleList[i];
                    
                    if (checkModule == module) {
                        continue;
                    }
                    
                    if (checkModule.name == moduleName) {

                        if (checkModule.tagNameMap.TryGetValue(tagName, out ProcessedType type)) {
                            return type;
                        }
                    }
                }

            }
                
            return null;
        }

        internal struct TagResolution {

            public UIModule2 module;
            public ProcessedType type;
            public TemplateFileShell shell;
            public int templateIdx;

        }

        private static void MapElementsToTemplates(Compilation compilation) {
            Stopwatch sw = Stopwatch.StartNew();

            // todo -- this should very likely be cached or parallelized somehow
            // bare minimum would be a faster implementation of path combine and GetFullPath

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {

                ProcessedType processedType = kvp.Value;
                if (!string.IsNullOrEmpty(processedType.templatePath)) {

                    if (compilation.moduleMap.TryGetValue(processedType, out UIModule2 module)) {

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

            // how to solve threading
            // producer / consumer
            // optimistic partial compilation -- don't like
            // parallel per entry point -> not great
            
            // can I map tag names without compiling? I think so
            // at which point I know which ones are generic
            // so i can probably do the tag name mapping up front / in parallel
            // at least most of the time...
            // generics are an issue kind of


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