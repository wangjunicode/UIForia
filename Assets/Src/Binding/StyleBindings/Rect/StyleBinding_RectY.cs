using UIForia.Rendering;

namespace UIForia.StyleBindings {

//    public class StyleBinding_RectY : StyleBinding {
//
//        public readonly Expression<UIMeasurement> expression;
//
//        public StyleBinding_RectY(StyleState state, Expression<UIMeasurement> expression) : base(state) {
//            this.expression = expression;
//        }
//
//        public override void Execute(UIElement element, UITemplateContext context) {
//            UIMeasurement y = element.style.GetRectY(state);
//            UIMeasurement newY = expression.EvaluateTyped(context);
//            if (y != newY) {
//                element.style.SetRectY(newY, state);
//            }
//        }
//
//        public override bool IsConstant() {
//            return expression.IsConstant();
//        }
//
//        public override void Apply(UIStyle style, UITemplateContext context) {
//            style.rect.y = expression.EvaluateTyped(context);
//        }
//
//        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
//            styleSet.SetRectY(expression.EvaluateTyped(context), state);
//        }
//
//    }

}