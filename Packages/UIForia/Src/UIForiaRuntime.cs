using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

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

    public static class UIForiaRuntime {

        private static bool _failedToLoad;

        public static bool FailedToLoad {
            get => _failedToLoad;
            internal set {
                IsLoading = false;
                _failedToLoad = value;
            }
        }

        public static bool IsLoading { get; internal set; }
        public static UIModule BuiltInModule { get; private set; }

        internal static bool s_ConstructionAllowed;

        private static UIModule[] modules;
        private static List<DiagnosticEntry> diagnosticLog;

        public static void Initialize() {

            diagnosticLog = new List<DiagnosticEntry>();
            TypeResolver.Stats stats = TypeResolver.Initialize();
            TypeScanner.Stats scanStats = TypeScanner.Scan();

            Debug.Log($"Loaded {stats.typeCount} types from {stats.namespaceCount} namespaces from {stats.assemblyCount} assemblies in {stats.namespaceScanTime:F2}ms");
            Debug.Log($"Scanned for UIForia types in {scanStats.totalScanTime:F2}ms");

            InitializeModules();

            if (FailedToLoad) {
                return;
            }

            ValidateModulePaths();
            if (FailedToLoad) {
                return;
            }

            ValidateModuleDependencies();
            if (FailedToLoad) {
                return;
            }

            AssignElementsToModules();
            if (FailedToLoad) {
                return;
            }

            IsLoading = false;

        }

        private static void ValidateModulePaths() {

            for (int i = 0; i < modules.Length; i++) {
                for (int j = i; j < modules.Length; j++) {
                    UIModule moduleI = modules[i];
                    UIModule moduleJ = modules[j];
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

        private static void ValidateModuleDependencies() {

            Dictionary<string, Type> stringHash = new Dictionary<string, Type>();
            HashSet<Type> typeHash = new HashSet<Type>();

            for (int m = 0; m < modules.Length; m++) {
                UIModule instance = modules[m];

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

            LightList<UIModule> sorted = new LightList<UIModule>(modules.Length);

            LightStack<UIModule> stack = new LightStack<UIModule>();

            Visit(modules[0], stack, sorted);

        }

        private static bool Visit(UIModule module, LightStack<UIModule> stack, LightList<UIModule> sorted) {

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

        // cannot be mulithreaded without significant work
        private static void AssignElementsToModules() {

            IList<Type> elements = TypeScanner.elementTypes;

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

            // stats.elementCount = elements.Count;
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
                UIModule module = modules[i];

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

                // todo -- need to get template sources somehow
                
                // if (!s_TemplateShells.TryGetValue(templateLocation, out TemplateFileShell shell)) {
                //     shell = new TemplateFileShell(templateLocation);
                //     module.templateShells.Add(shell);
                //     processedType.templateFileShell = shell;
                //     shell.module = module;
                //     s_TemplateShells.Add(templateLocation, shell);
                // }
                // else {
                //     processedType.templateFileShell = shell;
                // }

                return;
            }

            LogDiagnosticError($"Type {TypeNameGenerator.GetTypeName(processedType.rawType)} at {processedType.elementPath} was not inside a module hierarchy.");

        }

        private static UIModule GetModuleInstance(Type moduleType) {
            for (int i = 0; i < modules.Length; i++) {
                if (modules[i].type == moduleType) {
                    return modules[i];
                }
            }

            return null;
        }

        internal static void LogDiagnosticInfo(string message) {
            diagnosticLog.Add(new DiagnosticEntry() {
                message = message,
                diagnosticType = DiagnosticType.Info
            });
            Debug.Log(message);
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

        private static void InitializeModules() {

            IList<Type> moduleTypes = TypeScanner.moduleTypes;

            modules = new UIModule[moduleTypes.Count];

            for (int i = 0; i < moduleTypes.Count; i++) {
                s_ConstructionAllowed = true;
                UIModule instance = null;

                if (moduleTypes[i].IsAbstract) {
                    FailedToLoad = true;
                    LogDiagnosticInfo("Modules cannot be abstract but found " + moduleTypes[i].GetTypeName() + " which was declared as abstract");
                    continue;
                }

                try {
                    instance = (UIModule) Activator.CreateInstance(moduleTypes[i]);
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
        }

    }

}