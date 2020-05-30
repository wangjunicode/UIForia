using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Text;
using UIForia.Util;
using Unity.Jobs;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace UIForia.Src {

    public static class ModuleSystem {

        private static Module[] modules;

        internal static bool s_ConstructionAllowed;

        private static List<DiagnosticEntry> diagnostics;
        private static readonly object diagnosticLock = new object();
        private static TemplateCache s_TemplateCache;

        public static Module BuiltInModule { get; private set; }
        public static bool FailedToLoad { get; internal set; }
        public static bool IsLoading { get; internal set; }
        public static bool IsInitialized { get; private set; }

        private static List<DiagnosticEntry> diagnosticLog = new List<DiagnosticEntry>();
        internal static Dictionary<string, TemplateFileShell> s_TemplateShells;

        public struct Stats {

            public int elementCount;
            public int moduleCount;
            public double totalModuleLoadTime;
            public double getModuleTypesTime;
            public double createModulesTime;
            public double validateModulePathsTime;
            public double validateModuleDependenciesTime;
            public double assignElementsToModulesTime;
            public TypeResolver.Stats typeResolverStats;

        }

        internal static Stats stats;

        public static Stats GetStats() {
            return stats;
        }

        public static List<DiagnosticEntry> GetDiagnosticLogs() {
            return diagnosticLog;
        }

        public static void Initialize() {
            // if (IsInitialized) return;
            // IsInitialized = true;
            IsLoading = true;
            s_TemplateShells = new Dictionary<string, TemplateFileShell>();
            TypeResolver.Initialize();
            stats.typeResolverStats = TypeResolver.GetLoadStats();

            Stopwatch watch = Stopwatch.StartNew();
            Stopwatch total = Stopwatch.StartNew();

            // todo -- this likely needs to be generated per app when precompiling since I dont want the type processor included in builds

            IList<Type> moduleTypes = TypeProcessor.GetModuleTypes();
            stats.getModuleTypesTime = watch.Elapsed.TotalMilliseconds;

            modules = new Module[moduleTypes.Count];

            watch.Restart();

            for (int i = 0; i < moduleTypes.Count; i++) {
                s_ConstructionAllowed = true;
                Module instance = null;

                if (moduleTypes[i].IsAbstract) {
                    FailedToLoad = true;
                    LogDiagnosticInfo("Modules cannot be abstract but found " + moduleTypes[i].GetTypeName() + " which was");
                    continue;
                }

                try {
                    instance = (Module) Activator.CreateInstance(moduleTypes[i]);
                }
                catch (Exception e) {
                    LogDiagnosticException("Exception while creating module instance of type " + moduleTypes[i].GetTypeName(), e);
                    continue;
                }

                s_ConstructionAllowed = false;

                RecordFilePathAttribute attr = moduleTypes[i].GetCustomAttribute<RecordFilePathAttribute>();

                if (attr == null) {
                    LogDiagnosticError($"Modules must provide a [{TypeNameGenerator.GetTypeName(typeof(RecordFilePathAttribute))}] attribute. {TypeNameGenerator.GetTypeName(moduleTypes[i])} is missing one.");
                    continue;
                }

                string moduleLocation = attr.filePath;

                instance.location = Path.GetDirectoryName(moduleLocation) + Path.DirectorySeparatorChar;
                modules[i] = instance;

                if (instance.IsBuiltIn) {
                    BuiltInModule = instance;
                }

                try {
                    instance.Configure();
                }
                catch (Exception e) {
                    LogDiagnosticError(e.Message);
                }
            }

            stats.moduleCount = moduleTypes.Count;
            stats.createModulesTime = watch.Elapsed.TotalMilliseconds;

            if (FailedToLoad) {
                IsLoading = false;
                return;
            }

            watch.Restart();
            ValidateModulePaths();
            stats.validateModulePathsTime = watch.Elapsed.TotalMilliseconds;

            if (FailedToLoad) {
                IsLoading = false;
                return;
            }

            watch.Restart();
            ValidateModuleDependencies();
            stats.validateModuleDependenciesTime = watch.Elapsed.TotalMilliseconds;

            if (FailedToLoad) {
                IsLoading = false;
                return;
            }

            watch.Restart();
            AssignElementsToModules();
            stats.assignElementsToModulesTime = watch.Elapsed.TotalMilliseconds;

            IsLoading = false;
            stats.totalModuleLoadTime = total.Elapsed.TotalMilliseconds;
        }

        static ModuleSystem() {
            Initialize();
        }

        private static string indexCache;
        
        public static bool ModuleIndicesChanged() {
            if (indexCache != null) {
                return false;
            }
#if UNITY_EDITOR

            string lastIndexCache = UnityEditor.EditorPrefs.GetString("UIFORIA_MODULE_INDEX_CACHE");

            string[] names = new string[modules.Length];

            for (int i = 0; i < modules.Length; i++) {
                names[i] = modules[i].GetType().AssemblyQualifiedName;
            }

            Array.Sort(names);

            for (int i = 0; i < names.Length; i++) {
                TextUtil.StringBuilder.Append(names[i]);
            }

            indexCache = TextUtil.StringBuilder.ToString();
            TextUtil.StringBuilder.Clear();
            if (lastIndexCache == null || lastIndexCache != indexCache) {
                UnityEditor.EditorPrefs.SetString("UIFORIA_MODULE_INDEX_CACHE", indexCache);
                return true;
            }

            return false;
#else
            return true;

#endif
        }

        internal static void LogDiagnosticException(string message, Exception e) {
            diagnosticLog.Add(new DiagnosticEntry() {
                exception = e,
                message = message,
                diagnosticType = DiagnosticType.Exception
            });
            Debug.LogError(message + "\n" + e.Message);
            Debug.LogError(e.StackTrace);
            FailedToLoad = true;
        }

        internal static void LogDiagnosticError(string message) {
            diagnosticLog.Add(new DiagnosticEntry() {
                message = message,
                diagnosticType = DiagnosticType.Error
            });
            Debug.LogError(message);
            FailedToLoad = true;
        }

        internal static void LogDiagnosticInfo(string message) {
            diagnosticLog.Add(new DiagnosticEntry() {
                message = message,
                diagnosticType = DiagnosticType.Info
            });
            Debug.Log(message);
        }

        // cannot be mulithreaded without significant work
        private static void AssignElementsToModules() {

            IList<Type> elements = TypeProcessor.GetElementTypes();

            for (int i = 0; i < elements.Count; i++) {
                Type currentType = elements[i];

                if (currentType.IsAbstract) continue;

                ProcessedType processedType = ProcessedType.CreateFromType(currentType);

                // CreateFromType handles logging diagnostics, can just move on if we failed
                if (processedType == null) {
                    continue;
                }

                if (string.IsNullOrEmpty(processedType.elementPath)) {
                    if (processedType.rawType.Assembly != typeof(UIElement).Assembly) {
                        LogDiagnosticError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} requires a location providing attribute." +
                                           $" Please use [{nameof(RecordFilePathAttribute)}], [{nameof(TemplateAttribute)}], " +
                                           $"[{nameof(ImportStyleSheetAttribute)}], [{nameof(StyleAttribute)}]" +
                                           $" or [{nameof(TemplateTagNameAttribute)}] on the class. If you intend not to provide a template you can also use [{nameof(ContainerElementAttribute)}].");
                        continue;
                    }
                }

                TypeProcessor.typeMap[processedType.rawType] = processedType;

                TryAssignModule(processedType);

            }

            stats.elementCount = elements.Count;
        }

        internal static Module LoadRootModule(Type rootType) {

            ProcessedType processedType = TypeProcessor.GetProcessedType(rootType);

            if (processedType == null) {
                throw new Exception("Unable to find concrete UIElement from " + rootType);
            }

            Module rootModule = processedType.module;

            if (rootModule == null) {
                throw new Exception("Unable to find module for type " + rootType);
            }

            return rootModule;
        }

        private static void TryAssignModule(ProcessedType processedType) {
            string path = processedType.elementPath;

            if (processedType.module != null) {
                return;
            }

            if (processedType.rawType.IsAbstract) {
                return;
            }

            for (int i = 0; i < modules.Length; i++) {
                Module module = modules[i];

                if (!path.StartsWith(module.location, StringComparison.Ordinal)) {
                    continue;
                }

                module.AddElementType(processedType);

                try {
                    module.tagNameMap.Add(processedType.tagName, processedType);
                }
                catch (ArgumentException) {
                    LogDiagnosticError($"UIForia does not support multiple elements with the same tag name within the same module. Tried to register type {TypeNameGenerator.GetTypeName(processedType.rawType)} for `{processedType.tagName}` " +
                                       $" in module {TypeNameGenerator.GetTypeName(module.GetType())} at {module.location} " +
                                       $"but this tag name was already taken by type {TypeNameGenerator.GetTypeName(module.tagNameMap[processedType.tagName].rawType)}. " +
                                       "For generic overload types with multiple arguments you need to supply a unique [TagName] attribute");
                    return;
                }

                processedType.module = module;

                if (processedType.IsContainerElement) {
                    return;
                }

                try {
                    processedType.resolvedTemplateLocation = module.ResolveTemplatePath(new TemplateLookup(processedType));
                }
                catch (Exception e) {
                    LogDiagnosticException($"Unable to resolve template location for {processedType.rawType.GetTypeName()}", e);
                    return;
                }

                if (processedType.resolvedTemplateLocation == null) {
                    LogDiagnosticError($"Unable to locate template for {TypeNameGenerator.GetTypeName(processedType.rawType)}.");
                    return;
                }

                string templateLocation = processedType.resolvedTemplateLocation.Value.filePath;

                if (!s_TemplateShells.TryGetValue(templateLocation, out TemplateFileShell shell)) {
                    shell = new TemplateFileShell(templateLocation);
                    module.templateShells.Add(shell);
                    processedType.templateFileShell = shell;
                    shell.module = module;
                    s_TemplateShells.Add(templateLocation, shell);
                }
                else {
                    processedType.templateFileShell = shell;
                }

                return;
            }

            LogDiagnosticError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} at {processedType.elementPath} was not inside a module hierarchy.");

        }

        private static void ValidateModulePaths() {

            for (int i = 0; i < modules.Length; i++) {
                for (int j = i; j < modules.Length; j++) {
                    Module moduleI = modules[i];
                    Module moduleJ = modules[j];
                    if (moduleI == moduleJ) continue;

                    if (moduleI.location.StartsWith(moduleJ.location, StringComparison.Ordinal)) {
                        LogDiagnosticError("Nested Modules are not yet supported. " +
                                           $"{TypeNameGenerator.GetTypeName(moduleI.GetType())} is a parent of " +
                                           $"{TypeNameGenerator.GetTypeName(moduleJ.GetType())}. ({moduleJ.location})");
                        continue;
                    }

                    if (moduleJ.location.StartsWith(moduleI.location, StringComparison.Ordinal)) {
                        LogDiagnosticError("Nested Modules are not yet supported. " +
                                           $"{TypeNameGenerator.GetTypeName(moduleJ.GetType())} is a parent of " +
                                           $"{TypeNameGenerator.GetTypeName(moduleI.GetType())}. ({moduleI.location})");
                    }
                }
            }
        }

        private static bool Visit(Module module, LightStack<Module> stack, LightList<Module> sorted) {

            if (sorted.Contains(module)) {
                return true;
            }

            if (stack.Contains(module)) {

                string error = StringUtil.ListToString(stack.array.Select(m => m.GetType().GetTypeName()).ToArray(), " -> ");

                LogDiagnosticError($"Cyclic dependency found while loading modules: {error}");
                return false;
            }

            stack.Push(module);

            IList<ModuleReference> dependencies = module.dependencies;

            for (int i = 0; i < dependencies.Count; i++) {
                if (!Visit(dependencies[i].GetModuleInstance(), stack, sorted)) {
                    return false;
                }
            }

            Assert.AreEqual(module, stack.Peek());
            sorted.Add(stack.Pop());
            return true;
        }

        private static Module GetModuleInstance(Type moduleType) {
            for (int i = 0; i < modules.Length; i++) {
                if (modules[i].type == moduleType) {
                    return modules[i];
                }
            }

            return null;
        }

        private static void ValidateModuleDependencies() {

            Dictionary<string, Type> stringHash = new Dictionary<string, Type>();
            HashSet<Type> typeHash = new HashSet<Type>();

            for (int m = 0; m < modules.Length; m++) {
                Module instance = modules[m];

                stringHash.Clear();
                typeHash.Clear();

                IList<ModuleReference> dependencies = instance.dependencies;

                for (int j = 0; j < dependencies.Count; j++) {
                    dependencies[j].ResolveModule(GetModuleInstance(dependencies[j].GetModuleType()));
                }

                if (instance.GetType() != typeof(BuiltInElementsModule)) {
                    bool found = false;
                    for (int i = 0; i < dependencies.Count; i++) {
                        if (dependencies[i].GetModuleType() == typeof(BuiltInElementsModule)) {
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        dependencies.Add(new ModuleReference(typeof(BuiltInElementsModule)));
                        dependencies[dependencies.Count - 1].ResolveModule(GetModuleInstance(typeof(BuiltInElementsModule)));
                    }
                }

                for (int i = 0; i < dependencies.Count; i++) {
                    Type dependencyType = dependencies[i].GetModuleType();
                    string alias = dependencies[i].GetAlias();

                    if (stringHash.TryGetValue(alias, out Type otherModule)) {
                        LogDiagnosticError($"Duplicate alias or module name found in module {instance.GetType().GetTypeName()}. Both {dependencyType.GetTypeName()} and {otherModule.GetTypeName()} are registered as {alias}");
                        break;
                    }

                    if (typeHash.Contains(dependencyType)) {
                        LogDiagnosticError($"Duplicate dependency of type {TypeNameGenerator.GetTypeName(dependencyType)} in module {instance.GetType().GetTypeName()}");
                        break;
                    }

                    stringHash.Add(alias, dependencyType);
                    typeHash.Add(dependencyType);
                }
            }

            if (FailedToLoad) {
                return;
            }

            LightList<Module> sorted = new LightList<Module>(modules.Length);

            LightStack<Module> stack = new LightStack<Module>();

            Visit(modules[0], stack, sorted);

        }

        public static Module GetModule<T>() {
            return GetModuleInstance(typeof(T));
        }

        public static IList<Module> GetModuleList() {
            return new List<Module>(modules);
        }

        public static IList<ProcessedType> GetTemplateElements() {
            return TypeProcessor.GetTemplateElements();
        }

        private static Module GetModuleFromEntryPointType(Type entryType) {
            // todo -- if not element or abstract fail
            ProcessedType type = TypeProcessor.GetProcessedType(entryType);

            return type.module;
        }

        private static readonly SourceCache sourceCache = new SourceCache();

        internal static ProcessedType[] ParseTemplates(Type entryType, IList<Type> dynamicTypes = null) {
            if (!typeof(UIElement).IsAssignableFrom(entryType)) {
                return null;
            }

            List<Module> modulesToCompile = new List<Module>() {
                GetModuleFromEntryPointType(entryType)
            };

            if (dynamicTypes != null) {
                for (int i = 0; i < dynamicTypes.Count; i++) {
                    modulesToCompile.Add(GetModuleFromEntryPointType(dynamicTypes[i]));
                }
            }

            // maybe keep this flattened list on each module already?
            // then just join them all 
            Module[] list = FlattenDependencyTree(modulesToCompile);

            LightList<TemplateShell_Deprecated> parseList = new LightList<TemplateShell_Deprecated>();

            for (int i = 0; i < list.Length; i++) {
                Module module = list[i];

                // ReadOnlySizedArray<TemplateShell> sources = module.GetTemplateShells();
                //
                // for (int j = 0; j < sources.size; j++) {
                //
                //     TemplateShell source = sources.array[j];
                //
                //     sourceCache.Ensure(source.filePath);
                //
                //     parseList.Add(source);
                //
                // }

            }

            Parse(parseList);

            // ParseStyles(templateTypes);

            return GatherTypesToCompile(entryType);

        }

        private static ProcessedType[] GatherTypesToCompile(Type entryType) {

            ProcessedType entry = TypeProcessor.GetProcessedType(entryType);

            HashSet<ProcessedType> toCompile = new HashSet<ProcessedType>();
            HashSet<ProcessedType> searched = new HashSet<ProcessedType>();

            LightStack<TemplateNode> stack = new LightStack<TemplateNode>();
            LightStack<ProcessedType> toSearchStack = new LightStack<ProcessedType>();

            toCompile.Add(entry);

            toSearchStack.Push(entry);

            while (toSearchStack.size != 0) {

                ProcessedType checkType = toSearchStack.Pop();

                stack.Push(checkType.templateRootNode);

                while (stack.size != 0) {
                    TemplateNode current = stack.Pop();

                    // dont want to add unresolved generics but do want to search them

                    if (current.processedType != null && current.processedType.DeclaresTemplate) {

                        toCompile.Add(current.processedType);

                        if (searched.Add(current.processedType)) {

                            toSearchStack.Push(current.processedType);

                        }

                    }

                    for (int i = 0; i < current.children.size; i++) {
                        stack.Push(current.children.array[i]);
                    }

                }

            }

            return toCompile.ToArray();
        }

        private struct ParseJob : IJobParallelFor, IJob {

            public GCHandle handle;

            public void Execute(int i) {

                LightList<TemplateShell_Deprecated> parseList = (LightList<TemplateShell_Deprecated>) handle.Target;

                TemplateShell_Deprecated templateShell = parseList.array[i];

                FileInfo fileInfo = sourceCache.Get(parseList.array[i].filePath);

                if (fileInfo.missing) {
                    Debug.Log($"Cannot resolve file {fileInfo} referenced from {parseList.array[i].module.type.GetTypeName()}");
                    return;
                }

                if (templateShell.lastParseVersion == fileInfo.lastWriteTime) {
                    return; // todo -- still want to validate probably
                }

                templateShell.Reset();

                TemplateParser_Deprecated parserDeprecated = TemplateParser_Deprecated.GetParserForFileType("xml");

                parserDeprecated.OnSetup();

                if (parserDeprecated.TryParse(fileInfo.contents, templateShell)) {
                    templateShell.lastParseVersion = fileInfo.lastWriteTime;
                }
                else {
                    templateShell.lastParseVersion = default;
                }

                parserDeprecated.OnReset();

                TemplateValidator.Validate(templateShell); // separate job?

            }

            public void Execute() {

                LightList<TemplateShell_Deprecated> parseList = (LightList<TemplateShell_Deprecated>) handle.Target;

                for (int i = 0; i < parseList.size; i++) {
                    Execute(i);
                }

            }

        }

        private static void Parse(LightList<TemplateShell_Deprecated> parseList) {

            sourceCache.FlushPendingUpdates();

            ParseJob parseJob = new ParseJob() {
                handle = GCHandle.Alloc(parseList)
            };

            // note: there is a somewhat high startup cost to running this job for the first time
            // for this job in particular we get a ~2x speed up with parallel (without caching of parse results)
            // warmup time with parallel is ~30ms for 1 use case, then runs are ~0.5ms. just calling Run()
            // on main thread is around 1ms with bad xml parser, probably half of that with a faster one

            parseJob.Run();
            // JobHandle handle = parseJob.Schedule(parseList.size, 1);
            // handle.Complete();

            parseJob.handle.Free();
        }

        private static Module[] FlattenDependencyTree(IList<Module> modules) {
            HashSet<Module> set = new HashSet<Module>();

            LightStack<Module> stack = new LightStack<Module>();

            for (int i = 0; i < modules.Count; i++) {
                stack.Push(modules[i]);
            }

            while (stack.size != 0) {
                Module m = stack.Pop();

                set.Add(m);

                for (int i = 0; i < m.dependencies.Count; i++) {
                    stack.Push(m.dependencies[i].GetModuleInstance());
                }

            }

            return set.ToArray();

        }

    }

}