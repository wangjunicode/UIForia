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
using UIForia.Parsing;
using UIForia.Util;
using Unity.Jobs;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace UIForia.Src {

    public static class ModuleSystem {

        private static readonly Module[] modules;
        private static readonly HashSet<ResolvedTemplateLocation> s_TemplateSet;

        internal static readonly bool s_ConstructionAllowed;

        private static List<Diagnostic> diagnostics;
        private static readonly object diagnosticLock = new object();
        private static TemplateCache s_TemplateCache;

        public static Module BuiltInModule { get; private set; }

        static ModuleSystem() {
            s_TemplateSet = new HashSet<ResolvedTemplateLocation>();
            IList<Type> moduleTypes = TypeProcessor.GetModuleTypes();
            modules = new Module[moduleTypes.Count];

            for (int i = 0; i < moduleTypes.Count; i++) {
                s_ConstructionAllowed = true;
                Module instance = (Module) Activator.CreateInstance(moduleTypes[i]);
                s_ConstructionAllowed = false;
                string moduleLocation = GetFilePathFromAttribute(moduleTypes[i]);
                instance.location = Path.GetDirectoryName(moduleLocation) + Path.DirectorySeparatorChar;
                modules[i] = instance;

                if (instance.IsBuiltIn) {
                    BuiltInModule = instance;
                }

                try {
                    instance.Configure();
                }
                catch (Exception e) {
                    Debug.LogError(e);
                }
            }

            ValidateModulePaths();

            ValidateModuleDependencies();

            AssignElementsToModules();

        }

        private static void AssignElementsToModules() {

            Stopwatch sw = Stopwatch.StartNew();

            IList<Type> elements = TypeProcessor.GetElementTypes();

            for (int i = 0; i < elements.Count; i++) {
                Type currentType = elements[i];

                if (currentType.IsAbstract) continue;

                ProcessedType processedType = ProcessedType.CreateFromType(currentType);
                TypeProcessor.typeMap[processedType.rawType] = processedType;

                if (string.IsNullOrEmpty(processedType.elementPath)) {
                    if (processedType.rawType.Assembly != typeof(UIElement).Assembly) {
                        Debug.LogError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} requires a location providing attribute." +
                                       $" Please use [{nameof(RecordFilePathAttribute)}], [{nameof(TemplateAttribute)}], " +
                                       $"[{nameof(ImportStyleSheetAttribute)}], [{nameof(StyleAttribute)}]" +
                                       $" or [{nameof(TemplateTagNameAttribute)}] on the class. If you intend not to provide a template you can also use [{nameof(ContainerElementAttribute)}].");
                        continue;
                    }
                }

                if (!TryAssignModule(processedType)) {
                    Debug.LogError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} at {processedType.elementPath} was not inside a module hierarchy.");
                }

            }

            sw.Stop();
