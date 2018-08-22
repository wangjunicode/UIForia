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

    }

    public abstract class UITemplate {

        private string name;
        public readonly List<UITemplate> childTemplates;
        public readonly List<AttributeDefinition> attributes;
        
        private readonly StyleDefinition styleDefinition;
        private Binding[] bindings; // used for output
        protected readonly List<Binding> bindingList = new List<Binding>(); // used for compilation
        private static readonly StyleBindingCompiler styleCompiler = new StyleBindingCompiler();

        protected UITemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) {
            this.childTemplates = childTemplates;
            this.attributes = attributes;
            styleDefinition = new StyleDefinition();
            bindings = Binding.EmptyArray;
        }

        public UIElementCreationData GetCreationData(UIElement element, UITemplateContext context) {
            UIElementCreationData data = new UIElementCreationData();
            data.name = name;
            data.element = element;
            data.context = context;
            data.style = styleDefinition;
            data.bindings = bindings;
            return data;
        }

        public abstract UIElementCreationData CreateScoped(TemplateScope scope);

//        public abstract Type ElementType { get; }

        public void CompileStyles(ParsedTemplate template) {
            if (attributes == null || attributes.Count == 0) return;

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

            if (baseStyles != null) {
                styleDefinition.baseStyles = baseStyles;
            }

            if (styleList == null) return;
            
            styleDefinition.constantBindings = styleList.Where((s) => s.IsConstant()).ToArray();
            bindingList.AddRange(styleList.Where((s) => !s.IsConstant()));
        }

        public virtual bool Compile(ParsedTemplate template) {
            AttributeDefinition nameAttr = GetAttribute("x-name");
            bindings = new Binding[bindingList.Count];
            if (nameAttr != null) {
                this.name = nameAttr.value;
            }

            bindings = bindingList.ToArray();
            return true;
        }

        protected bool HasAttribute(string attributeName) {
            if (attributes == null) return false;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attributeName) return true;
            }

            return false;
        }

        protected AttributeDefinition GetAttribute(string attributeName) {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attributeName) return attributes[i];
            }

            return null;
        }

    }

}