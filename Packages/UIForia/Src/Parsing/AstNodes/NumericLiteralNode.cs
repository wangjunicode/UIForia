using System;

namespace UIForia {

    public abstract class NumericLiteralNodeOld : LiteralValueNodeOld {

        
        protected NumericLiteralNodeOld() : base(ExpressionNodeType.LiteralValue) {}

    }

    public class FloatLiteralNodeOld : NumericLiteralNodeOld {

        public readonly float value;

        public FloatLiteralNodeOld(float value) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(float);
        }

    }
    
    public class IntLiteralNodeOld : NumericLiteralNodeOld {

        public readonly int value;

        public IntLiteralNodeOld(int value) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(int);
        }

    }
    
    public class DoubleLiteralNodeOld : NumericLiteralNodeOld {

        public readonly double value;

        public DoubleLiteralNodeOld(double value) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(double);
        }

    }
    
}