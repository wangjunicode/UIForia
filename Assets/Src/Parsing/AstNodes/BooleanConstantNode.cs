using System;

namespace Src {

    public class BooleanConstantNode : ConstantValueNode {

        public readonly bool value;
        
        public BooleanConstantNode(bool value) : base(ExpressionNodeType.ConstantValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(bool);
        }

    }

}