using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Templates;
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

        static ModuleSystem() {
            TypeProcessor.Initialize();
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
            
            Parse();
            Parse();
        }

        public static Module BuiltInModule { get; private set; }

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
            Debug.Log($"Assigned elements to modules in {sw.Elapsed.TotalMilliseconds:F3}ms with {elements.Count} element types");
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

            // for (int i = 0; i < dependencySort.Count; i++) {
            //     Module module = dependencySort[i];
            //
            //     // IEnumerable<string> templateFiles = Directory.EnumerateFiles(module.location, "*.xml", SearchOption.AllDirectories);
            //     IEnumerable<string> styleFiles = Directory.EnumerateFiles(module.location, "*.style", SearchOption.AllDirectories);
            //
            //     // foreach (string file in templateFiles) {
            //     //     module.AddTemplateFile(file);
            //     // }
            //
            //     foreach (string file in styleFiles) {
            //         //  module.AddStyleFile(file);
            //     }
            // }


            return rootModule;
        }

        internal static class TemplateValidator {

            public static bool Validate(TemplateShell templateShell) {
                
                // threading?
                LightStack<TemplateNode> stack = LightStack<TemplateNode>.Get();

                Module module = templateShell.module;
                
                for (int i = 0; i < templateShell.templateRootNodes.size; i++) {
                    // try to resolve tag name. might fail due to generic. maybe here is when we can run resolution?
                    // validate tag can live on parent
                    
                    stack.Push(templateShell.templateRootNodes.array[i]);

                    while (stack.size != 0) {
                        TemplateNode node = stack.Pop();
                        
                        if (node.processedType == null) {

                            if (node is ElementNode elementNode) {
                                ProcessedType processedType = module.ResolveTagName(elementNode, templateShell);
                            }

                        }
                        
                    }
                    
                }
                
                stack.Release();

                return true;
            }

        }

        private static void Parse() {
            
            Stopwatch w = Stopwatch.StartNew();
            
            s_TemplateCache = new TemplateCache(s_TemplateSet, s_TemplateCache);

            // generating types needs to be sync anyway so we can probably just do a 2nd pass to collect generated types and handle their templates

            ParseTemplateJob job = new ParseTemplateJob(s_TemplateCache.cache);

            JobHandle handle = job.Schedule(s_TemplateCache.cache.Length, 1);

            handle.Complete();
            job.handle.Free();

            w.Stop();
            Debug.Log($"Read {s_TemplateSet.Count} files in {w.Elapsed.TotalMilliseconds:F3} ms");

            for (int i = 0; i < s_TemplateCache.cache.Length; i++) {
                ref TemplateCache.FileInfo fileInfo = ref s_TemplateCache.cache[i];
                
                TemplateValidator.Validate(fileInfo.templateShell);
                
                TemplateShell templateShell = s_TemplateCache.cache[i].templateShell;
                
                if (templateShell.styles != null) {
                    for (int s = 0; s < templateShell.styles.size; s++) {
                        StyleDefinition style = templateShell.styles[s];
                    }
                }
                
            }
            
            // w.Reset();
            // w.Start();
            //
            // List<TemplateParseInfo> parseInfos = new List<TemplateParseInfo>(128);
            //
            // for (int i = 0; i < modulesToParse.Count; i++) {
            //
            //     Module module = modulesToParse[i];
            //
            //     Dictionary<string, TemplateSource>.ValueCollection values = module.fileSources.Values;
            //
            //     foreach (TemplateSource value in values) {
            //         TemplateParseInfo parseInfo = new TemplateParseInfo() {
            //             module = module,
            //             source = value.source,
            //             path = value.path
            //         };
            //
            //         parseInfos.Add(parseInfo);
            //     }
            // }
            //
            // TemplateParseJob parseJob = new TemplateParseJob {
            //     handle = GCHandle.Alloc(parseInfos)
            // };
            //
            // //JobHandle x = parseJob.Schedule();
            // JobHandle x = parseJob.Schedule(parseInfos.Count, 1);
            // x.Complete();
            //
            // bool failedToParse = false;
            //
            // for (int i = 0; i < modulesToParse.Count; i++) {
            //
            //     List<Diagnostic> diagnostics = modulesToParse[i].diagnostics;
            //
            //     if (diagnostics != null) {
            //
            //         failedToParse = true;
            //         for (int j = 0; j < diagnostics.Count; j++) {
            //             Debug.LogError($"{diagnostics[j].filePath} at line {diagnostics[j].lineNumber}:{diagnostics[j].columnNumber}| {diagnostics[j].message}");
            //         }
            //
            //         diagnostics.Clear();
            //     }
            //
            //     Module module = modulesToParse[i];
            //
            //     // find range of parsed files for each module 
            //     // for each item in range
            //     // if null, do nothing
            //     //    
            //     // else
            //     //     search remaining list of processed types looking for template match
            //     // 
            //
            //     //     for (int j = 0; j < module.processedTypeList.size; j++) {
            //     //
            //     //         ProcessedType current = module.processedTypeList.array[j];
            //     //         if (current.IsContainerElement) {
            //     //             continue;
            //     //         }
            //     //
            //     //         if (current.rawType.IsSubclassOf(typeof(UITextElement))) {
            //     //             continue;
            //     //         }
            //     //
            //     //         if (current.templateRootNode == null) {
            //     //             // resolve template
            //     //             // module.GetTemplateForElement();
            //     //             // look through module's template paths
            //     //             // determine template path for module
            //     //             TemplateLocation? templatePath = module.ResolveTemplatePath(new TemplateLookup(current));
            //     //
            //     //             if (templatePath == null) {
            //     //                 continue;
            //     //             }
            //     //
            //     //             //
            //     //             // if (module.fileSources.TryGetValue(templatePath, out TemplateSource source)) {
            //     //             //     Debug.Log("found " + templatePath);
            //     //             // }
            //     //         }
            //     //
            //     //     }
            //     //
            // }
            //
            // Debug.Log($"Parsed {parseInfos.Count} files in {w.Elapsed.TotalMilliseconds:F3}ms");
            // w.Stop();
            // return !failedToParse;

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
                    module.elementTypes.Add(processedType);

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

    }

}