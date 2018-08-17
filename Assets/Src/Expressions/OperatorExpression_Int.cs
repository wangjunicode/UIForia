using System;

namespace Src {

    public class OperatorExpression_Int : Expression {

        private readonly Expression left;
        private readonly Expression right;
        private readonly OperatorType operatorType;

        public OperatorExpression_Int(OperatorType operatorType, Expression left, Expression right) {
            this.left = left;
            this.right = right;
            this.operatorType = operatorType;
        }

        public override object Evaluate(TemplateContext context) {
            int leftInt = (int) left.Evaluate(context);
            int rightInt = (int) right.Evaluate(context);
            switch (operatorType) {

                case OperatorType.Plus:
                    return leftInt + rightInt;

                case OperatorType.Minus:
                    return leftInt + rightInt;

                case OperatorType.Times:
                    return leftInt * rightInt;

                case OperatorType.Divide:
                    return leftInt / rightInt;

                case OperatorType.Mod:
                    return leftInt % rightInt;
                
            }
            throw new Exception("Invalid operator type");
        }

    }

}