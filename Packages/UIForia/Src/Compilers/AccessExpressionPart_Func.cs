using System;
using System.Collections.Generic;
using UIForia.Expressions;

namespace UIForia.Compilers {

    public class AccessExpressionPart_Func<TReturn, TInput, TNext, TArg0, TArg1, TArg2, TArg3> : AccessExpressionPart<TReturn, TInput> {

        protected readonly AccessExpressionPart<TReturn, TNext> next;

        public readonly int argCount;
        public readonly Expression<TArg0> arg0;
        public readonly Expression<TArg1> arg1;
        public readonly Expression<TArg2> arg2;
        public readonly Expression<TArg3> arg3;

        public AccessExpressionPart_Func(List<Expression> expressions, AccessExpressionPart<TReturn, TNext> next) {
            this.next = next;
            this.argCount = expressions.Count;
            this.arg0 = expressions.Count > 0 ? (Expression<TArg0>)expressions[0] : null;
            this.arg1 = expressions.Count > 1 ? (Expression<TArg1>)expressions[1] : null;
            this.arg2 = expressions.Count > 2 ? (Expression<TArg2>)expressions[2] : null;
            this.arg3 = expressions.Count > 3 ? (Expression<TArg3>)expressions[3] : null;
        }

        public override TReturn Execute(TInput previous, ExpressionContext context) {
            if (next != null) {
                TNext value = default;
                switch (argCount) {
                    case 0:
                        value = ((Func<TNext>) (object) previous).Invoke();
                        break;
                    case 1:
                        value = ((Func<TArg0, TNext>) (object) previous).Invoke(
                            arg0.Evaluate(context)
                        );
                        break;
                    case 2:
                        value = ((Func<TArg0, TArg1, TNext>) (object) previous).Invoke(
                            arg0.Evaluate(context),
                            arg1.Evaluate(context)
                        );
                        break;
                    case 3:
                        value = ((Func<TArg0, TArg1, TArg2, TNext>) (object) previous).Invoke(
                            arg0.Evaluate(context),
                            arg1.Evaluate(context),
                            arg2.Evaluate(context)
                        );
                        break;
                    case 4:
                        value = ((Func<TArg0, TArg1, TArg2, TArg3, TNext>) (object) previous).Invoke(
                            arg0.Evaluate(context),
                            arg1.Evaluate(context),
                            arg2.Evaluate(context),
                            arg3.Evaluate(context)
                        );
                        break;
                }

                // todo - dont box
                if (value == null) {
                    return default;
                }

                return next.Execute(value, context);
            }
            else {
                TReturn value = default;
                switch (argCount) {
                    case 0:
                        value = ((Func<TReturn>) (object) previous).Invoke();
                        break;
                    case 1:
                        value = ((Func<TArg0, TReturn>) (object) previous).Invoke(
                            arg0.Evaluate(context)
                        );
                        break;
                    case 2:
                        value = ((Func<TArg0, TArg1, TReturn>) (object) previous).Invoke(
                            arg0.Evaluate(context),
                            arg1.Evaluate(context)
                        );
                        break;
                    case 3:
                        value = ((Func<TArg0, TArg1, TArg2, TReturn>) (object) previous).Invoke(
                            arg0.Evaluate(context),
                            arg1.Evaluate(context),
                            arg2.Evaluate(context)
                        );
                        break;
                    case 4:
                        value = ((Func<TArg0, TArg1, TArg2, TArg3, TReturn>) (object) previous).Invoke(
                            arg0.Evaluate(context),
                            arg1.Evaluate(context),
                            arg2.Evaluate(context),
                            arg3.Evaluate(context)
                        );
                        break;
                }

                return value;
            }
        }

    }

}