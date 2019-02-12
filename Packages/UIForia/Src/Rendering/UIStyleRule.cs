using UIForia.Parsing.Style;

namespace UIForia.Rendering {
    public struct UIStyleRule {
        
        /// <summary>
        /// true if this rule was preceded by a `not` keyword
        /// </summary>
        public readonly bool invert;
        
        /// <summary>
        /// the attribute name
        /// </summary>
        public readonly string attributeName;
        
        /// <summary>
        /// null == any value is ok
        /// </summary>
        public readonly string attributeValue;

        public readonly StyleAttributeExpression expression;

        public UIStyleRule(bool invert, string attributeName, string attributeValue, StyleAttributeExpression expression) {
            this.invert = invert;
            this.attributeName = attributeName;
            this.attributeValue = attributeValue;
            this.expression = expression;
        }

        public bool IsApplicableTo(UIElement element) {
            return expression?.Execute() == 0;
        }

        public static bool operator ==(UIStyleRule x, UIStyleRule y) {
            return x.invert == y.invert 
                   && string.Equals(x.attributeName, y.attributeName) 
                   && string.Equals(x.attributeValue, y.attributeValue);
        }

        public static bool operator !=(UIStyleRule x, UIStyleRule y) {
            return !(x == y);
        }
    }
}
