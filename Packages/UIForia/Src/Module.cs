using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Style2;
using UIForia.Util;

namespace UIForia {

    public abstract class Module {

        private bool dependenciesResolved;
        private LightList<StyleSheet2> styleSheets;
        private ModuleReference[] moduleReferences;
        private IList<ModuleReference> cachedDependencies;

        private List<bool> conditionResults;
        private IList<StyleCondition> styleConditions;
        private VisitMark visitedMark;

        private string path;
        private string moduleName;
        private Type defaultRootType;
        private IList<ModuleReference> dependencies;
        private IList<Type> dynamicTypeReferences;
        private Uri location;

        private static readonly HashSet<Assembly> s_Assemblies = new HashSet<Assembly>();
        private static readonly Dictionary<Type, DiscoveredModule> s_DiscoveryMap = new Dictionary<Type, DiscoveredModule>();
        private static readonly Dictionary<Type, Module> s_ModuleInstances = new Dictionary<Type, Module>();

        private static bool s_ConstructionAllowed;

        public Module() {
            if (!s_ConstructionAllowed) {
                throw new Exception("Modules should never have their constructor called.");
            }

            this.dependencies = new List<ModuleReference>();
            this.dynamicTypeReferences = new List<Type>();
            this.visitedMark = VisitMark.Alive;
            this.location = new Uri(GetFilePathFromAttribute(GetType()));

        }

        public Action zz__INTERNAL_DO_NOT_CALL; // use this for precompiled loading instead of doing type reflection to find caller type

        public abstract void Initialize();

        protected void SetDefaultRootType<TElementType>() where TElementType : UIElement {
            defaultRootType = typeof(TElementType);
        }

        protected void SetModuleName(string moduleName) {
            this.moduleName = moduleName;
        }

        protected void SetFilePath(string filePath) {
            Type type = GetType();
            if (!type.Assembly.Location.Contains(UnityEngine.Application.dataPath)) { }

            path = filePath;

        }

        protected void UseDefaultFilePath([CallerFilePath] string path = "") {
            // todo -- validate path
            SetFilePath(path);
        }

        protected void AddDependency<TDependency>(string alias) where TDependency : Module, new() {

            if (typeof(TDependency).IsAbstract) {
                throw new InvalidArgumentException("Dependencies must be concrete classes. " + TypeNameGenerator.GetTypeName(typeof(TDependency)) + " is abstract");
            }

            dependencies.Add(new ModuleReference(typeof(TDependency), alias));
        }

        private void ValidateDependencies(HashSet<string> stringHash, HashSet<Type> typeHash) {

            for (int i = 0; i < dependencies.Count; i++) {
                Type dependencyType = dependencies[i].GetModuleType();
                string alias = dependencies[i].GetAlias();

                if (stringHash.Contains(alias)) {
                    throw new Exception($"Duplicate alias or module name " + alias);
                }

                if (typeHash.Contains(dependencyType)) {
                    throw new Exception($"Duplicate dependency of type {TypeNameGenerator.GetTypeName(dependencyType)} in module {moduleName}");
                }

                stringHash.Add(alias);
                typeHash.Add(dependencyType);

            }
        }

        protected void AddDynamicTypeReference<TReference>() where TReference : UIElement {
            dynamicTypeReferences.Add(typeof(TReference));
        }

        public virtual void BuildCustomStyles(IStyleCodeGenerator generator) { }

        public static Module CreateRootModule<T>() where T : Module {

            Module root = Activator.CreateInstance<T>();

            GatherDependencies(root);

            List<Module> dependencySort = DependencySort(root);

            TypeProcessor.Initialize(dependencySort);

            return root;

        }

        internal static Module CreateRootModule(Type moduleType) {

            if (moduleType.IsAbstract) {
                throw new Exception($"Module types cannot be abstract. {TypeNameGenerator.GetTypeName(moduleType)} is abstract!");
            }

            Module root = (Module) Activator.CreateInstance(moduleType);

            GatherDependencies(root);

            List<Module> dependencySort = DependencySort(root);

            TypeProcessor.Initialize(dependencySort);

            return root;

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
                    throw new Exception("Duplicate DisplayCondition '" + condition + "'");
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

            m.dependenciesResolved = true;

            IList<ModuleReference> dependencies = m.GetCachedDependencies();

            for (int i = 0; i < dependencies.Count; i++) {

                Type moduleType = dependencies[i].GetModuleType();

                if (!s_ModuleInstances.TryGetValue(moduleType, out Module instance)) {
                    instance = (Module) Activator.CreateInstance(moduleType);
                    s_ModuleInstances[moduleType] = instance;
                }

                dependencies[i].ResolveModule(instance);

                if (!instance.dependenciesResolved) {
                    GatherDependencies(instance);
                }

            }

        }

        private static List<Module> DependencySort(Module root) {

            int count = CountSizeAndReset(root, 1);

            List<Module> sorted = new List<Module>(count);

            Visit(root, sorted);

            return sorted;
        }

        private static int CountSizeAndReset(Module module, int count) {
            module.visitedMark = VisitMark.Alive;

            IList<ModuleReference> dependencies = module.GetCachedDependencies();

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
                throw new ArgumentException("Cyclic dependency found."); // probably need a stack to track this properly
            }

