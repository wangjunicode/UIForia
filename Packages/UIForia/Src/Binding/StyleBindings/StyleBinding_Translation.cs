using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_Translation : StyleBinding {

        public readonly Expression<MeasurementPair> expression;

        public StyleBinding_Translation(string propertyName, StyleState state, Expression<MeasurementPair> expression)
            : base(propertyName, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;
            
            MeasurementPair oldValue = new MeasurementPair(
                element.style.TransformPositionX,
                element.style.TransformPositionY
            );
            MeasurementPair value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetTransformPositionX(value.x, state);
                element.style.SetTransformPositionY(value.y, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            MeasurementPair value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value.x));
            style.SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value.y));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            MeasurementPair value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value.x), state);
            styleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value.y), state);
        }

    }
    
    public class StyleBinding_PreferredSize : StyleBinding {

        public readonly Expression<MeasurementPair> expression;

        public StyleBinding_PreferredSize(string propertyName, StyleState state, Expression<MeasurementPair> expression)
            : base(propertyName, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;
            
            MeasurementPair oldValue = new MeasurementPair(
                element.style.PreferredWidth,
                element.style.PreferredHeight
            );
            MeasurementPair value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetPreferredWidth(value.x, state);
                element.style.SetPreferredHeight(value.y, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            MeasurementPair value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value.x));
            style.SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value.y));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            MeasurementPair value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value.x), state);
            styleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value.y), state);
        }

    }

}
