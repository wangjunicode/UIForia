using System.Collections;

namespace Src {

    public class ArrayAccessorEvaluator : ExpressionEvaluator {

        public readonly ExpressionEvaluator ExpressionEvaluator;

        public ArrayAccessorEvaluator(ExpressionEvaluator expressionEvaluator) {
            this.ExpressionEvaluator = expressionEvaluator;
        }

        public object Evaluate(TemplateContext context, IList list) {
            int idx = (int) ExpressionEvaluator.Evaluate(context);
            if ((uint) idx >= (uint) list.Count) {
                return null;
            }
            return list[idx];
        }

    }

}