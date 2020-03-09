using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Style2;
using UIForia.Util;

namespace UIForia {

    public class ModuleLoadException : System.Exception {

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
        private Uri location;

        private static readonly HashSet<Assembly> s_Assemblies = new HashSet<Assembly>();
        private static readonly Dictionary<Type, DiscoveredModule> s_DiscoveryMap = new Dictionary<Type, DiscoveredModule>();
        private static readonly Dictionary<Type, Module> s_ModuleInstances = new Dictionary<Type, Module>();

        private static readonly HashSet<string> s_StringHashSet = new HashSet<string>();
        private static readonly HashSet<Type> s_TypeHashSet = new HashSet<Type>();

        private static bool s_ConstructionAllowed;

        protected Module() {
            if (!s_ConstructionAllowed) {
                throw new ModuleLoadException("Modules should never have their constructor called.");
            }

            this.dependencies = new List<ModuleReference>();
            this.dynamicTypeReferences = new List<Type>();
            this.visitedMark = VisitMark.Alive;
        }

        internal Action zz__INTERNAL_DO_NOT_CALL; // use this for precompiled loading instead of doing type reflection to find caller type

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
            if (GetType() != typeof(UIForiaElements)) {
                bool found = false;
                for (int i = 0; i < dependencies.Count; i++) {
                    if (dependencies[i].GetModuleType() == typeof(UIForiaElements)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    dependencies.Add(new ModuleReference(typeof(UIForiaElements)));
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

        public static T CreateRootModule<T>() where T : Module {
            return CreateRootModule(typeof(T)) as T;
        }

        private static Module GetModuleInstance(Type moduleType) {
            if (!s_ModuleInstances.TryGetValue(moduleType, out Module instance)) {
                s_ConstructionAllowed = true;
                Uri location = new Uri(GetFilePathFromAttribute(moduleType));
                instance = (Module) Activator.CreateInstance(moduleType);
                instance.location = location;
                s_ModuleInstances[moduleType] = instance;
                s_ConstructionAllowed = false;
            }

            return instance;
        }

        internal static Module CreateRootModule(Type moduleType) {
            if (moduleType == null) {
                throw new ModuleLoadException("Module type was null.");
            }

            if (moduleType.IsAbstract) {
                throw new ModuleLoadException($"Module types cannot be abstract. {TypeNameGenerator.GetTypeName(moduleType)} is abstract!");
            }

            Module root = GetModuleInstance(moduleType);

            GatherDependencies(root);

            List<Module> dependencySort = DependencySort(root);

            TypeProcessor.Initialize(dependencySort);

            for (int i = 0; i < dependencySort.Count; i++) {
                if (!dependencySort[i].initialized) {
                    dependencySort[i].Configure();
                    dependencySort[i].initialized = true;
                }
            }

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

                    if (module.moduleLocation.IsBaseOf(discoveredModule.moduleLocation)) {
                        throw new ModuleLoadException("Nested Modules are not yet supported." +
                                                      $"{TypeNameGenerator.GetTypeName(module.moduleType)} is a parent of " +
                                                      $"{TypeNameGenerator.GetTypeName(discoveredModule.moduleType)}. ({discoveredModule.moduleLocation})");
                    }

                    if (discoveredModule.moduleLocation.IsBaseOf(module.moduleLocation)) {
                        throw new ModuleLoadException("Nested Modules are not yet supported." +
                                                      $"{TypeNameGenerator.GetTypeName(discoveredModule.moduleType)} is a parent of " +
                                                      $"{TypeNameGenerator.GetTypeName(module.moduleType)}. ({module.moduleLocation})");
                    }

                    if (module.parentLocation == discoveredModule.parentLocation) {
                        throw new ModuleLoadException("Modules cannot be siblings. " +
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
            if (!type.IsSubclassOf(typeof(UIElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is not a subclass of {TypeNameGenerator.GetTypeName(typeof(UIElement))}.");
            }

            if (type.IsAbstract) {
                throw new ModuleLoadException($"Cannot create a module for a type that is abstract. Tried to create module for {TypeNameGenerator.GetTypeName(typeof(UIElement))} but it was abstract.");
            }

            if (type.IsSubclassOf(typeof(UITextElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is a subclass of {TypeNameGenerator.GetTypeName(typeof(UITextElement))}. Tried to create module for {TypeNameGenerator.GetTypeName(type)}");
            }

            if (type.IsSubclassOf(typeof(UIContainerElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is a subclass of {TypeNameGenerator.GetTypeName(typeof(UIContainerElement))}. Tried to create module for {TypeNameGenerator.GetTypeName(type)}");
            }

            if (type.IsSubclassOf(typeof(UIImageElement))) {
                throw new ModuleLoadException($"Cannot create a module for a type that is a subclass of {TypeNameGenerator.GetTypeName(typeof(UIImageElement))}.Tried to create module for {TypeNameGenerator.GetTypeName(type)}");
            }

            TemplateAttribute attribute = type.GetCustomAttribute<TemplateAttribute>();

            if (attribute == null) {
                throw new ModuleLoadException($"Can only create an application when the root element is annotated with [{nameof(TemplateAttribute)}]. {TypeNameGenerator.GetTypeName(type)} is missing one.");
            }

            Uri locationUri = new Uri(attribute.elementPath);

            ProcessAssembly(type.Assembly);

            foreach (KeyValuePair<Type, DiscoveredModule> kvp in s_DiscoveryMap) {
                if (kvp.Value.moduleLocation.IsBaseOf(locationUri)) {
                    return kvp.Value.moduleType;
                }
            }

            return null;
        }

        private static string GetFilePathFromAttribute(Type moduleType) {
            RecordFilePathAttribute attr = moduleType.GetCustomAttribute<RecordFilePathAttribute>();

            if (attr == null) {
                throw new ModuleLoadException($"Modules must provide a [{TypeNameGenerator.GetTypeName(typeof(RecordFilePathAttribute))}] attribute. {TypeNameGenerator.GetTypeName(moduleType)} is missing one.");
            }

            return attr.filePath;
        }

    }

}