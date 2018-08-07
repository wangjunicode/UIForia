using System;

namespace Src {

    public class StringConstantNode : ConstantValueNode {

        public readonly string value;
        
        public StringConstantNode(string value) : base(ExpressionNodeType.ConstantValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(string);
        }

    }

}