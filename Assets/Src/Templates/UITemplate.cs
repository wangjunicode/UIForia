using System;
using System.Collections.Generic;
using Rendering;
using Src.Parsing.Style;
using UnityEngine;

namespace Src {

    public abstract class UITemplate {

        public ProcessedType processedElementType;
        public List<UITemplate> childTemplates;
        public List<AttributeDefinition> attributes;
        public List<ExpressionEvaluator> generatedBindings;

        public UIStyle normalStyleTemplate;
        public UIStyle hoverStyleTemplate;
        public UIStyle activeStyleTemplate;
        public UIStyle focusedStyleTemplate;
        public UIStyle disabledStyleTemplate;
        public List<UIStyle> baseStyles;

        public UITemplate() {
            childTemplates = new List<UITemplate>();
        }
        
        public abstract bool TypeCheck();

        public abstract UIElement CreateScoped(TemplateScope scope);

        public virtual Type ElementType => processedElementType.type;

        // todo -- also compile a binding if needed
        private void CompileStyleBinding(ref UIStyle style, AttributeDefinition attr, string styleKey) {
            switch (styleKey) {
                case "paint.backgroundColor":
                    style = style ?? new UIStyle();
                    style.paint.backgroundColor = StyleParseUtil.ParseColor(attr.value);
                    break;

                case "rect.x":
                    style = style ?? new UIStyle();
                    style.rect.x = StyleParseUtil.ParseMeasurement(attr.value);
                    break;

                case "rect.y":
                    style = style ?? new UIStyle();
                    style.rect.y = StyleParseUtil.ParseMeasurement(attr.value);
                    break;

                case "rect.w":
                    style = style ?? new UIStyle();
                    style.rect.width = StyleParseUtil.ParseMeasurement(attr.value);
                    break;

                case "rect.h":
                    style = style ?? new UIStyle();
                    style.rect.height = StyleParseUtil.ParseMeasurement(attr.value);
                    break;
                
                default:
                    Debug.LogWarning(
                        $"Attribute {attr.key} seems like a style property but doesn't match a valid style key name");
                    break;
            }
        }


        public void CompileStyles(ParsedTemplate template) {
            if (attributes == null) return;

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
                else if (attr.key.StartsWith("style.hover.")) {
                    CompileStyleBinding(ref hoverStyleTemplate, attr, attr.key.Substring("style.hover.".Length));
                }

                else if (attr.key.StartsWith("style.disabled.")) {
                    CompileStyleBinding(ref disabledStyleTemplate, attr, attr.key.Substring("style.disabled.".Length));
                }

                else if (attr.key.StartsWith("style.focused.")) {
                    CompileStyleBinding(ref focusedStyleTemplate, attr, attr.key.Substring("style.focused.".Length));
                }

                else if (attr.key.StartsWith("style.active.")) {
                    CompileStyleBinding(ref activeStyleTemplate, attr, attr.key.Substring("style.active.".Length));
                }
                else if (attr.key.StartsWith("style.")) {
                    CompileStyleBinding(ref normalStyleTemplate, attr, attr.key.Substring("style.".Length));
                }
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

        public void ApplyStyles(UIElement element, TemplateScope scope) {
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
        }

    }

}