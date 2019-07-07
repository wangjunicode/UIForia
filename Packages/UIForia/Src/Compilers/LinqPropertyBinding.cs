using System;
using System.Linq.Expressions;
using UIForia.Elements;

namespace UIForia.Compilers {

    public class LinqPropertyBinding : LinqBinding {

        public Action<UIElement, UIElement, TemplateContext> fn;
        public LambdaExpression lambdaExpression;

        public void Initialize() {
            fn = (Action<UIElement, UIElement, TemplateContext>)lambdaExpression.Compile();
        }
            
        public override void Execute(UIElement templateRoot, UIElement current, TemplateContext context) {
            fn(templateRoot, current, context);
        }

    }

}