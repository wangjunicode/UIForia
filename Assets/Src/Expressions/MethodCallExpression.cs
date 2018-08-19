using System;
using System.Reflection;

namespace Src {

    public class MethodCallExpression : Expression {

        public readonly object[] argumentResults;
        public readonly Expression[] argumentExpressions;
        public readonly MethodInfo methodInfo;
        
        public MethodCallExpression(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.methodInfo = methodInfo;
            this.argumentExpressions = argumentExpressions;
            argumentResults = new object[argumentExpressions.Length];
        }

        public override Type YieldedType => methodInfo.ReturnType;
        
        public override object Evaluate(ExpressionContext context) {
            for (int i = 0; i < argumentExpressions.Length; i++) {
                argumentResults[i] = argumentExpressions[i].Evaluate(context);
            }
            if (methodInfo.IsStatic) {
                return methodInfo.Invoke(null, argumentResults);
            }
            else {
                return methodInfo.Invoke(context.rootContext, argumentResults);
            }
        }

        public override bool IsConstant() {
            return false;
        }

    }

}