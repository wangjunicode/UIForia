using System;

namespace Src {

    public class StringLiteralNode : LiteralValueNode {

        public readonly string value;
        
        public StringLiteralNode(string value) : base(ExpressionNodeType.LiteralValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(string);
        }

    }

}