using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src {

    public class ExpressionCompiler {

        private readonly ContextDefinition context;

        public ExpressionCompiler(ContextDefinition context) {
            this.context = context;
        }

        public Expression Compile(ExpressionNode root) {
            return Visit(root);
        }

        private RootContextSimpleAccessExpression VisitRootContextAccessor(RootContextLookupNode node) {
            string fieldName = node.idNode.identifier;
            Type type = context.processedType.rawType;
            return new RootContextSimpleAccessExpression(type, fieldName);
        }

        private Expression Visit(ExpressionNode node) {
            switch (node.expressionType) {
                case ExpressionNodeType.RootContextAccessor:
                    return VisitRootContextAccessor((RootContextLookupNode) node);

                case ExpressionNodeType.LiteralValue:
                    return VisitConstant((LiteralValueNode)node);

                case ExpressionNodeType.Accessor:
                    return VisitAccessExpression(context, (AccessExpressionNode) node);

                case ExpressionNodeType.Unary:
                    return VisitUnaryExpression(context, (UnaryExpressionNode) node);

                case ExpressionNodeType.Operator:
                    return VisitOperatorExpression(context, (OperatorExpressionNode) node);
            }

            return null;
        }

        private Expression VisitOperatorExpression(ContextDefinition context, OperatorExpressionNode node) {
            throw new NotImplementedException();
        }

        private Expression VisitUnaryExpression(ContextDefinition context, UnaryExpressionNode node) {
            Type yieldType = node.expression.GetYieldedType(context);
            if (yieldType == typeof(bool)) {
                if (node.op == UnaryOperatorType.Not) {
                    return new UnaryBooleanExpression(Visit(node.expression));
                }

                throw new Exception("Unary but not boolean operator");
            }
            else if (IsNumericType(yieldType)) {
                switch (node.op) {
                    case UnaryOperatorType.Plus:
                        return new UnaryPlusEvaluator(Visit(node.expression));
                    case UnaryOperatorType.Minus:
                        return new UnaryMinusEvaluator(Visit(node.expression));
                }
            }
            else { }

            return null;
        }

        private static bool IsNumericType(Type type) {
            return type == typeof(int)
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

        private Expression VisitAccessExpression(ContextDefinition context, AccessExpressionNode node) {
//            string contextId = node.rootIdentifier;
//            for (int i = 0; i < node.parts.Count; i++) {
//                AccessExpressionPart part = node.parts[i];
//                if (part is ArrayAccessExpressionPart) {
//                    ArrayAccessExpressionPart arrayPart = (ArrayAccessExpressionPart) part;
//                }
//                else {
//                    PropertyAccessExpressionPart propertyPart = (PropertyAccessExpressionPart) part;
//                }
//            }

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

        private static Expression VisitConstant(LiteralValueNode node) {
            
            if (node is NumericLiteralNode) {
                return new NumericLiteralExpression(((NumericLiteralNode) node).value);
            }

            if (node is BooleanLiteralNode) {
                return new BooleanLiteralExpression(((BooleanLiteralNode) node).value);
            }

            if (node is StringLiteralNode) {
                return new StringLiteralExpression(((StringLiteralNode) node).value);
            }

            return null;
        }

        private static Expression VisitStringConstant(StringLiteralNode node) {
            return new StringLiteralExpression(node.value);
        }

        private static Expression VisitBooleanConstant(BooleanLiteralNode node) {
            return new BooleanLiteralExpression(node.value);
        }

        private static Expression VisitNumericConstant(NumericLiteralNode node) {
            return new NumericLiteralExpression(node.value);
        }

        private static bool EnsureCompatibleType(Type type1, Type type2) {
            return type1.IsAssignableFrom(type2);
        }

        private static bool EnsureHasField(Type type, string fieldName) {
            return type.GetField(fieldName) != null;
        }

    }

}