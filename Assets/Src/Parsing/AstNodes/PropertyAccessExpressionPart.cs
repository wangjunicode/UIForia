namespace Src {

    public class PropertyAccessExpressionPart : AccessExpressionPart {

        public readonly string fieldName;

        public PropertyAccessExpressionPart(string fieldName) {
            this.fieldName = fieldName;
        }

    }

}