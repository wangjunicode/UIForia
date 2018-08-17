using System;

namespace Src {

    public class UIRepeatTemplate : UITemplate {

        private Binding[] bindings;

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public override bool Compile(ParsedTemplate template) {
            
            ContextDefinition context = template.contextDefinition;
            
            AttributeDefinition listAttr = attributes.Find((attr) => attr.key == "list");
            AttributeDefinition aliasAttr = attributes.Find((attr) => attr.key == "itemAlias");
            AttributeDefinition indexAliasAttr = attributes.Find((attr) => attr.key == "indexAlias");
            AttributeDefinition lengthAliasAttr = attributes.Find((attr) => attr.key == "lengthAlias");
            
            // todo ensure item alias is a literal string starting with $
            // todo ensure length alias is a literal string starting with $
            // todo ensure index alias is a literal string starting with $
            
            string itemAlias = "$item";
            string lengthAlias = "$length";
            string indexAlias = "$i";
            
            if (aliasAttr != null) {
                itemAlias = aliasAttr.value;
                if (itemAlias[0] != '$') {
                    itemAlias = "$" + itemAlias;
                }
            }
            
            ExpressionNode listExpressionNode = new ExpressionParser(listAttr.value).Parse();
            ExpressionCompiler compiler = new ExpressionCompiler(context);
            Expression listExpression = compiler.Compile(listExpressionNode);
            Type listType = listExpressionNode.GetYieldedType(context);
            
            context.AddListContext(listType, itemAlias, indexAlias, lengthAlias);

            for (int i = 0; i < childTemplates.Count; i++) {
                childTemplates[i].Compile(template);
            }
            
            context.RemoveListContext(itemAlias);
            
            bindings = new Binding[] {
               // new RepeatEnterBinding()
            };

            return true;
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            UIRepeatElement element = new UIRepeatElement();


            scope.view.RegisterBindings(element, bindings, scope.context);

            return element;
        }

    }

}