//            Debug.Log($"Assigned elements to modules in {sw.Elapsed.TotalMilliseconds:F3}ms with {elements.Count} element types");
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

        private static bool TryAssignModule(ProcessedType processedType) {
            string path = processedType.elementPath;

            if (processedType.module != null) {
                return true;
            }

            if (string.IsNullOrEmpty(path)) {
                return false;
            }

            for (int i = 0; i < modules.Length; i++) {
                Module module = modules[i];
                if (path.StartsWith(module.location, StringComparison.Ordinal)) {
                    
                    module.AddElementType(processedType);

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

                    processedType.module = module;

                    // todo -- maybe run this as part of template gather in case something changed

                    if (!processedType.IsContainerElement && !processedType.rawType.IsAbstract) {
                        processedType.resolvedTemplateLocation = module.ResolveTemplatePath(new TemplateLookup(processedType));
                        if (processedType.resolvedTemplateLocation == null) {
                            Debug.LogError($"Unable to locate template for {TypeNameGenerator.GetTypeName(processedType.rawType)}.");
                        }
                        else {
                            s_TemplateSet.Add(new ResolvedTemplateLocation(module, processedType.resolvedTemplateLocation.Value.filePath));
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private static string GetFilePathFromAttribute(Type moduleType) {
            RecordFilePathAttribute attr = moduleType.GetCustomAttribute<RecordFilePathAttribute>();

            if (attr == null) {
                throw new ModuleLoadException($"Modules must provide a [{TypeNameGenerator.GetTypeName(typeof(RecordFilePathAttribute))}] attribute. {TypeNameGenerator.GetTypeName(moduleType)} is missing one.");
            }

            return attr.filePath;
        }

        private static void ValidateModulePaths() {

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

        internal static void ReportParseError(string file, string message, int lineNumber, int col = -1) {
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

        private static void Visit(Module module, LightStack<Module> stack, LightList<Module> sorted) {


            if (sorted.Contains(module)) {
                return;
            }

            if (stack.Contains(module)) {

                string error = StringUtil.ListToString(stack.array.Select(m => m.GetType().GetTypeName()).ToArray(), " -> ");

                throw new ModuleLoadException($"Cyclic dependency found while loading modules: {error}");
            }

            stack.Push(module);

            IList<ModuleReference> dependencies = module.dependencies;

            for (int i = 0; i < dependencies.Count; i++) {
                Visit(dependencies[i].GetModuleInstance(), stack, sorted);
            }

            Assert.AreEqual(module, stack.Peek());
            sorted.Add(stack.Pop());

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

            HashSet<string> stringHash = new HashSet<string>();
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

                    if (stringHash.Contains(alias)) {
                        throw new ModuleLoadException($"Duplicate alias or module name {alias}");
                    }

                    if (typeHash.Contains(dependencyType)) {
                        throw new ModuleLoadException($"Duplicate dependency of type {TypeNameGenerator.GetTypeName(dependencyType)} in module {instance.GetType().GetTypeName()}");
                    }

                    stringHash.Add(alias);
                    typeHash.Add(dependencyType);
                }
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

            Module[] list = FlattenDependencyTree(modulesToCompile);

            LightList<TemplateShell> parseList = new LightList<TemplateShell>();

            for (int i = 0; i < list.Length; i++) {
                Module module = list[i];

                ReadOnlySizedArray<TemplateShell> sources = module.GetTemplateShells();

                for (int j = 0; j < sources.size; j++) {

                    TemplateShell source = sources.array[j];

                    sourceCache.Ensure(source.filePath);

                    parseList.Add(source);

                }

            }

            Parse(parseList);

            MatchElementTypesToTemplateNodes(list);

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

        private static void MatchElementTypesToTemplateNodes(Module[] list) {

            for (int i = 0; i < list.Length; i++) {
                list[i].MatchElementTypesToTemplateNodes();
            }

        }

        private struct ParseJob : IJobParallelFor, IJob {

            public GCHandle handle;

            public void Execute(int i) {
                
                LightList<TemplateShell> parseList = (LightList<TemplateShell>) handle.Target;
                
                TemplateShell templateShell = parseList.array[i];

                FileInfo fileInfo = sourceCache.Get(parseList.array[i].filePath);

                if (fileInfo.missing) {
                    Debug.Log($"Cannot resolve file {fileInfo} referenced from {parseList.array[i].module.type.GetTypeName()}");
                    return;
                }

                if (templateShell.lastParseVersion == fileInfo.lastWriteTime) {
                    return; // todo -- still want to validate probably
                }

                templateShell.Reset();

                TemplateParser parser = TemplateParser.GetParserForFileType("xml");

                parser.OnSetup();

                if (parser.TryParse(fileInfo.contents, templateShell)) {
                    templateShell.lastParseVersion = fileInfo.lastWriteTime;
                }
                else {
                    templateShell.lastParseVersion = default;
                }

                parser.OnReset();

                TemplateValidator.Validate(templateShell); // separate job?

            }

            public void Execute() {

                LightList<TemplateShell> parseList = (LightList<TemplateShell>) handle.Target;

                for (int i = 0; i < parseList.size; i++) {
                    Execute(i);
                }

            }

        }

        private static void Parse(LightList<TemplateShell> parseList) {
            
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