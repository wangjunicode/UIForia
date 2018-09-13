using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Rendering;
using Src.Compilers;
using Src.Input;
using Src.InputBindings;
using Src.StyleBindings;
using Src.Systems;

namespace Src {

    public abstract class UITemplate {

        public const string k_SpecialAttrPrefix = "x-";
        
        public List<UITemplate> childTemplates;
        public readonly List<AttributeDefinition> attributes;

        protected internal Binding[] bindings;
        protected internal Binding[] constantBindings;
        protected Binding[] conditionalBindings;
        protected InputBinding[] inputBindings;

        protected List<UIStyle> baseStyles;
        protected List<StyleBinding> constantStyleBindings;
        
        protected DragEventCreator[] dragEventCreators;
        protected DragEventHandler[] dragEventHandlers;
        protected MouseEventHandler[] mouseEventHandlers;
        protected KeyboardEventHandler[] keyboardEventHandlers;
        
        protected List<Binding> bindingList;
        protected List<ValueTuple<string, string>> templateAttributes;

        public bool acceptFocus;
        private static readonly StyleBindingCompiler styleCompiler = new StyleBindingCompiler(null);
        private static readonly InputBindingCompiler inputCompiler = new InputBindingCompiler(null);
        private static readonly PropertyBindingCompiler propCompiler = new PropertyBindingCompiler(null);

        public abstract Type elementType { get; }
        
        protected UITemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) {
            this.childTemplates = childTemplates;
            this.attributes = attributes;

            this.baseStyles = new List<UIStyle>();
            this.bindingList = new List<Binding>();
            this.constantStyleBindings = new List<StyleBinding>();

            this.bindings = Binding.EmptyArray;
            this.inputBindings = InputBinding.EmptyArray;
            this.constantBindings = Binding.EmptyArray;
            this.conditionalBindings = Binding.EmptyArray;
        }

        public MetaData GetCreationData(UIElement element, UITemplateContext context) {
            MetaData data = new MetaData(element, context);
            data.baseStyles = baseStyles;
            data.bindings = bindings;
            data.inputBindings = inputBindings;
            data.constantBindings = constantBindings;
            data.constantStyleBindings = constantStyleBindings;
            data.conditionalBindings = conditionalBindings;
            data.keyboardEventHandlers = keyboardEventHandlers;
            data.mouseEventHandlers = mouseEventHandlers;
            data.dragEventCreators = dragEventCreators;
            data.dragEventHandlers = dragEventHandlers;
            element.templateAttributes = templateAttributes;
            if (acceptFocus) {
                element.flags |= UIElementFlags.AcceptFocus;
            }
            return data;
        }

        public abstract MetaData CreateScoped(TemplateScope inputScope);

        public void CompileStyleBindings(ParsedTemplate template) {
            if (attributes == null || attributes.Count == 0) return;

            styleCompiler.SetContext(template.contextDefinition);
            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];
                StyleBinding binding = styleCompiler.Compile(attr);
                if (binding == null) continue;
                attr.isCompiled = true;

