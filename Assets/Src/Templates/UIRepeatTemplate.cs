using System;
using System.Collections.Generic;

namespace Src {

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
        
        public UIRepeatTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UIRepeatTemplate);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            UIRepeatElement instance = new UIRepeatElement(childTemplates[0], inputScope);
            instance.listExpression = listExpression;
            instance.itemType = itemType;
            instance.listType = listType;
            instance.indexAlias = indexAlias;
            instance.lengthAlias = lengthAlias;
            instance.itemAlias = itemAlias;
            
            MetaData data = GetCreationData(instance, inputScope.context);

            return data;
        }

        public override bool Compile(ParsedTemplate template) {
            // todo -- check conflicts 
            AttributeDefinition listAttr = GetAttribute(ListAttrName);
            AttributeDefinition aliasAttr = GetAttribute(AliasAttrName);
            AttributeDefinition lengthAttr = GetAttribute(LengthAttrName);
            AttributeDefinition indexAttr = GetAttribute(IndexAttrName);

            listAttr.isCompiled = true;

            listExpression = template.compiler.Compile(listAttr.value);

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
            itemType = genericTypes[0];

            template.contextDefinition.AddRuntimeAlias(indexAlias, typeof(int));
            template.contextDefinition.AddRuntimeAlias(lengthAlias, typeof(int));
            template.contextDefinition.AddRuntimeAlias(itemAlias, itemType);

            return base.Compile(template);
        }

    }

}