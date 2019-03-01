using System;

namespace UIForia.Expressions {

    public class ArrayLiteralExpression<T> : Expression<T[]> {

        // cached return value so subsequent runs don't return new arrays
        // this binding expects to be cloned so we always create the return array only once

        public readonly T[] retn;
        public readonly Expression<T>[] list;

        public ArrayLiteralExpression(Expression<T>[] list) {
            this.retn = new T[list.Length];
            this.list = list;
        }

        public override Type YieldedType => typeof(T[]);

        public override T[] Evaluate(ExpressionContext ctx) {
            for (int i = 0; i < list.Length; i++) {
                retn[i] = list[i].Evaluate(ctx);
            }

            return retn;
        }

        public override bool IsConstant() {
            if (list == null) {
                return true;
            }

            for (int i = 0; i < list.Length; i++) {
                if (!list[i].IsConstant()) {
                    return false;
                }
            }

            return true;
        }

        public ArrayLiteralExpression<T> Clone() {
            return new ArrayLiteralExpression<T>(list);
        }

    }

}