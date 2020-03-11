using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Style2;
using UIForia.Util;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public class ModuleLoadException : Exception {

        public ModuleLoadException(string message) : base(message) { }

    }

    public abstract class Module {

        private bool dependenciesResolved;
        private LightList<StyleSheet2> styleSheets;
        private ModuleReference[] moduleReferences;
        private IList<ModuleReference> cachedDependencies;

        private List<bool> conditionResults;
        private IList<StyleCondition> styleConditions;
        private VisitMark visitedMark;

        private bool initialized;
        private string moduleName;
        private Type defaultRootType;
        private readonly IList<Type> dynamicTypeReferences;
        private readonly IList<ModuleReference> dependencies;
        private readonly Dictionary<string, TemplateSource> fileSources;

        private static readonly HashSet<Assembly> s_Assemblies = new HashSet<Assembly>();
        private static readonly Dictionary<Type, Module> s_ModuleInstances = new Dictionary<Type, Module>();

        private static readonly HashSet<string> s_StringHashSet = new HashSet<string>();
        private static readonly HashSet<Type> s_TypeHashSet = new HashSet<Type>();

        private static bool s_ConstructionAllowed;
        private static string[] s_BuiltInElementTags;

        internal static Module s_BuiltInModule;

        internal Dictionary<string, ProcessedType> tagNameMap;
        private List<Diagnostic> diagnostics;

        private object diagnosticLock = new object();

        public struct Diagnostic {

            public string message;
            public string filePath;
            public int lineNumber;
            public int columnNumber;
            public DiagnosticType diagnosticType;

        }

        public enum DiagnosticType {

            ParseError,
            ParseWarning

        }

        protected Module() {
            if (!s_ConstructionAllowed) {
                throw new ModuleLoadException("Modules should never have their constructor called.");
            }

            this.tagNameMap = new Dictionary<string, ProcessedType>();
            this.fileSources = new Dictionary<string, TemplateSource>();
            this.dependencies = new List<ModuleReference>();
            this.dynamicTypeReferences = new List<Type>();
            this.visitedMark = VisitMark.Alive;
        }

        internal void ReportParseError(string file, string message, int lineNumber, int col = -1) {
            lock (diagnosticLock) {
                diagnostics = diagnostics ?? new List<Diagnostic>();
                diagnostics.Add(new Diagnostic() {
                    filePath = file,
                    message = message,
                    lineNumber = lineNumber,
                    columnNumber = col,
                    diagnosticType = DiagnosticType.ParseError
                });
            }
        }

        internal Action zz__INTERNAL_DO_NOT_CALL; // use this for precompiled loading instead of doing type reflection to find caller type

        public string location { get; private set; }

        public virtual void Configure() { }

        protected void SetDefaultRootType<TElementType>() where TElementType : UIElement {
            defaultRootType = typeof(TElementType);
        }

        protected void SetModuleName(string moduleName) {
            this.moduleName = moduleName;
        }

        protected void AddDependency<TDependency>(string alias) where TDependency : Module, new() {
            if (typeof(TDependency).IsAbstract) {
                throw new InvalidArgumentException("Dependencies must be concrete classes. " + TypeNameGenerator.GetTypeName(typeof(TDependency)) + " is abstract");
            }

            dependencies.Add(new ModuleReference(typeof(TDependency), alias));
        }

        private void ValidateDependencies(HashSet<string> stringHash, HashSet<Type> typeHash) {
            if (GetType() != typeof(BuiltInElementsModule)) {
                bool found = false;
                for (int i = 0; i < dependencies.Count; i++) {
                    if (dependencies[i].GetModuleType() == typeof(BuiltInElementsModule)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    dependencies.Add(new ModuleReference(typeof(BuiltInElementsModule)));
                }
            }

            for (int i = 0; i < dependencies.Count; i++) {
                Type dependencyType = dependencies[i].GetModuleType();
                string alias = dependencies[i].GetAlias();

                if (stringHash.Contains(alias)) {
                    throw new ModuleLoadException($"Duplicate alias or module name {alias}");
                }

                if (typeHash.Contains(dependencyType)) {
                    throw new ModuleLoadException($"Duplicate dependency of type {TypeNameGenerator.GetTypeName(dependencyType)} in module {moduleName}");
                }

                stringHash.Add(alias);
                typeHash.Add(dependencyType);
            }
        }

        protected void AddDynamicTypeReference<TReference>() where TReference : UIElement {
            dynamicTypeReferences.Add(typeof(TReference));
        }

        public virtual void BuildCustomStyles(IStyleCodeGenerator generator) { }

        public static T LoadRootModule<T>() where T : Module {
            return LoadRootModule(typeof(T)) as T;
        }

        internal static Module GetModuleInstance(Type moduleType) {
            if (!s_ModuleInstances.TryGetValue(moduleType, out Module instance)) {
                s_ConstructionAllowed = true;
                instance = (Module) Activator.CreateInstance(moduleType);
                string moduleLocation = GetFilePathFromAttribute(moduleType);
                instance.location = Path.GetDirectoryName(moduleLocation) + Path.DirectorySeparatorChar;
                s_ModuleInstances[moduleType] = instance;
                s_ConstructionAllowed = false;
            }

            return instance;
        }

        internal static Module LoadRootModule(Type rootType) {
            TypeProcessor.Initialize();

            ProcessedType processedType = TypeProcessor.GetProcessedType(rootType);

            if (processedType == null) {
                throw new Exception("Unable to find concrete UIElement from " + rootType);
            }

            Module rootModule = processedType.module;

            if (rootModule == null) {
                throw new Exception("Unable to find module for type " + rootType);
            }

            GatherDependencies(rootModule);

            List<Module> dependencySort = DependencySort(rootModule);

            TypeProcessor.Initialize();

            for (int i = 0; i < dependencySort.Count; i++) {
                if (!dependencySort[i].initialized) {
                    dependencySort[i].Configure();
                    dependencySort[i].initialized = true;
                }
            }

            for (int i = 0; i < dependencySort.Count; i++) {
                Module module = dependencySort[i];

                IEnumerable<string> templateFiles = Directory.EnumerateFiles(module.location, "*.xml", SearchOption.AllDirectories);
                IEnumerable<string> styleFiles = Directory.EnumerateFiles(module.location, "*.style", SearchOption.AllDirectories);

                foreach (string file in templateFiles) {
                    module.AddTemplateFile(file);
                }

                foreach (string file in styleFiles) {
                    module.AddStyleFile(file);
                }
            }

            if (!Parse(dependencySort)) {
                return null;
            }

            if (!Compile(dependencySort)) {
                return null;
            }
            
            return rootModule;
        }

        private static bool Parse(List<Module> modulesToParse) {
            List<TemplateParseInfo> parseInfos = new List<TemplateParseInfo>(128);

            for (int i = 0; i < modulesToParse.Count; i++) {
                Module module = modulesToParse[i];

                Dictionary<string, TemplateSource>.ValueCollection values = module.fileSources.Values;

                // tag name resolve needs to happen in 2nd pass after all other modules were parsed
                // or need to run in job with dependencies more likely

                foreach (TemplateSource value in values) {
                    TemplateParseInfo parseInfo = new TemplateParseInfo() {
                        module = module,
                        source = value.source,
                        path = value.path
                    };

                    parseInfos.Add(parseInfo);
                }
            }

            TemplateParseJob parseJob = new TemplateParseJob();
            parseJob.handle = GCHandle.Alloc(parseInfos); 
            JobHandle x = parseJob.Schedule(); //parseInfos.Count, 1);
            //JobHandle x = parseJob.Schedule(parseInfos.Count, 4);
            x.Complete();

            bool failedToParse = false;
            
            for (int i = 0; i < modulesToParse.Count; i++) {
                List<Diagnostic> diagnostics = modulesToParse[i].diagnostics;
                if (diagnostics == null) continue;
                failedToParse = true;
                for (int j = 0; j < diagnostics.Count; j++) {
                    Debug.LogError($"{diagnostics[j].filePath} at line {diagnostics[j].lineNumber}:{diagnostics[j].columnNumber} -> {diagnostics[j].message}");
                }

                diagnostics.Clear();
            }


            return !failedToParse;

        }

        private static bool Compile(List<Module> modulesToCompile) {
            for (int i = 0; i < modulesToCompile.Count; i++) {
                
            }
            return true;
        }

        private struct TemplateSource {

            public string source;
            public DateTime lastReadTime;
            public string path;

        }

        private void AddTemplateFile(string file) {
            DateTime lastModified = File.GetLastWriteTime(file);
            if (fileSources.TryGetValue(file, out TemplateSource source)) {
                if (lastModified == source.lastReadTime) {
                    return;
                }
            }

            fileSources[file] = new TemplateSource() {
                lastReadTime = lastModified,
                path = file,
                source = File.ReadAllText(file)
            };
        }

        private void AddStyleFile(string file) {
            DateTime lastModified = File.GetLastWriteTime(file);
            if (fileSources.TryGetValue(file, out TemplateSource source)) {
                if (lastModified == source.lastReadTime) {
                    return;
                }
            }

            fileSources[file] = new TemplateSource() {
                lastReadTime = lastModified,
                path = file,
                source = File.ReadAllText(file)
            };
        }

        protected internal void UpdateConditions(DisplayConfiguration displayConfiguration) {
            if (styleConditions == null) return;

            conditionResults = conditionResults ?? new List<bool>();
            conditionResults.Clear();

            for (int i = 0; i < styleConditions.Count; i++) {
                conditionResults.Add(styleConditions[i].fn(displayConfiguration));
            }
        }

        public List<bool> GetDisplayConditions() {
            return conditionResults;
        }

        protected internal void RegisterDisplayCondition(string condition, Func<DisplayConfiguration, bool> fn) {
            if (styleConditions == null) {
                styleConditions = new List<StyleCondition>();
                styleConditions.Add(new StyleCondition(0, condition, fn));
                return;
            }

            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == condition) {
                    styleConditions[i] = new StyleCondition(i, condition, fn);
                    return;
                }
            }

            styleConditions.Add(new StyleCondition(styleConditions.Count, condition, fn));
        }

        public int GetDisplayConditionId(CharSpan conditionSpan) {
            if (styleConditions == null) return -1;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionSpan) {
                    return i;
                }
            }

            return -1;
        }

        public bool HasStyleCondition(string conditionName) {
            return HasStyleCondition(new CharSpan(conditionName));
        }

        public bool HasStyleCondition(CharSpan conditionName) {
            if (styleConditions == null) return false;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionName) {
                    return true;
                }
            }

            return false;
        }

        private enum VisitMark {

            Alive,
            Dead,
            Undead

        }

        private static void GatherDependencies(Module m) {
            if (m.dependenciesResolved) {
                return;
            }

            m.dependenciesResolved = true;

            IList<ModuleReference> dependencies = m.dependencies;

            s_StringHashSet.Clear();
            s_TypeHashSet.Clear();

            m.ValidateDependencies(s_StringHashSet, s_TypeHashSet);

            for (int i = 0; i < dependencies.Count; i++) {
                Type moduleType = dependencies[i].GetModuleType();

                Module instance = GetModuleInstance(moduleType);

                dependencies[i].ResolveModule(instance);

                GatherDependencies(instance);
            }
        }

        private static List<Module> DependencySort(Module root) {
            List<Module> sorted = new List<Module>(CountSizeAndReset(root, 1));

            Visit(root, sorted);

            return sorted;
        }

        private static int CountSizeAndReset(Module module, int count) {
            module.visitedMark = VisitMark.Alive;

            IList<ModuleReference> dependencies = module.dependencies;

            for (int i = 0; i < dependencies.Count; i++) {
                count += CountSizeAndReset(dependencies[i].GetModuleInstance(), count);
            }

            return count;
        }

        private static void Visit(Module module, List<Module> sorted) {
            if (module.visitedMark == VisitMark.Dead) {
                return;
            }

            if (module.visitedMark == VisitMark.Undead) {
                throw new ModuleLoadException("Cyclic dependency found."); // probably need a stack to track this properly
            }

            module.visitedMark = VisitMark.Undead;

            IList<ModuleReference> dependencies = module.dependencies;

            for (int i = 0; i < dependencies.Count; i++) {
                Visit(dependencies[i].GetModuleInstance(), sorted);
            }

            module.visitedMark = VisitMark.Undead;

            sorted.Add(module);
        }

        private static Module[] modules;
        private string assetPath;

        internal static void ValidateModulePaths() {
            modules = s_ModuleInstances.Values.ToArray();

            for (int i = 0; i < modules.Length; i++) {
                for (int j = i; j < modules.Length; j++) {
                    Module moduleI = modules[i];
                    Module moduleJ = modules[j];
                    if (moduleI == moduleJ) continue;

                    if (moduleI.location.StartsWith(moduleJ.location, StringComparison.Ordinal)) {
                        throw new ModuleLoadException("Nested Modules are not yet supported. " +
                                                      $"{TypeNameGenerator.GetTypeName(moduleI.GetType())} is a parent of " +
                                                      $"{TypeNameGenerator.GetTypeName(moduleJ.GetType())}. ({moduleJ.location})");
                    }

                    if (moduleJ.location.StartsWith(moduleI.location, StringComparison.Ordinal)) {
                        throw new ModuleLoadException("Nested Modules are not yet supported. " +
                                                      $"{TypeNameGenerator.GetTypeName(moduleJ.GetType())} is a parent of " +
                                                      $"{TypeNameGenerator.GetTypeName(moduleI.GetType())}. ({moduleI.location})");
                    }
                }
            }
        }

        internal static Type GetModuleTypeFromElementType(Type type) {
            if (!type.IsSubclassOf(typeof(UIElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is not a subclass of {TypeNameGenerator.GetTypeName(typeof(UIElement))}.");
            }

            if (type.IsAbstract) {
                throw new ModuleLoadException($"Cannot create a module for a type that is abstract. Tried to create module for {TypeNameGenerator.GetTypeName(typeof(UIElement))} but it was abstract.");
            }

            if (type.IsSubclassOf(typeof(UITextElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is a subclass of {TypeNameGenerator.GetTypeName(typeof(UITextElement))}. Tried to create module for {TypeNameGenerator.GetTypeName(type)}");
            }

            ProcessedType processedType = TypeProcessor.GetProcessedType(type);
            if (processedType.IsContainerElement) {
                throw new ModuleLoadException($"Cannot create a module for a type that is declared as a Container element. Tried to create module for {TypeNameGenerator.GetTypeName(type)}");
            }

            if (type.IsSubclassOf(typeof(UIImageElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is a subclass of {TypeNameGenerator.GetTypeName(typeof(UIImageElement))}.Tried to create module for {TypeNameGenerator.GetTypeName(type)}");
            }

            TemplateAttribute attribute = type.GetCustomAttribute<TemplateAttribute>();

            if (attribute == null) {
                throw new ModuleLoadException($"Can only create an application when the root element is annotated with [{nameof(TemplateAttribute)}]. {TypeNameGenerator.GetTypeName(type)} is missing one.");
            }

            return null;
        }

        internal static Module GetModuleFromElementType(ProcessedType processedType) {
            throw new Exception("Finish this");
        }

        private static string GetFilePathFromAttribute(Type moduleType) {
            RecordFilePathAttribute attr = moduleType.GetCustomAttribute<RecordFilePathAttribute>();

            if (attr == null) {
                throw new ModuleLoadException($"Modules must provide a [{TypeNameGenerator.GetTypeName(typeof(RecordFilePathAttribute))}] attribute. {TypeNameGenerator.GetTypeName(moduleType)} is missing one.");
            }

            return attr.filePath;
        }

        internal CompiledTemplateData LoadRuntimeTemplates(Type rootType) {
            return null;
        }

        public static bool TryGetInstance(Type moduleType, out Module module) {
            return s_ModuleInstances.TryGetValue(moduleType, out module);
        }

        public struct TemplateLookup {

            public readonly Type elementType;
            public readonly string elementFilePath;
            public readonly string declaredTemplatePath;

            public TemplateLookup(Type elementType, string elementFilePath, string declaredTemplatePath) {
                this.elementType = elementType;
                this.elementFilePath = elementFilePath;
                this.declaredTemplatePath = declaredTemplatePath;
            }

        }

        public virtual string GetTemplatePath(in TemplateLookup lookup) {
            return Path.GetFullPath(Path.Combine(location, lookup.declaredTemplatePath));
        }

        internal ProcessedType ResolveTagName(string prefix, string tagName, StructList<UsingDeclaration> usings) {

            if (usings != null && usings.size != 0) {
                // todo -- solve using lookups     
            }

            if (string.IsNullOrEmpty(prefix)) {
                int idx = Array.BinarySearch(s_BuiltInElementTags, 0, s_BuiltInElementTags.Length, tagName);
                if (idx < 0) {
                    // not found, search module's tags
                    tagNameMap.TryGetValue(tagName, out ProcessedType retn);
                    return retn;
                }
                else {
                    s_BuiltInModule.tagNameMap.TryGetValue(tagName, out ProcessedType retn);
                    return retn;
                }
            }
            else {
                for (int i = 0; i < dependencies.Count; i++) {
                    if (dependencies[i].GetAlias() == prefix) {
                        dependencies[i].GetModuleInstance().tagNameMap.TryGetValue(tagName, out ProcessedType retn);
                        return retn;
                    }
                }
                return null;
            }
        }

        public static IList<ProcessedType> GetTemplateElements(Assembly assembly) {
            TypeProcessor.Initialize();

            List<ProcessedType> retn = new List<ProcessedType>();

            for (int i = 0; i < modules.Length; i++) {
                Module module = modules[i];

                foreach (KeyValuePair<string, ProcessedType> kvp in module.tagNameMap) {
                    retn.Add(kvp.Value);
                }
            }

            return retn;
        }

        public static bool TryGetModule(ProcessedType processedType, out Module module) {
            string path = processedType.elementPath;

            if (string.IsNullOrEmpty(path)) {
                module = default;
                return false;
            }

            for (int k = 0; k < modules.Length; k++) {
                if (path.StartsWith(modules[k].location, StringComparison.Ordinal)) {
                    module = modules[k];
                    return true;
                }
            }

            module = default;
            return false;
        }

        public struct TemplateParseInfo {

            public Module module;
            public string source;
            public string path;
            public object result;

        }

        internal static void InitializeModules(IList<Type> moduleTypes) {
            for (int i = 0; i < moduleTypes.Count; i++) {
                Module instance = GetModuleInstance(moduleTypes[i]);
                if (instance is BuiltInElementsModule) {
                    s_BuiltInModule = instance;
                }
            }
            ValidateModulePaths();
        }

        internal static void CreateBuiltInTypeArray() {
            s_BuiltInElementTags = s_BuiltInModule.tagNameMap.Keys.ToArray();
            Array.Sort(s_BuiltInElementTags);
        }

    }

}