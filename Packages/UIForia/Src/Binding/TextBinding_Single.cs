namespace UIForia {

    public class TextBinding_Single : Binding {

        private readonly Expression<string> expression;

        public TextBinding_Single(Expression<string> expression) : base("text") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UITextElement textElement = (UITextElement) element;
            textElement.SetText(expression.Evaluate(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}