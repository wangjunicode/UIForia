using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia {
    
    public struct DisplayConfiguration {

        public readonly float dpi;
        public readonly float screenWidth;
        public readonly float screenHeight;
        public readonly ScreenOrientation screenOrientation;
        public readonly DeviceOrientation deviceOrientation;

        public DisplayConfiguration(float screenWidth, float screenHeight, float dpi, ScreenOrientation screenOrientation = ScreenOrientation.Landscape, DeviceOrientation deviceOrientation = DeviceOrientation.Unknown) {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.dpi = dpi;
            this.screenOrientation = screenOrientation;
            this.deviceOrientation = deviceOrientation;
        }

    }
    
    public struct StyleCondition {

        public readonly int id;
        public readonly string name;
        public readonly Func<DisplayConfiguration, bool> fn;

        public StyleCondition(int id, string name, Func<DisplayConfiguration, bool> fn) {
            this.id = id;
            this.name = name;
            this.fn = fn;
        }

    }
    public class ModuleReference {

        private UIModule module;
        private readonly string alias;
        private readonly Type moduleType;

        internal ModuleReference(Type moduleType, string alias = null) {
            this.moduleType = moduleType;
            this.alias = alias;
        }

        public Type GetModuleType() {
            return moduleType;
        }

        public string GetAlias() {
            return alias ?? module.GetModuleName();
        }

        public void ResolveModule(UIModule module) {
            this.module = module;
            if (string.IsNullOrEmpty(alias)) {
                // alias = module.GetModuleName();
            }
        }

        public UIModule GetModuleInstance() {
            return module;
        }

    }
    
    public class ModuleLoadException : Exception {

        public ModuleLoadException(string message) : base(message) { }

    }

    public abstract class UIModule {

        public readonly Type type;
        public readonly bool IsBuiltIn;

        private LightList<StyleSheet> styleSheets;

        private List<bool> conditionResults;
        private IList<StyleCondition> styleConditions;
        private Func<TemplateLookup, TemplateLocation?> templateResolver;

        internal readonly IList<ModuleReference> dependencies;
        internal readonly PagedLightList<ProcessedType> elementTypes;
        internal readonly Dictionary<string, ProcessedType> tagNameMap;

       // internal readonly LightList<TemplateFileShell> templateShells;
        private UIModule[] flattenedDependencyTree;

        private string assetPath;
        private string moduleName;
        internal ushort index;

        public string location { get; internal set; }

        protected UIModule() {

            if (!UIForiaRuntime.s_ConstructionAllowed) {
                throw new ModuleLoadException("Modules should never have their constructor called outside of the ModuleSystem initialization.");
            }

            this.type = GetType();
            this.moduleName = type.Name;
            this.dependencies = new List<ModuleReference>();
       //     this.templateShells = new LightList<TemplateFileShell>();
            this.IsBuiltIn = type == typeof(BuiltInElementsModule);
            this.elementTypes = new PagedLightList<ProcessedType>(32);
            this.tagNameMap = new Dictionary<string, ProcessedType>(31);
        }

        // internal Action zz__INTERNAL_DO_NOT_CALL; // use this for precompiled loading instead of doing type reflection to find caller type

        public virtual void Configure() { }

        protected void SetModuleName(string moduleName) {
            if (moduleName == null) {
                moduleName = GetType().Name;
            }

            this.moduleName = moduleName;
        }

        protected void AddDependency<TDependency>(string alias = null) where TDependency : UIModule, new() {
            if (typeof(TDependency).IsAbstract) {
                throw new InvalidArgumentException("Dependencies must be concrete classes. " + TypeNameGenerator.GetTypeName(typeof(TDependency)) + " is abstract");
            }

            dependencies.Add(new ModuleReference(typeof(TDependency), alias));
        }

        // public virtual void BuildCustomStyles(IStyleCodeGenerator generator) { }

        protected void SetTemplateResolver(Func<TemplateLookup, TemplateLocation?> resolver) {
            this.templateResolver = resolver;
        }

        internal TemplateLocation? ResolveTemplatePath(TemplateLookup typeLookup) {
            if (templateResolver != null) {
                return templateResolver(typeLookup);
            }

            return new TemplateLocation(location + typeLookup.declaredTemplatePath, typeLookup.templateId);
        }

        protected internal void UpdateConditions(DisplayConfiguration displayConfiguration) {
            if (styleConditions == null) return;

            conditionResults = conditionResults ?? new List<bool>();
            conditionResults.Clear();

            for (int i = 0; i < styleConditions.Count; i++) {
                conditionResults.Add(styleConditions[i].fn(displayConfiguration));
            }
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

        public bool HasStyleCondition(CharSpan conditionName) {
            if (styleConditions == null) return false;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionName) {
                    return true;
                }
            }

            return false;
        }

        public UIModule GetDependency(string moduleName) {
            for (int i = 0; i < dependencies.Count; i++) {
                if (dependencies[i].GetAlias() == moduleName) {
                    return dependencies[i].GetModuleInstance();
                }
            }

            return null;
        }

        protected internal void RegisterDisplayCondition(string condition, Func<DisplayConfiguration, bool> fn) {
            if (styleConditions == null) {
                styleConditions = new List<StyleCondition>() {
                    new StyleCondition(0, condition, fn)
                };
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

        public bool TryResolveTagName(string moduleName, string tagName, Diagnostics diagnostics, string file, LineInfo lineInfo, out ProcessedType processedType) {
            processedType = ResolveTagName(moduleName, tagName, diagnostics, file, lineInfo);
            return processedType != null;
        }

        // this is called from multiple threads!
        internal ProcessedType ResolveTagName(string moduleName, string tagName, Diagnostics diagnostics, string file, LineInfo lineInfo) {

            ProcessedType retn;

            if (string.IsNullOrEmpty(moduleName)) {
                // must be in this module or default.
                if (!IsBuiltIn) {
                    if (tagNameMap.TryGetValue(tagName, out retn)) {
                        return retn;
                    }
                }

                if (UIForiaRuntime.BuiltInModule.tagNameMap.TryGetValue(tagName, out retn)) {
                    return retn;
                }

                diagnostics.LogError($"Unable to resolve element tag name '{tagName}'.", file, lineInfo.line, lineInfo.column);

                return null;
            }

            // todo -- if we support <Using module="x" as="y"/> do the resolution here
            UIModule module = GetDependency(moduleName);

            if (module == null) {
                List<string> list = dependencies.Select(d => d.GetAlias()).ToList();
                diagnostics?.LogError($"Unable to resolve module `{moduleName}`. Available module names from current module ({GetType().GetTypeName()}) are {StringUtil.ListToString(list)}", file, lineInfo.line, lineInfo.column);
                return null;
            }

            if (module.tagNameMap.TryGetValue(tagName, out retn)) {
                return retn;
            }

            diagnostics?.LogError($"Unable to resolve tag name `{tagName}` from module {moduleName}.", file, lineInfo.line, lineInfo.column);
            return null;
        }

        public string GetModuleName() {
            return moduleName;
        }

        // doesn't resolve closed generics! We expect the compiler to handle this based on the open types
        internal ReadOnlySizedArray<ProcessedType> GetTemplateElements() {

            ListPage<ProcessedType> ptr = elementTypes.head;

            // todo cache this?
            SizedArray<ProcessedType> retn = new SizedArray<ProcessedType>(elementTypes.size / 2);

            while (ptr != null) {

                for (int j = 0; j < ptr.size; j++) {
                    ProcessedType t = ptr.data[j];

                    if (t.resolvedTemplateLocation != null) {

                        elementTypes.Add(t);

                    }

                }

                ptr = ptr.next;
            }

            return retn;

        }

        public void AddElementType(ProcessedType processedType) {
            elementTypes.Add(processedType);
        }

        public UIModule ResolveModuleName(string moduleName) {
            for (int i = 0; i < dependencies.Count; i++) {
                if (dependencies[i].GetAlias() == moduleName) {
                    return dependencies[i].GetModuleInstance();
                }
            }

            return null;
        }

        internal UIModule[] GetFlattenedDependencyTree() {

            if (flattenedDependencyTree != null) {
                return flattenedDependencyTree;
            }

            LightList<UIModule> dependencyList = LightList<UIModule>.Get();
            LightStack<UIModule> stack = LightStack<UIModule>.Get();

            stack.Push(this);

            while (stack.size != 0) {

                UIModule module = stack.Pop();

                bool found = false;
                for (int i = 0; i < dependencyList.size; i++) {
                    if (dependencyList.array[i] == module) {
                        found = true;
                        break;
                    }
                }

                if (found) {
                    continue;
                }

                dependencyList.Add(module);
                for (int i = 0; i < module.dependencies.Count; i++) {
                    stack.Push(module.dependencies[i].GetModuleInstance());
                }

            }

            flattenedDependencyTree = dependencyList.ToArray();
            dependencyList.Release();
            stack.Release();
            return flattenedDependencyTree;
        }

    }

}