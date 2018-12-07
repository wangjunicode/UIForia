using System.Text;

namespace UIForia {

    public class TextBinding_Multiple : Binding {

        private readonly Expression<string>[] expressions;
        
        private static readonly StringBuilder builder = new StringBuilder(1024);
        
        public TextBinding_Multiple(Expression<string>[] expressions) : base("text") {
            this.expressions = expressions;
        }
        
        public override void Execute(UIElement element, ExpressionContext context) {
            UITextElement textElement = (UITextElement) element;
            
            for (int i = 0; i < expressions.Length; i++) {
                builder.Append(expressions[i].Evaluate(context));
            }

            textElement.SetText(builder.ToString());
            builder.Clear();
        }

        public override bool IsConstant() {
            for (int i = 0; i < expressions.Length; i++) {
                if (!expressions[i].IsConstant()) {
                    return false;
                }
            }

            return true;
        }

    }


}