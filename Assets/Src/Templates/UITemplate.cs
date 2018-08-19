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

        public UITemplate() {
            childTemplates = new List<UITemplate>();
        }

        
        public abstract RegistrationData CreateScoped(TemplateScope scope);

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
                        foreach (string name in names) {
                            UIStyle style = template.GetStyleInstance(name);
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

        private UIStyle GetStyleForState(StyleStateType state) {
            switch (state) {
                case StyleStateType.Normal:
                    return normalStyleTemplate;
                case StyleStateType.Active:
                    return activeStyleTemplate;
                case StyleStateType.Disabled:
                    return disabledStyleTemplate;
                case StyleStateType.Hover:
                    return hoverStyleTemplate;
                case StyleStateType.Focused:
                    return focusedStyleTemplate;
                default: return null;
            }
        }

        public virtual bool Compile(ParsedTemplate template) {
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
                element.style.SetInstanceStyle(new UIStyle(normalStyleTemplate), StyleStateType.Normal);
            }

            if (hoverStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(hoverStyleTemplate), StyleStateType.Hover);
            }

            if (disabledStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(disabledStyleTemplate), StyleStateType.Disabled);
            }

            if (focusedStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(focusedStyleTemplate), StyleStateType.Focused);
            }

            if (activeStyleTemplate != null) {
                element.style.SetInstanceStyle(new UIStyle(activeStyleTemplate), StyleStateType.Active);
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