using System;

namespace Src {

    public class NumericConstantNode : ConstantValueNode {

        public readonly double value;
        
        public NumericConstantNode(double value) : base(ExpressionNodeType.ConstantValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(double);
        }

    }

}