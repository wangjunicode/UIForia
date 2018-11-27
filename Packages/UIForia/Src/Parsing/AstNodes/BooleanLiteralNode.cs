using System;

namespace UIForia {

    public class BooleanLiteralNodeOld : LiteralValueNodeOld {

        public readonly bool value;
        
        public BooleanLiteralNodeOld(bool value) : base(ExpressionNodeType.LiteralValue) {
            this.value = value;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(bool);
        }

    }

}