                if (binding.IsConstant()) {
                    constantStyleBindings.Add(binding);
                }
                else {
                    bindingList.Add(binding);
                }
            }
        }

        public virtual bool Compile(ParsedTemplate template) {
            ResolveBaseStyles(template);
            CompileStyleBindings(template);
            CompileInputBindings(template);
            CompilePropertyBindings(template);
            CompileConditionalBindings(template);
            ResolveActualAttributes();
            ResolveConstantBindings();
            acceptFocus = elementType.GetCustomAttribute(typeof(AcceptFocus)) != null;
            return true;
        }

        [PublicAPI]
        public AttributeDefinition GetAttribute(string attributeName) {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attributeName) return attributes[i];
            }

            return null;
        }

        [PublicAPI]
        public List<AttributeDefinition> GetUncompiledAttributes() {
            return attributes.Where((attr) => !attr.isCompiled).ToList();
        }

        protected void ResolveActualAttributes() {
            if (attributes == null) return;
            // todo maybe can be shared
            // todo enforce constant-ness of attr values
            IEnumerable<AttributeDefinition> realAttributes = attributes.Where(a => a.isRealAttribute).ToArray();
            if (realAttributes.Any()) {
                templateAttributes = new List<ValueTuple<string, string>>();
                foreach (var s in realAttributes) {
                    templateAttributes.Add(ValueTuple.Create(s.key.Substring(k_SpecialAttrPrefix.Length), s.value));
                }
            }
        }
        
        protected void AddConditionalBinding(Binding binding) {
            Array.Resize(ref conditionalBindings, conditionalBindings.Length + 1);
            conditionalBindings[conditionalBindings.Length - 1] = binding;
        }

        protected void AddConstantBinding(Binding binding) {
            Array.Resize(ref constantBindings, constantBindings.Length + 1);
            constantBindings[constantBindings.Length - 1] = binding;
        }
        
        protected virtual void CompileInputBindings(ParsedTemplate template) {
            inputCompiler.SetContext(template.contextDefinition);
            List<MouseEventHandler> mouseHandlers = inputCompiler.CompileMouseEventHandlers(elementType, attributes);
            List<KeyboardEventHandler> keyboardHandlers = inputCompiler.CompileKeyboardEventHandlers(elementType, attributes);
            List<DragEventCreator> dragCreators = inputCompiler.CompileDragEventCreators(elementType, attributes);
            List<DragEventHandler> dragHandlers = inputCompiler.CompileDragEventHandlers(elementType, attributes);
            if (mouseHandlers != null) {
                mouseEventHandlers = mouseHandlers.ToArray();
            }

            if (keyboardHandlers != null) {
                keyboardEventHandlers = keyboardHandlers.ToArray();
            }

            if (dragCreators != null) {
                dragEventCreators = dragCreators.ToArray();
            }

            if (dragHandlers != null) {
                dragEventHandlers = dragHandlers.ToArray();
            }
        }


        protected virtual void CompilePropertyBindings(ParsedTemplate template) {
            if (attributes == null || attributes.Count == 0) return;
          
            propCompiler.SetContext(template.contextDefinition);

            for (int i = 0; i < attributes.Count; i++) {
                if(attributes[i].isCompiled) continue;
                if (attributes[i].key.StartsWith("x-")) {
                    continue;
                }
                attributes[i].isCompiled = true;
                Binding binding = propCompiler.CompileAttribute(elementType, attributes[i]);
                if (binding != null) {
                    bindingList.Add(binding);
                }
            }
        }
        
        // todo -- show / hide / disable
        protected virtual void CompileConditionalBindings(ParsedTemplate template) {
            AttributeDefinition ifDef = GetAttribute("x-if");
            AttributeDefinition unlessDef = GetAttribute("x-unless");
            AttributeDefinition showDef = GetAttribute("x-show");
            AttributeDefinition hideDef = GetAttribute("x-hide");

            if (ifDef != null) {
                Expression<bool> ifExpression = template.compiler.Compile<bool>(ifDef.value);
                ifDef.isCompiled = true;
                AddConditionalBinding(new EnabledBinding(ifExpression));
            }
        }

        protected void ResolveConstantBindings() {
            bindings = bindingList.Where((binding) => !binding.IsConstant()).ToArray();
            constantBindings = bindingList.Where((binding) => binding.IsConstant()).ToArray();
            bindingList = null;
        }

        private void ResolveBaseStyles(ParsedTemplate template) {
            AttributeDefinition styleAttr = GetAttribute("style");
            if (styleAttr == null) return;

            string[] names = styleAttr.value.Split(' ');
            foreach (string part in names) {
                UIStyle style = template.GetStyleInstance(part);
                if (style != null) {
                    baseStyles.Add(style);
                }
            }
        }

    }

}