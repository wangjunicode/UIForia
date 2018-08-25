using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Src {

    public class UIRepeatTemplate : UITemplate {

        public const string ListAttrName = "list";
        public const string AliasAttrName = "as";

        private Type genericArgType;

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

            return GetCreationData(instance, scope.context);
        }

        public override bool Compile(ParsedTemplate template) {
            
            UIRepeatChildTemplate childTemplate = new UIRepeatChildTemplate(childTemplates);
            childTemplates.Clear();
            childTemplates.Add(childTemplate);
            
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
            ReflectionUtil.ObjectArray1[0] = listExpression;

            Binding repeatBinding = (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(RepeatBinding<,>),
                ReflectionUtil.TypeArray2,
                ReflectionUtil.ObjectArray1
            );

            bindingList.Add(repeatBinding);
            return base.Compile(template);
        }

    }

}