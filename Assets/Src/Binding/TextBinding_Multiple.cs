using System.Text;

namespace Src {

    public class TextBinding_Multiple : Binding {

        private readonly Expression<string>[] expressions;
        
        private static readonly StringBuilder builder = new StringBuilder(1024);
        
        public TextBinding_Multiple(Expression<string>[] expressions) {
            this.expressions = expressions;
        }
        
        public override void Execute(UIElement element, UITemplateContext context) {
            UITextElement textElement = (UITextElement) element;
            
            for (int i = 0; i < expressions.Length; i++) {
                builder.Append(expressions[i].EvaluateTyped(context));
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