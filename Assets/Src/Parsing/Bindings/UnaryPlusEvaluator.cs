using System;

namespace Src {

    public class UnaryPlusEvaluator : ExpressionEvaluator {

        public readonly ExpressionEvaluator Evaluator;

        public UnaryPlusEvaluator(ExpressionEvaluator evaluator) {
            this.Evaluator = evaluator;
        }

        public override object Evaluate(TemplateContext context) {
            object value = Evaluator.Evaluate(context);
            if (value is int) return +(int) value;
            if (value is float) return +(float) value;
            if (value is double) return +(double) value;
            if (value is short) return +(short) value;
            if (value is ushort) return +(ushort) value;
            if (value is byte) return +(byte) value;
            if (value is sbyte) return +(sbyte) value;
            if (value is long) return +(long) value;
            if (value is ulong) return +(ulong) value;
            throw new Exception("Failed to match numeric type");
        }

    }

}