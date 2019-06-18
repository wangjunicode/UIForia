using System;
using UIForia.Expressions;

namespace UIForia.Compilers {

    public class AccessExpressionPart_FieldProperty<TReturn, TInput, TNext> : AccessExpressionPart<TReturn, TInput> {

        protected Func<TInput, TNext> getter;
        protected Func<TInput, TReturn> terminalGetter;
        protected readonly AccessExpressionPart<TReturn, TNext> next;
        protected readonly bool previousCanBeNull;
        protected bool isStatic;

        protected AccessExpressionPart_FieldProperty(AccessExpressionPart<TReturn, TNext> next) {
            this.next = next;
            this.previousCanBeNull = typeof(TInput).IsClass;
        }

        public override TReturn Execute(TInput previous, ExpressionContext context) {
            if (next != null) {
                TNext value = getter(previous);
                if (!isStatic && previousCanBeNull && ReferenceEquals(value, null)) {
                    return default;
                }

                return next.Execute(value, context);
            }

            // check should avoid boxing 
            if (!isStatic && previousCanBeNull && ReferenceEquals(previous, null)) {
                return default;
            }

            if (previous == null) {
                return default;
            }

            return terminalGetter(previous);
        }

    }

}