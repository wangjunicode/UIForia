using System;
using System.Collections.Generic;
using System.Linq;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Style;
using UIForia.Style2;
using UIForia.Util;

namespace UIForia {

    public class ModuleLoadException : Exception {

        public ModuleLoadException(string message) : base(message) { }

    }

    public abstract class Module : TypeProcessor.IDiagnosticProvider {

        public readonly Type type;
        public readonly bool IsBuiltIn;

        private LightList<StyleSheet2> styleSheets;

        private List<bool> conditionResults;
        private IList<StyleCondition> styleConditions;
        private Func<TemplateLookup, TemplateLocation?> templateResolver;
        
        internal readonly IList<ModuleReference> dependencies;
        internal readonly PagedLightList<ProcessedType> elementTypes;
        internal readonly Dictionary<string, ProcessedType> tagNameMap;
        
        private string assetPath;
        private string moduleName;

        public string location { get; internal set; }
        
        protected Module() {
            
            if (!ModuleSystem.s_ConstructionAllowed) {
                throw new ModuleLoadException("Modules should never have their constructor called.");
            }

            this.type = GetType();
            this.moduleName = type.Name;
            this.elementTypes = new PagedLightList<ProcessedType>(32);
            this.tagNameMap = new Dictionary<string, ProcessedType>(31);
            this.dependencies = new List<ModuleReference>();
            this.IsBuiltIn = type == typeof(BuiltInElementsModule);
        }

        // internal Action zz__INTERNAL_DO_NOT_CALL; // use this for precompiled loading instead of doing type reflection to find caller type

        public virtual void Configure() { }
        
        protected void SetModuleName(string moduleName) {
            if (moduleName == null) {
                moduleName = GetType().Name;
            }
            this.moduleName = moduleName;
        }

        protected void AddDependency<TDependency>(string alias = null) where TDependency : Module, new() {
            if (typeof(TDependency).IsAbstract) {
                throw new InvalidArgumentException("Dependencies must be concrete classes. " + TypeNameGenerator.GetTypeName(typeof(TDependency)) + " is abstract");
            }

            dependencies.Add(new ModuleReference(typeof(TDependency), alias));
        }
        
        public virtual void BuildCustomStyles(IStyleCodeGenerator generator) { }

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

        public List<bool> GetDisplayConditions() {
            return conditionResults;
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

        public Module GetDependency(string moduleName) {
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
        
        internal ProcessedType ResolveTagName(string moduleName, string tagName, TypeProcessor.DiagnosticWrapper diagnosticWrapper) {

            ProcessedType retn;

            if (string.IsNullOrEmpty(moduleName)) {
                // must be in this module or default.
                if (!IsBuiltIn) {
                    if (tagNameMap.TryGetValue(tagName, out retn)) {
                        return retn;
                    }
                }
                
                return ModuleSystem.BuiltInModule.tagNameMap.TryGetValue(tagName, out retn) ? retn : null;

            }

            // todo -- if we support <Using module="x" as="y"/> do the resolution here
            Module module = GetDependency(moduleName);

            if (module == null) {
                List<string> list = dependencies.Select(d => d.GetAlias()).ToList();
                diagnosticWrapper.AddDiagnostic($"Unable to resolve module `{moduleName}`. Available module names from current module ({GetType().GetTypeName()}) are {StringUtil.ListToString(list)}");
                return null;
            }
            
            if (module.tagNameMap.TryGetValue(tagName, out retn)) {
                return retn;
            }
            
            diagnosticWrapper.AddDiagnostic($"Unable to resolve tag name `{tagName}` from module {moduleName}.");
            return null;
        }

        public void AddDiagnostic(string message) {
            
        }

        public string GetModuleName() {
            return moduleName;
        }

    }

}