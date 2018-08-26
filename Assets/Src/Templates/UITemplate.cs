using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Compilers;
using Src.InputBindings;
using Src.StyleBindings;

namespace Src {

    public abstract class UITemplate {

        public List<UITemplate> childTemplates;
        public readonly List<AttributeDefinition> attributes;

        private Binding[] bindings;
        private Binding[] constantBindings;
        private Binding[] conditionalBindings;
        private InputBinding[] inputBindings;

        private List<UIStyle> baseStyles;
        private List<StyleBinding> constantStyleBindings;

        protected List<Binding> bindingList;

        private static readonly StyleBindingCompiler styleCompiler = new StyleBindingCompiler(null);
        private static readonly InputBindingCompiler inputCompiler = new InputBindingCompiler(null);
        private static readonly PropertyBindingCompiler propCompiler = new PropertyBindingCompiler(null);

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

        public UIElementCreationData GetCreationData(UIElement element, UITemplateContext context) {
            UIElementCreationData data = new UIElementCreationData();
            data.element = element;
            data.context = context;
            data.baseStyles = baseStyles;

            data.bindings = bindings;
            data.inputBindings = inputBindings;
            data.constantBindings = constantBindings;
            data.constantStyleBindings = constantStyleBindings;
            data.conditionalBindings = conditionalBindings;
            return data;
        }

        public abstract UIElementCreationData CreateScoped(TemplateScope scope);

        public void CompileStyleBindings(ParsedTemplate template) {
            if (attributes == null || attributes.Count == 0) return;

            styleCompiler.SetContext(template.contextDefinition);
            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];
                StyleBinding binding = styleCompiler.Compile(attr);
                if (binding == null) continue;

                if (binding.IsConstant()) {
                    constantStyleBindings.Add(binding);
                }
                else {
                    bindingList.Add(binding);
                }
            }

        }

        public virtual void CompileInputBindings(ParsedTemplate template) {
            inputCompiler.SetContext(template.contextDefinition);
            inputBindings = inputCompiler.Compile(attributes).ToArray();
        }

        public virtual void CompilePropertyBindings(ParsedTemplate template) {
            if (attributes == null || attributes.Count == 0) return;
            propCompiler.SetContext(template.contextDefinition);

            // todo -- filter out already compiled attributes, warn if attribute was already handled
//            propCompiler.CompileAttribute(attributes.Where(a) => !a.isCompiled);
            // set constant bindings here
        }

        public virtual bool Compile(ParsedTemplate template) {
            ResolveBaseStyles(template);
            CompileStyleBindings(template);
            CompileInputBindings(template);
            CompilePropertyBindings(template);
            CompileConditionalBindings(template);
            ResolveConstantBindings();
            return true;
        }

        // todo -- show / hide / disable
        protected virtual void CompileConditionalBindings(ParsedTemplate template) {
            AttributeDefinition ifDef = GetAttribute("x-if");
            AttributeDefinition unlessDef = GetAttribute("x-unless");
            AttributeDefinition showDef = GetAttribute("x-show");
            AttributeDefinition hideDef = GetAttribute("x-hide");

            if (ifDef != null) {
                conditionalBindings = new Binding[1];
                Expression<bool> ifExpression = template.compiler.Compile<bool>(ifDef.value);
                ifDef.isCompiled = true;
                conditionalBindings[0] = new EnabledBinding(ifExpression);
            }
        }

        protected void ResolveConstantBindings() {
            bindings = bindingList.Where((binding) => !binding.IsConstant()).ToArray();
            constantBindings = bindingList.Where((binding) => binding.IsConstant()).ToArray();
            bindingList = null;
        }

        protected AttributeDefinition GetAttribute(string attributeName) {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attributeName) return attributes[i];
            }

            return null;
        }

        protected List<AttributeDefinition> GetUncompiledAttributes() {
            return attributes.Where((attr) => !attr.isCompiled).ToList();
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

    public class ShowBinding : Binding {

        private readonly Expression<bool> expression;

        public ShowBinding(Expression<bool> expression) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            
        }

        public override bool IsConstant() {
            throw new System.NotImplementedException();
        }

    }

}