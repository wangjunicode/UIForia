namespace UIForia.Compilers {

    public class AccessExpressionPart_Method<TReturn, TInput, TNext> : AccessExpressionPart<TReturn, TInput> {

        protected bool isStatic;
        protected readonly AccessExpressionPart<TReturn, TNext> next;
        protected readonly bool previousCanBeNull;
        public Expression<TNext> methodExpression;
        public Expression<TReturn> terminalMethodExpression;

        public AccessExpressionPart_Method(Expression expression, AccessExpressionPart<TReturn, TNext> next) {
            this.next = next;
            this.previousCanBeNull = typeof(TInput).IsClass;
            if (next == null) {
                terminalMethodExpression = (Expression<TReturn>) expression;
            }
            else {
                methodExpression = (Expression<TNext>) expression;
            }
        }

        // todo -- remove this boxing for aux
        public override TReturn Execute(TInput previous, ExpressionContext context) {
            if (next != null) {
                object previousAux = context.aux;
                context.aux = previous;
                TNext value = methodExpression.Evaluate(context);
                context.aux = previousAux;
                if (!isStatic && previousCanBeNull && ReferenceEquals(value, null)) {
                    return default;
                }

                return next.Execute(value, context);
            }
            else {
                if (!isStatic && previousCanBeNull && ReferenceEquals(previous, null)) {
                    return default;
                }

                object previousAux = context.aux;
                context.aux = previous;
                TReturn retn = terminalMethodExpression.Evaluate(context);
                context.aux = previousAux;
                return retn;
            }
        }

    }

}