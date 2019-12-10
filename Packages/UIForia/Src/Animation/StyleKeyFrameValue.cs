using UIForia.Rendering;

namespace UIForia.Animation {

    public struct StyleKeyFrameValue {

        public readonly string rawExpression;
        public readonly StyleProperty styleProperty;
        
        public StyleKeyFrameValue(StyleProperty property) {
            this.styleProperty = property;
            this.rawExpression = null;
        }

        public bool IsCalculated => rawExpression != null;

        public StylePropertyId propertyId => styleProperty.propertyId;
        
        public static implicit operator StyleKeyFrameValue(StyleProperty property) {
            return new StyleKeyFrameValue(property);
        }

    }

}