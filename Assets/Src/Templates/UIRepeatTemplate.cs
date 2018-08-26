using System;
using System.Collections.Generic;

namespace Src {

    public class UIRepeatTemplate : UITemplate {

        public const string ListAttrName = "list";
        public const string AliasAttrName = "as";

        private Type genericArgType;
        private Binding[] terminalBindings;
        private Binding[] childBindings;

        public UIRepeatTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {

            ReflectionUtil.ObjectArray2[0] = childTemplates[0];
            ReflectionUtil.ObjectArray2[1] = scope;

            UIElement instance = (UIElement) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(UIRepeatElement<>),
                genericArgType,
                ReflectionUtil.ObjectArray2
            );

            UIElementCreationData data = GetCreationData(instance, scope.context);

            UIRepeatTerminal terminal = new UIRepeatTerminal();

            UIElementCreationData terminalData = new UIElementCreationData();
            terminalData.element = terminal;
            terminalData.bindings = terminalBindings;
            terminalData.context = scope.context;
            terminalData.constantBindings = Binding.EmptyArray;
            terminalData.inputBindings = InputBindings.InputBinding.EmptyArray;
            terminalData.constantStyleBindings = null;

            scope.SetParent(terminalData, data);

            return data;
        }

        // todo remove constants 
        public override bool Compile(ParsedTemplate template) {

            AttributeDefinition listAttr = GetAttribute(ListAttrName);
            AttributeDefinition aliasAttr = GetAttribute(AliasAttrName);

            aliasAttr.isCompiled = true;
            listAttr.isCompiled = true;

            Expression listExpression = template.compiler.Compile(listAttr.value);

            genericArgType = listExpression.YieldedType;

            // for now assume generic and assume its a list
            Type[] genericTypes = genericArgType.GetGenericArguments();

            ReflectionUtil.TypeArray2[0] = genericArgType;
            ReflectionUtil.TypeArray2[1] = genericTypes[0];

            ReflectionUtil.ObjectArray3[0] = listExpression;
            ReflectionUtil.ObjectArray3[1] = "$index";
            ReflectionUtil.ObjectArray3[2] = "$length";

            RepeatBinding repeatBinding = (RepeatBinding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(RepeatBinding<,>),
                ReflectionUtil.TypeArray2,
                ReflectionUtil.ObjectArray3
            );

            terminalBindings = new Binding[1];
            terminalBindings[0] = new RepeatTerminalBinding(repeatBinding);

            childBindings = new Binding[1];
            childBindings[0] = new RepeatChildBinding(repeatBinding);

            bindingList.Add(repeatBinding);

            UIRepeatChildTemplate childTemplate = new UIRepeatChildTemplate(childTemplates);
            childTemplates.Clear();
            childTemplates.Add(childTemplate);
            childTemplate.repeatChildBindings = childBindings;
            
            template.contextDefinition.AddRuntimeAlias("$item", genericTypes[0]);
            template.contextDefinition.AddRuntimeAlias("$length", typeof(int));

            return base.Compile(template);
        }

    }

}