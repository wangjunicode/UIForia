using System;

namespace UIForia {

    public class BooleanLiteralNode : LiteralValueNode {

        public readonly bool value;
        
        public BooleanLiteralNode(bool value) : base(ExpressionNodeType.LiteralValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(bool);
        }

    }

}