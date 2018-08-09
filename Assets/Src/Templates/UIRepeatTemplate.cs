namespace Src {

    public class UIRepeatTemplate : UITemplate {

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            NonRenderedElement element = new NonRenderedElement();

            // <Repeat list="{values}"/>
            //     {item.stringOutput}
            //     binding = list[i].stringValue
            //     binding = listSizeChange
            //     eventually id like callbacks for list events (move, add, remove)
            //     so that they can be animated etc
            //     that probably only works with an observable list

            ExpressionBinding binding = new ManualExpressionBinding((context) => {
                return context.currentList.Count != 0;
            });

            for (int i = 0; i < attributes.Count; i++) {
                //scope.CreateBinding(binding, this, HandleBindingUpdate);
            }

            return element;
        }

    }

}