using System;

namespace Src {

    public class BindingGenerator {

        public static ExpressionBinding Generate(ContextDefinition context, ExpressionNode root) {
            return Visit(context, root);
        }

        public static void GenerateFromAttribute(ContextDefinition context, AttributeDefinition attr) {
            
        }

        private static ExpressionBinding Visit(ContextDefinition context, ExpressionNode node) {
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

        private static ExpressionBinding VisitOperatorExpression(ContextDefinition context, OperatorExpression node) {
            throw new NotImplementedException();
        }

        private static ExpressionBinding VisitUnaryExpression(ContextDefinition context, UnaryExpressionNode node) {
            Type yieldType = node.expression.GetYieldedType(context);
            if (yieldType == typeof(bool)) {
                if (node.op == UnaryOperatorType.Not) {
                    return new UnaryBooleanBinding(Visit(context, node.expression));
                }
                throw new Exception("Unary but not boolean operator");
            }
            else if (IsNumericType(yieldType)) {
                switch (node.op) {
                    case UnaryOperatorType.Plus:
                        return new UnaryPlusBinding(Visit(context, node.expression));
                    case UnaryOperatorType.Minus:
                        return new UnaryMinusBinding(Visit(context, node.expression));
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

        private static ExpressionBinding VisitAccessExpression(ContextDefinition context, AccessExpressionNode node) {

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

        private static ExpressionBinding VisitNumericConstant(NumericConstantNode node) {
            return new NumericConstantBinding(node.value);
        }

        private static bool EnsureCompatableType(Type type1, Type type2) {
            return type1.IsAssignableFrom(type2);
        }

        private static bool EnsureHasField(Type type, string fieldName) {
            return type.GetField(fieldName) != null;
        }

    }

}