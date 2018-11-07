using System;

namespace UIForia {

    public abstract class NumericLiteralNode : LiteralValueNode {

        
        protected NumericLiteralNode() : base(ExpressionNodeType.LiteralValue) {}

    }

    public class FloatLiteralNode : NumericLiteralNode {

        public readonly float value;

        public FloatLiteralNode(float value) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(float);
        }

    }
    
    public class IntLiteralNode : NumericLiteralNode {

        public readonly int value;

        public IntLiteralNode(int value) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(int);
        }

    }
    
    public class DoubleLiteralNode : NumericLiteralNode {

        public readonly double value;

        public DoubleLiteralNode(double value) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(double);
        }

    }
    
}