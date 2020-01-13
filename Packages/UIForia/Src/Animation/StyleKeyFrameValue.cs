using UIForia.Rendering;

namespace UIForia.Animation {

    public struct StyleKeyFrameValue {

        public readonly string rawExpression;
        public readonly StyleProperty styleProperty;
        public IRunCommand runCommand;

        public StyleKeyFrameValue(IRunCommand runCommand) {
            this.runCommand = runCommand;
            this.rawExpression = null;
            this.styleProperty = default;
        }

        public StyleKeyFrameValue(StylePropertyId propertyId, string rawExpression) {
            this.rawExpression = rawExpression;
            this.styleProperty = new StyleProperty(propertyId);
            this.runCommand = default;
        }

        
        public StyleKeyFrameValue(StyleProperty property) {
            this.styleProperty = property;
            this.rawExpression = null;
            this.runCommand = default;
        }

        public bool IsCalculated => rawExpression != null;

        public StylePropertyId propertyId => styleProperty.propertyId;
        
        public static implicit operator StyleKeyFrameValue(StyleProperty property) {
            return new StyleKeyFrameValue(property);
        }

    }

}