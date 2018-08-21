using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Compilers;
using Src.StyleBindings;

namespace Src {

    public abstract class UITemplate {

        public ProcessedType processedElementType;
        public List<UITemplate> childTemplates;
        public List<AttributeDefinition> attributes;

        public UIStyle normalStyleTemplate;
        public UIStyle hoverStyleTemplate;
        public UIStyle activeStyleTemplate;
        public UIStyle focusedStyleTemplate;
        public UIStyle disabledStyleTemplate;
        public List<UIStyle> baseStyles;

        public StyleBinding[] constantBindings;
        public StyleBinding[] dynamicStyleBindings;
        public string name;

        public UITemplate() {
            childTemplates = new List<UITemplate>();
        }

        public abstract UIElementCreationData CreateScoped(TemplateScope scope);

        public virtual Type ElementType => processedElementType.rawType;

        private static readonly StyleBindingCompiler styleCompiler = new StyleBindingCompiler();

        public void CompileStyles(ParsedTemplate template) {
            if (attributes == null) return;

            List<StyleBinding> styleList = new List<StyleBinding>();

            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];

                if (!attr.key.StartsWith("style")) continue;

                if (attr.key == "style") {
                    baseStyles = new List<UIStyle>();

                    if (attr.value.IndexOf(' ') != -1) {
                        string[] names = attr.value.Split(' ');
                        foreach (string part in names) {
                            UIStyle style = template.GetStyleInstance(part);
                            if (style != null) {
                                baseStyles.Add(style);
                            }
                        }
                    }
                    else {
                        UIStyle style = template.GetStyleInstance(attr.value);
                        if (style != null) {
                            baseStyles.Add(style);
                        }
                    }

                    if (baseStyles.Count == 0) baseStyles = null;
                }
                else {
                    StyleBinding binding = styleCompiler.Compile(template.contextDefinition, attr.key, attr.value);

                    styleList.Add(binding);

                }
            }

            constantBindings = styleList.Where((s) => s.IsConstant()).ToArray();
            dynamicStyleBindings = styleList.Where((s) => !s.IsConstant()).ToArray();

        }

        private UIStyle GetStyleForState(StyleState state) {
            switch (state) {
                case StyleState.Normal:
                    return normalStyleTemplate;
                case StyleState.Active:
                    return activeStyleTemplate;
                case StyleState.Disabled:
                    return disabledStyleTemplate;
                case StyleState.Hover:
                    return hoverStyleTemplate;
                case StyleState.Focused:
                    return focusedStyleTemplate;
                default: return null;
            }
        }

        public virtual bool Compile(ParsedTemplate template) {
            AttributeDefinition nameAttr = GetAttribute("x-name");
            if (nameAttr != null) {
                this.name = nameAttr.value;
            }
            return true;
        }

        public bool HasAttribute(string attributeName) {
            if (attributes == null) return false;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attributeName) return true;
            }

            return false;
        }

        public AttributeDefinition GetAttribute(string attributeName) {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attributeName) return attributes[i];
            }

            return null;
        }

        public void ApplyConstantStyles(UIElement element, TemplateScope scope) {
            element.style = new UIStyleSet(element, scope.view);

            if (normalStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(normalStyleTemplate), StyleState.Normal);
            }

            if (hoverStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(hoverStyleTemplate), StyleState.Hover);
            }

            if (disabledStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(disabledStyleTemplate), StyleState.Disabled);
            }

            if (focusedStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(focusedStyleTemplate), StyleState.Focused);
            }

            if (activeStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(activeStyleTemplate), StyleState.Active);
            }

            if (baseStyles != null) {
                // todo -- for now we only use 'normal' styles, no states
                for (int i = 0; i < baseStyles.Count; i++) {
                    element.style.AddBaseStyle(baseStyles[i]);
                }
            }

            if (constantBindings != null) {
                for (int i = 0; i < constantBindings.Length; i++) {
                    constantBindings[i].Apply(element.style, scope.context);
                }
            }

        }

    }

}