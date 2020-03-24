using System;
using System.Collections.Generic;
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

        protected void AddDependency<TDependency>(string alias) where TDependency : Module, new() {
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
        
        private static ProcessedType AttemptResolveGenericTag(ProcessedType generic, ElementNode elementNode, TemplateShell templateShell) {
            if (!generic.IsUnresolvedGeneric) {
                return generic;
            }

            if (!string.IsNullOrEmpty(elementNode.genericTypeResolver)) {
                // todo -- diagnostics!!!
                return TypeProcessor.ResolveGenericElementType(generic, elementNode.genericTypeResolver, templateShell.referencedNamespaces, new TypeProcessor.DiagnosticWrapper(templateShell));
            }

            // if we dont have a generic type resolver, we might still be able to compile this element but the template compiler needs to do it since it 
            // is the only place in which the context to do so is available. 
            return generic;
        }

      
        internal ProcessedType ResolveTagName(ElementNode elementNode, TemplateShell templateShell) {

            ProcessedType retn;

            if (string.IsNullOrEmpty(elementNode.moduleName)) {
                // must be in this module or default.
                if (!IsBuiltIn) {
                    if (tagNameMap.TryGetValue(elementNode.tagName, out retn)) {
                        return AttemptResolveGenericTag(retn, elementNode, templateShell);
                    }
                }
                
                if (ModuleSystem.BuiltInModule.tagNameMap.TryGetValue(elementNode.tagName, out retn)) {
                    return AttemptResolveGenericTag(retn, elementNode, templateShell);
                }

                return null; // todo -- diagnostics instead?
                
            }

            Module module = GetDependency(elementNode.moduleName);

            if (module == null) {
                return null; // todo -- diagnostics!
            }
            
            if (module.tagNameMap.TryGetValue(elementNode.tagName, out retn)) {
                return AttemptResolveGenericTag(retn, elementNode, templateShell);
            }
            
            return null;
        }

        public void AddDiagnostic(string message) {
            
        }

        public string GetModuleName() {
            return moduleName;
        }

    }

}