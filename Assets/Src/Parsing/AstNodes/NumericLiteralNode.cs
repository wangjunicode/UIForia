using System;

namespace Src {

    public class NumericLiteralNode : LiteralValueNode {

        public readonly double value;
        
        public NumericLiteralNode(double value) : base(ExpressionNodeType.LiteralValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(double);
        }

    }

}