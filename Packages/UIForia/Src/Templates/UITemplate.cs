using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UIForia.InputBindings;
using UIForia.Systems;
using UIForia.Compilers;
using UIForia.Input;
using UIForia.Rendering;
using UIForia.StyleBindings;
using UnityEngine;

namespace UIForia {

    public abstract class UITemplate {

        public const string k_SpecialAttrPrefix = "x-";
        
        public ushort id;
        public bool acceptFocus;
        public readonly List<UITemplate> childTemplates;
        public readonly List<AttributeDefinition> attributes;

        public Binding[] bindings;
        public Binding[] constantBindings;

        public readonly List<UIStyleGroup> baseStyles;
        public readonly List<StyleBinding> constantStyleBindings;
        
        public DragEventCreator[] dragEventCreators;
        public DragEventHandler[] dragEventHandlers;
        public MouseEventHandler[] mouseEventHandlers;
        public KeyboardEventHandler[] keyboardEventHandlers;
        
        public List<Binding> bindingList;
        public List<ValueTuple<string, string>> templateAttributes;

        
        private static ushort s_IdGenerator;
        private static readonly StyleBindingCompiler styleCompiler = new StyleBindingCompiler(null);
        private static readonly InputBindingCompiler inputCompiler = new InputBindingCompiler(null);
        private static readonly PropertyBindingCompiler propCompiler = new PropertyBindingCompiler(null);

        
        protected UITemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) {
            this.id = ++s_IdGenerator;
            this.childTemplates = childTemplates;
            this.attributes = attributes;

            // todo -- remove allocations
            this.baseStyles = new List<UIStyleGroup>();
            this.bindingList = new List<Binding>();
            this.constantStyleBindings = new List<StyleBinding>();

            this.bindings = Binding.EmptyArray;
            this.constantBindings = Binding.EmptyArray;
        }
        
        public abstract Type elementType { get; }

        public MetaData GetCreationData(UIElement element, UITemplateContext context) {
            MetaData data = new MetaData(element, context);
            data.baseStyles = baseStyles;
            data.bindings = bindings;
            data.constantBindings = constantBindings;
            data.constantStyleBindings = constantStyleBindings;
            data.keyboardEventHandlers = keyboardEventHandlers;
            data.mouseEventHandlers = mouseEventHandlers;
            data.dragEventCreators = dragEventCreators;
            data.dragEventHandlers = dragEventHandlers;
//            element.templateAttributes = templateAttributes;
            if (acceptFocus) {
                element.flags |= UIElementFlags.AcceptFocus;
            }
            return data;
        }

        public abstract UIElement CreateScoped(TemplateScope inputScope);

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
            if (!(typeof(UIElement).IsAssignableFrom(elementType))) {
                Debug.Log($"{elementType} must be a subclass of {typeof(UIElement)} in order to be used in templates");
                return false;
            }
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
            if (binding.IsConstant()) {
                bindingList.Add(binding);
            }
            else {
//                Array.Resize(ref conditionalBindings, conditionalBindings.Length + 1);
//                conditionalBindings[conditionalBindings.Length - 1] = binding;
                bindingList.Add(binding);

            }
        }
        
        protected void CompileInputBindings(ParsedTemplate template) {
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


        protected void CompilePropertyBindings(ParsedTemplate template) {
            if (attributes == null || attributes.Count == 0) return;

            try {
                propCompiler.SetContext(template.contextDefinition);

                for (int i = 0; i < attributes.Count; i++) {
                    if (attributes[i].isCompiled) continue;
                    if (attributes[i].key.StartsWith("x-") || attributes[i].key == "style") {
                        continue;
                    }

                    attributes[i].isCompiled = true;
                    Binding binding = propCompiler.CompileAttribute(elementType, attributes[i]);
                    if (binding != null) {
                        bindingList.Add(binding);
                    }
                }
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        }
        
        // todo -- show / hide / disable
        protected void CompileConditionalBindings(ParsedTemplate template) {
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

            // todo -- handle + and - instead of space
            if (styleAttr.value.IndexOf(' ') != -1) {
                string[] names = styleAttr.value.Split(' ');
                foreach (string part in names) {
                    UIStyleGroup style = template.ResolveStyleGroup(part.Trim());
                    if (style != null) {
                        baseStyles.Add(style);
                    }
                }
            }
            else {
                UIStyleGroup style = template.ResolveStyleGroup(styleAttr.value);
                if (style != null) {
                    baseStyles.Add(style);
                }
            }
        }

        public static void AssignContext(UIElement element, UITemplateContext context) {
            element.TemplateContext = context;
                
            if (element.children == null) return;

            for (int i = 0; i < element.children.Length; i++) {
                
                if (element.children[i].templateRef is UIElementTemplate) {
                    element.children[i].TemplateContext = context;
                    continue;
                }
                
                if (element.children[i] is UIChildrenElement) {
                    continue;
                }
                
                AssignContext(element.children[i], context);
            }
        }

        protected void AssignChildrenAndTemplate(TemplateScope scope, UIElement element) {
            element.children = new UIElement[childTemplates.Count];
            
            for (int i = 0; i < childTemplates.Count; i++) {
                element.children[i] = childTemplates[i].CreateScoped(scope);
                element.children[i].parent = element;
                element.children[i].templateParent = element;
            }

            element.TemplateContext = scope.context;
            element.templateRef = this;
        }
    }

}