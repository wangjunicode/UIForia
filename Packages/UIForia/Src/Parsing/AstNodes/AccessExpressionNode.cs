using System;
using System.Collections.Generic;
using System.Reflection;

namespace UIForia {

    public class AccessExpressionNodeOld : ExpressionNodeOld {

        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public readonly IdentifierNodeOld identifierNodeOld;
        public readonly List<AccessExpressionPartNodeOld> parts;

        public AccessExpressionNodeOld(IdentifierNodeOld identifierNodeOld, List<AccessExpressionPartNodeOld> accessExpressionParts)
            : base(ExpressionNodeType.Accessor) {
            this.identifierNodeOld = identifierNodeOld;
            this.parts = accessExpressionParts;
        }

        // if is array access -> return currentType.GetElementType()
        // if is property access -> return currentType.GetField("identifier").FieldType
        public override Type GetYieldedType(ContextDefinition context) {
            Type partType = ReflectionUtil.ResolveFieldOrPropertyType(context.rootType, identifierNodeOld.identifier);
//            context.ResolveRuntimeAliasType(identifierNode.identifier);

            if (partType == null) {
                throw new Exception($"Unable to resolve '{identifierNodeOld.identifier}'");
            }
            // todo handle dotting into static types and enums
            
            for (int i = 0; i < parts.Count; i++) {
                if (parts[i] is PropertyAccessExpressionPartNodeOld) {
                    partType = GetFieldType(partType, (PropertyAccessExpressionPartNodeOld)parts[i]);
                }
                else if (parts[i] is ArrayAccessExpressionNodeOld) {
                    partType = partType.GetElementType();
                }
                if (partType == null) {
                    throw new Exception("Bad Access expression");
                }
                
                // todo -- method call as part of access chain
                
            }

            return partType;
        }

        private static Type GetFieldType(Type targetType, PropertyAccessExpressionPartNodeOld nodeOld) {
            FieldInfo fieldInfo = targetType.GetField(nodeOld.fieldName, flags);
            if (fieldInfo == null) {
                PropertyInfo propertyInfo = targetType.GetProperty(nodeOld.fieldName, flags);
                if (propertyInfo != null) {
                    return propertyInfo.PropertyType;
                }

                Type nestedType = targetType.GetNestedType(nodeOld.fieldName, flags);
                if (nestedType != null) {
                    return nestedType;
                }
                throw new FieldNotDefinedException(targetType, nodeOld.fieldName);
            }
            return fieldInfo.FieldType;
        }

    }

}

