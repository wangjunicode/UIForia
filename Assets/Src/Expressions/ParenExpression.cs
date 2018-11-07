using System;

namespace UIForia {

    public static class ParenExpressionFactory {

        public static Expression CreateParenExpression(Expression expression) {
            Type type = expression.YieldedType;
            Type openType = typeof(ParenExpression<>);
            ReflectionUtil.ObjectArray1[0] = expression;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(openType, type, ReflectionUtil.ObjectArray1);
        }

    }

    public class ParenExpression<T> : Expression<T> {

        public readonly Expression<T> expression;

        public ParenExpression(Expression<T> expression) {
            this.expression = expression;
        }
        
        public override Type YieldedType => expression.YieldedType;

        public override object Evaluate(ExpressionContext context) {
            return expression.Evaluate(context);
        }
        
        public override T EvaluateTyped(ExpressionContext context) {
            return expression.EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}