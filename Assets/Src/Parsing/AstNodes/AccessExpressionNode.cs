using System;
using System.Collections.Generic;
using System.Reflection;

namespace Src {

    public class AccessExpressionNode : ExpressionNode {

        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public readonly IdentifierNode identifierNode;
        public readonly List<AccessExpressionPartNode> parts;

        public AccessExpressionNode(IdentifierNode identifierNode, List<AccessExpressionPartNode> accessExpressionParts)
            : base(ExpressionNodeType.Accessor) {
            this.identifierNode = identifierNode;
            this.parts = accessExpressionParts;
        }

        // if is array access -> return currentType.GetElementType()
        // if is property access -> return currentType.GetField("identifier").FieldType
        public override Type GetYieldedType(ContextDefinition context) {
            Type partType = context.ResolveRuntimeAliasType(identifierNode.identifier);

            for (int i = 0; i < parts.Count; i++) {
                if (parts[i] is PropertyAccessExpressionPartNode) {
                    partType = GetFieldType(partType, (PropertyAccessExpressionPartNode)parts[i]);
                }
                else if (parts[i] is ArrayAccessExpressionNode) {
                    partType = partType.GetElementType();
                }
                if (partType == null) {
                    throw new Exception("Bad Access expression");
                }
                
                // todo -- method call as part of access chain
                
        
            }

            return partType;
        }

        private static Type GetFieldType(Type targetType, PropertyAccessExpressionPartNode node) {
            FieldInfo fieldInfo = targetType.GetField(node.fieldName, flags);
            if (fieldInfo == null) {
                PropertyInfo propertyInfo = targetType.GetProperty(node.fieldName, flags);
                if (propertyInfo != null) {
                    return propertyInfo.PropertyType;
                }
                throw new FieldNotDefinedException(targetType, node.fieldName);
            }
            return fieldInfo.FieldType;
        }

    }

}