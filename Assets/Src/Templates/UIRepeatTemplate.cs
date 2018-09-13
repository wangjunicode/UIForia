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

        public override Type elementType => typeof(UIRepeatTemplate);

        public override MetaData CreateScoped(TemplateScope inputScope) {

            ReflectionUtil.ObjectArray2[0] = childTemplates[0];
            ReflectionUtil.ObjectArray2[1] = inputScope;

            UIElement instance = (UIElement) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(UIRepeatElement<>),
                genericArgType,
                ReflectionUtil.ObjectArray2
            );

            MetaData data = GetCreationData(instance, inputScope.context);

            UIRepeatTerminal terminal = new UIRepeatTerminal();

            MetaData terminalData = new MetaData(terminal, inputScope.context);
            terminalData.bindings = terminalBindings;
            terminalData.constantBindings = Binding.EmptyArray;
            terminalData.inputBindings = InputBindings.InputBinding.EmptyArray;
            terminalData.constantStyleBindings = null;

            data.AddChild(terminalData);

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

            // todo -- unsure this works across templates, might best to communicate via context instead
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