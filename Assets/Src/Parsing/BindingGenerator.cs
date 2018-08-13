using System;

namespace Src {

    // bindings have 2 parts
    // 1. Evaluate some expression and return a value
    // 2. Do something with that value. This can be anything
    //    and is usually defined by 
    
    public static class BindingGenerator {

        public static ExpressionEvaluator Generate(ContextDefinition context, ExpressionNode root) {
            return Visit(context, root);
        }

        public static Binding GenerateFromAttribute(ContextDefinition context, AttributeDefinition attr) {
            return null;
        }

        private static ExpressionEvaluator Visit(ContextDefinition context, ExpressionNode node) {
            switch (node.expressionType) {

                case ExpressionNodeType.ConstantValue:
                    return VisitNumericConstant((NumericConstantNode) node);

                case ExpressionNodeType.Accessor:
                    return VisitAccessExpression(context, (AccessExpressionNode) node);

                case ExpressionNodeType.Unary:
                    return VisitUnaryExpression(context, (UnaryExpressionNode) node);

                case ExpressionNodeType.Operator:
                    return VisitOperatorExpression(context, (OperatorExpression) node);

            }

            return null;
        }

        private static ExpressionEvaluator VisitOperatorExpression(ContextDefinition context, OperatorExpression node) {
            throw new NotImplementedException();
        }

        private static ExpressionEvaluator VisitUnaryExpression(ContextDefinition context, UnaryExpressionNode node) {
            Type yieldType = node.expression.GetYieldedType(context);
            if (yieldType == typeof(bool)) {
                if (node.op == UnaryOperatorType.Not) {
                    return new UnaryBooleanEvaluator(Visit(context, node.expression));
                }
                throw new Exception("Unary but not boolean operator");
            }
            else if (IsNumericType(yieldType)) {
                switch (node.op) {
                    case UnaryOperatorType.Plus:
                        return new UnaryPlusEvaluator(Visit(context, node.expression));
                    case UnaryOperatorType.Minus:
                        return new UnaryMinusEvaluator(Visit(context, node.expression));
                }
            }
            else { }
            return null;
        }

        private static bool IsNumericType(Type type) {
            return type    == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(byte)
                   || type == typeof(sbyte)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(float)
                   || type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(decimal);
        }

        private static ExpressionEvaluator VisitAccessExpression(ContextDefinition context, AccessExpressionNode node) {

            string contextId = node.rootIdentifier;
            for (int i = 0; i < node.parts.Count; i++) {
                AccessExpressionPart part = node.parts[i];
                if (part is ArrayAccessExpressionPart) {
                    ArrayAccessExpressionPart arrayPart = (ArrayAccessExpressionPart) part;
                }
                else {
                    PropertyAccessExpressionPart propertyPart = (PropertyAccessExpressionPart) part;

                }
            }

            //TypeCheck here
            //node.TypeCheck(context);
            //  PropertyAccessBinding binding = new PropertyAccessBinding();
            //  return binding;
            // thing.item[$i].values[$j].x;
            // propertyLookup
            // -> ArrayLookup
            // -> (iterator lookup)
            // -> property access
            // -> array access
            // -> (iterator lookup)
            // -> property access
            return null;
        }

        private static ExpressionEvaluator VisitNumericConstant(NumericConstantNode node) {
            return new NumericConstantEvaluator(node.value);
        }

        private static bool EnsureCompatableType(Type type1, Type type2) {
            return type1.IsAssignableFrom(type2);
        }

        private static bool EnsureHasField(Type type, string fieldName) {
            return type.GetField(fieldName) != null;
        }

    }

}