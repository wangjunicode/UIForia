using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Compilers;
using Src.StyleBindings;

namespace Src {

    public class StyleDefinition {

        public List<UIStyle> baseStyles;
        public StyleBinding[] constantBindings;
        public StyleBinding[] dynamicStyleBindings;

    }

    public abstract class UITemplate {

        public string name;
        public List<UITemplate> childTemplates;
        public List<AttributeDefinition> attributes;
        public ProcessedType processedElementType;
        public StyleDefinition styleDefinition;
        public Binding[] bindings;
        
        public UITemplate() {
            childTemplates = new List<UITemplate>();
            styleDefinition = new StyleDefinition();
            bindings = Binding.EmptyArray;
        }

        public abstract UIElementCreationData CreateScoped(TemplateScope scope);

        public virtual Type ElementType => processedElementType.rawType;

        private static readonly StyleBindingCompiler styleCompiler = new StyleBindingCompiler();

        public void CompileStyles(ParsedTemplate template) {
            if (attributes == null) return;

            List<UIStyle> baseStyles = null;
            List<StyleBinding> styleList = null;

            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];

                if (!attr.key.StartsWith("style")) continue;

                baseStyles = baseStyles ?? new List<UIStyle>();

                if (attr.key == "style") {
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

                }
                else {
                    styleList = styleList ?? new List<StyleBinding>();
                    styleList.Add(styleCompiler.Compile(template.contextDefinition, attr.key, attr.value));
                }
            }

            styleDefinition.baseStyles = baseStyles;
            styleDefinition.constantBindings = styleList.Where((s) => s.IsConstant()).ToArray();
            styleDefinition.dynamicStyleBindings = styleList.Where((s) => !s.IsConstant()).ToArray();

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

//        ApplyConstantStyles(UIElement element, TemplateScope scope) {
//            element.style = new UIStyleSet(element, scope.view);
//            List<StyleDefinition> styles = new List<StyleDefinition>();
//            
//            if (baseStyles != null) {
//                // todo -- for now we only use 'normal' styles, no states
//                for (int i = 0; i < baseStyles.Count; i++) {
//                    element.style.AddBaseStyle(baseStyles[i]);
//                }
//            }
//
//            if (constantBindings != null) {
//                for (int i = 0; i < constantBindings.Length; i++) {
//                    constantBindings[i].Apply(element.style, scope.context);
//                }
//            }
//
//        }

    }

}