using System;

namespace Src {

    public class OperatorEvaluatorInt : Expression {

        public Expression left;
        public Expression right;
        public OperatorType operatorType;

        public override object Evaluate(TemplateContext context) {
            throw new NotImplementedException();
        }

    }

}