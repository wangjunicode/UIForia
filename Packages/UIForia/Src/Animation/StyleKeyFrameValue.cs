using UIForia.Compilers;
using UIForia.Expressions;
using UIForia.Rendering;

namespace UIForia.Animation {

    public struct StyleKeyFrameValue {

        public readonly string rawExpression;
        public readonly StyleProperty styleProperty;
        private Expression expr;

        public StyleKeyFrameValue(StylePropertyId propertyId, string rawExpression) {
            this.rawExpression = rawExpression;
            this.styleProperty = new StyleProperty(propertyId);
            this.expr = null;
        }

        public StyleKeyFrameValue(StyleProperty property) {
            this.styleProperty = property;
            this.rawExpression = null;
            this.expr = null;
        }

        public bool IsCalculated => rawExpression != null;

        public StylePropertyId propertyId => styleProperty.propertyId;

        public T Evaluate<T>(ExpressionContext ctx) {
            return ((Expression<T>)expr).Evaluate(ctx);
        }
        
        public static implicit operator StyleKeyFrameValue(StyleProperty property) {
            return new StyleKeyFrameValue(property);
        }

    }

}