using UIForia.Rendering;

namespace UIForia.StyleBindings {

//    public class StyleBinding_RectX : StyleBinding {
//
//        public readonly Expression<UIMeasurement> expression;
//
//        public StyleBinding_RectX(StyleState state, Expression<UIMeasurement> expression) : base(state) {
//            this.expression = expression;
//        }
//
//        public override void Execute(UIElement element, UITemplateContext context) {
//            UIMeasurement x = element.style.GetRectX(state);
//            UIMeasurement newX = expression.EvaluateTyped(context);
//            if (x != newX) {
//                element.style.SetRectX(newX, state);
//            }
//        }
//
//        public override bool IsConstant() {
//            return expression.IsConstant();
//        }
//
//        public override void Apply(UIStyle style, UITemplateContext context) {
//            style.rect.x = expression.EvaluateTyped(context);
//        }
//
//        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
//            styleSet.SetRectX(expression.EvaluateTyped(context), state);
//        }
//
//    }

}