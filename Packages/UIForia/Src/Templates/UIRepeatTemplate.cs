using System;
using System.Collections.Generic;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Templates {

    public class UIRepeatTemplate : UITemplate {

        public const string ListAttrName = "list";
        public const string AliasAttrName = "as";
        public const string LengthAttrName = "lengthAlias";
        public const string IndexAttrName = "indexAs";

        private Expression listExpression;
        private Type genericArgType;

        private Type listType;
        private Type itemType;
        private string indexAlias;
        private string lengthAlias;
        private string itemAlias;
        private RepeatItemAliasResolver itemResolver;
        private RepeatIndexAliasResolver indexResolver;
        private RepeatLengthAliasResolver lengthResolver;

        public UIRepeatTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType {
            get { return typeof(UIRepeatElement); }
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            // todo -- support multiple children?

            ReflectionUtil.ObjectArray2[0] = childTemplates[0];
            ReflectionUtil.ObjectArray2[1] = inputScope;

            UIRepeatElement element = (UIRepeatElement) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(UIRepeatElement<>),
                itemType,
                ReflectionUtil.ObjectArray2
            );

            element.listExpression = listExpression;
            element.itemType = itemType;
            element.listType = listType;
            element.indexAlias = indexAlias;
            element.lengthAlias = lengthAlias;
            element.itemAlias = itemAlias;
            element.OriginTemplate = this;
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            return element;
        }

        public override void Compile(ParsedTemplate template) {
            // todo -- check conflicts 
            AttributeDefinition listAttr = GetAttribute(ListAttrName);
            AttributeDefinition aliasAttr = GetAttribute(AliasAttrName);
            AttributeDefinition lengthAttr = GetAttribute(LengthAttrName);
            AttributeDefinition indexAttr = GetAttribute(IndexAttrName);

            listAttr.isCompiled = true;

            listExpression = template.compiler.Compile(template.RootType, elementType, listAttr.value, null);

            genericArgType = listExpression.YieldedType;


            lengthAlias = "$length";
            indexAlias = "$index";
            itemAlias = "$item";

            if (aliasAttr != null) {
                itemAlias = "$" + aliasAttr.value;
                aliasAttr.isCompiled = true;
            }

            if (lengthAttr != null) {
                lengthAlias = "$" + lengthAttr.value;
                lengthAttr.isCompiled = true;
            }

            if (indexAttr != null) {
                indexAlias = "$" + indexAttr.value;
                indexAttr.isCompiled = true;
            }

            // for now assume generic and assume its a list
            Type[] genericTypes = genericArgType.GetGenericArguments();

            listType = genericArgType;
            if (!(typeof(IRepeatableList).IsAssignableFrom(listType))) {
                throw new CompileException("<Repeat> element list argument must of type RepeatableList");
            }

            itemType = genericTypes[0];

            itemResolver = new RepeatItemAliasResolver(itemAlias, itemType);
            indexResolver = new RepeatIndexAliasResolver(indexAlias);
            lengthResolver = new RepeatLengthAliasResolver(lengthAlias);

            template.compiler.AddAliasResolver(itemResolver);
            template.compiler.AddAliasResolver(indexResolver);
            template.compiler.AddAliasResolver(lengthResolver);

            base.Compile(template);
        }

        public override void PostCompile(ParsedTemplate template) {
            template.compiler.RemoveAliasResolver(itemResolver);
            template.compiler.RemoveAliasResolver(indexResolver);
            template.compiler.RemoveAliasResolver(lengthResolver);
        }

    }

}