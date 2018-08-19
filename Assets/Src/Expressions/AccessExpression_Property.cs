using System;
using System.Collections;
using System.Reflection;

namespace Src {

    public class AccessExpression : Expression {

        public readonly string contextName;
        public readonly Type yieldedType;
        public readonly AccessExpressionPart[] parts;

        public AccessExpression(string contextName, Type yieldedType, AccessExpressionPart[] parts) {
            this.contextName = contextName;
            this.yieldedType = yieldedType;
            this.parts = parts;
        }

        public override Type YieldedType => yieldedType;

        public override bool IsConstant() {
            return false;
        }
        

        public override object Evaluate(ExpressionContext context) {
            object target = context.ResolveObjectAlias(contextName);
            object last = target;

            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return null;
                }
            }

            return last;
        }

    }

    public abstract class AccessExpressionPart {

        public abstract object Evaluate(object target, ExpressionContext context);

    }

    public class AccessExpressionPart_List : AccessExpressionPart {

        public readonly Expression<int> indexExpression;

        public AccessExpressionPart_List(Expression<int> indexExpression) {
            this.indexExpression = indexExpression;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            IList targetList = (IList) target;
            int index = indexExpression.EvaluateTyped(context);
            return targetList[index];
        }

    }

    public class AccessExpressionPart_Field : AccessExpressionPart {

        private Type cachedType;
        private FieldInfo cachedFieldInfo;
        public readonly string fieldName;

        public AccessExpressionPart_Field(string fieldName) {
            this.fieldName = fieldName;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            if (target == null) return null;
            Type targetType = target.GetType();
            if (targetType != cachedType || cachedFieldInfo == null) {
                cachedType = targetType;
                cachedFieldInfo = targetType.GetField(fieldName, ReflectionUtil.InstanceBindFlags);
                if (cachedFieldInfo == null) {
                    throw new Exception($"Field {fieldName} does not exist on type {targetType}");
                }
            }
            return cachedFieldInfo.GetValue(target);
        }

    }

}