            module.visitedMark = VisitMark.Undead;

            IList<ModuleReference> dependencies = module.GetCachedDependencies();

            for (int i = 0; i < dependencies.Count; i++) {
                Visit(dependencies[i].GetModuleInstance(), sorted);
            }

            module.visitedMark = VisitMark.Undead;

            sorted.Add(module);
        }

        private IList<ModuleReference> GetCachedDependencies() {
            if (cachedDependencies == null) {
                // cachedDependencies = GetDependencies();

                if (cachedDependencies == null) {
                    cachedDependencies = new ModuleReference[0];
                }

                for (int i = 0; i < cachedDependencies.Count; i++) {
                    if (cachedDependencies[i].GetModuleType().IsAbstract) {
                        throw new InvalidArgumentException("Dependency declared on an abstract type " + TypeNameGenerator.GetTypeName(cachedDependencies[i].GetModuleType()));
                    }
                }

                if (cachedDependencies.Count >= 2) {
                    // todo -- pool
                    List<Type> references = new List<Type>(cachedDependencies.Count);

                    for (int i = 0; i < cachedDependencies.Count; i++) {

                        if (references.Contains(cachedDependencies[i].GetModuleType())) {
                            throw new InvalidArgumentException($"Duplicate dependency declared on {TypeNameGenerator.GetTypeName(cachedDependencies[i].GetModuleType())}");
                        }

                        references.Add(cachedDependencies[i].GetModuleType());

                    }
                }

            }

            return cachedDependencies;

        }

        private struct DiscoveredModule {

            public readonly Type moduleType;
            public readonly Uri moduleLocation;
            public readonly Uri parentLocation;

            public DiscoveredModule(Type moduleType, Uri moduleLocation) {
                this.moduleType = moduleType;
                this.moduleLocation = moduleLocation;
                this.parentLocation = moduleLocation.Parent();
            }

        }

        internal static void ProcessAssembly(Assembly assembly) {

            if (assembly.IsDynamic || s_Assemblies.Contains(assembly)) {
                return;
            }

            s_Assemblies.Add(assembly);

            Type[] types = assembly.GetExportedTypes();

            StructList<DiscoveredModule> modules = StructList<DiscoveredModule>.Get();

            for (int i = 0; i < types.Length; i++) {
                Type currentType = types[i];
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (currentType == null || !currentType.IsClass || !currentType.IsSubclassOf(typeof(Module))) {
                    continue;
                }

                DiscoveredModule discoveredModule = new DiscoveredModule(currentType, new Uri(GetFilePathFromAttribute(currentType)));
                for (int j = 0; j < modules.size; j++) {
                    DiscoveredModule module = modules[j];
                    if (module.moduleLocation.IsBaseOf(discoveredModule.moduleLocation)) { }

                    if (module.parentLocation == discoveredModule.parentLocation) {
                        throw new Exception("Modules cannot be siblings. " +
                                            $"{TypeNameGenerator.GetTypeName(module.moduleType)} is at the same location as " +
                                            $"{TypeNameGenerator.GetTypeName(discoveredModule.moduleType)}. ({discoveredModule.parentLocation})");
                    }
                }

                s_DiscoveryMap.Add(currentType, discoveredModule);
                
                modules.Add(discoveredModule);
            }

            modules.Release();
        }

        internal static Type GetModuleFromElementType(Type type) {

            TemplateAttribute attribute = type.GetCustomAttribute<TemplateAttribute>();

            if (attribute == null) {
                throw new Exception($"Can only create an application when the root element is annotated with [{nameof(TemplateAttribute)}]. {TypeNameGenerator.GetTypeName(type)} is missing one.");
            }

            Uri locationUri = new Uri(attribute.elementPath);

            ProcessAssembly(type.Assembly);

            foreach (KeyValuePair<Type, DiscoveredModule> kvp in s_DiscoveryMap) {
                if (kvp.Value.moduleLocation.IsBaseOf(locationUri)) {
                    return kvp.Value.moduleType;
                }
            }

            // foreach (KeyValuePair<Type, Module> kvp in s_ModuleInstances) {
            //     Module module = kvp.Value;
            //     if (module.location.IsBaseOf(locationUri)) {
            //         return module;
            //     }
            // }
            //
            // Type[] types = type.Assembly.GetExportedTypes();
            //
            // for (int i = 0; i < types.Length; i++) {
            //     Type currentType = types[i];
            //     // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            //     if (currentType == null || !currentType.IsClass || !currentType.IsSubclassOf(typeof(Module))) {
            //         continue;
            //     }
            //
            //     Uri moduleUri = new Uri(GetFilePathFromAttribute(currentType));
            //
            //     if (moduleUri.IsBaseOf(locationUri)) {
            //         return CreateRootModule(currentType);
            //     }
            //
            // }

            return null;

        }

        private static string GetFilePathFromAttribute(Type moduleType) {

            RecordFilePathAttribute attr = moduleType.GetCustomAttribute<RecordFilePathAttribute>();

            if (attr == null) {
                throw new Exception($"Modules must provide a [{nameof(RecordFilePathAttribute)}] attribute. {TypeNameGenerator.GetTypeName(moduleType)} is missing one.");
            }

            return attr.filePath;
        }

    }

}