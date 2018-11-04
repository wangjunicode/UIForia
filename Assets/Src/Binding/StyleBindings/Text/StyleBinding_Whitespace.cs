using System;
using Src.Rendering;

namespace Src.StyleBindings.Text {

    public class StyleBinding_Whitespace : StyleBinding {

        private readonly Expression<WhitespaceMode> expression;

        public StyleBinding_Whitespace(StyleState state, Expression<WhitespaceMode> expression) : base(RenderConstants.Whitespace, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            throw new NotImplementedException();
//            WhitespaceMode current = element.style.GetWhitespace(state);
//            WhitespaceMode newValue = expression.EvaluateTyped(context);
//            if (current != newValue) {
//                element.style.SetWhitespace(newValue, state);
//            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            throw new NotImplementedException();
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            throw new NotImplementedException();
            //styleSet.SetWhitespace(expression.EvaluateTyped(context), state);
        }

    }

}