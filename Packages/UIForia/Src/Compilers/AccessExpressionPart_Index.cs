using System.Collections.Generic;

namespace UIForia.Compilers {

    public class AccessExpressionPart_Index<TReturn, TInput, TNext> : AccessExpressionPart<TReturn, TInput> {

        protected readonly Expression<int> expression;
        protected readonly AccessExpressionPart<TReturn, TNext> next;

        public AccessExpressionPart_Index(AccessExpressionPart<TReturn, TNext> next, Expression<int> expression) {
            this.next = next;
            this.expression = expression;
        }

        public override TReturn Execute(TInput previous, ExpressionContext context) {
            if (next == null) {
                if (previous == null) {
                    return default;
                }

                int index = expression.Evaluate(context);
                IList<TReturn> target = (IList<TReturn>) previous;
                if (index < target.Count) {
                    return target[index];
                }

                return default;
            }
            else {
                if (previous == null) {
                    return default;
                }

                int index = expression.Evaluate(context);
                IList<TNext> target = (IList<TNext>) previous;
                if (index < target.Count) {
                    return next.Execute(target[index], context);
                }

                return default;
            }
        }

    }

}