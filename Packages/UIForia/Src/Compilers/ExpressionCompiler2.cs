using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct TypeRestrictedAlias {

        public Type type;
        public ExpressionAliasResolver resolver;

    }
    
    // $texture-url("/")
    
    public class ExpressionCompiler2 {

        public object rootValue;
        private Type targetType;
        private LightList<string> namespaces;

        private LightList<TypeRestrictedAlias> restrictedAliases;
        
        public Expression<T> Compile<T>(string input) {
            ASTNode astRoot = Parser2.Parse(input);
            return default;
        }

        private Expression Visit(ASTNode node) {
            switch (node.type) {
                case ASTNodeType.NullLiteral:
                    return new ConstantExpression<Terminal>(null);
                
                case ASTNodeType.BooleanLiteral:
                    return new ConstantExpression<bool>(true);
                
                case ASTNodeType.NumericLiteral:
                    break;
                
                case ASTNodeType.DefaultLiteral:
                    break;
                
                case ASTNodeType.StringLiteral:
                    break;
                
                case ASTNodeType.Operator:
                    break;
                
                case ASTNodeType.TypeOf:
                    break;
                
                case ASTNodeType.Alias:
                    break;
                
                case ASTNodeType.Identifier:
                    break;
                
                case ASTNodeType.Invalid:
                    break;
                
                case ASTNodeType.MethodCall:
                    break;
                
                case ASTNodeType.DotAccess:
                    break;
                
                case ASTNodeType.AccessExpression:
                    return VisitAccessExpression((MemberAccessExpressionNode) node);

                case ASTNodeType.IndexExpression:
                    break;
                
                case ASTNodeType.UnaryNot:
                    break;
                
                case ASTNodeType.UnaryMinus:
                    break;
                
                case ASTNodeType.DirectCast:
                    break;
                
                case ASTNodeType.TypePath:
                    break;
                
                case ASTNodeType.ListInitializer:
                    break;
                
                case ASTNodeType.New:
                    break;
                
                case ASTNodeType.Paren:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public enum AccessExpressionPartType {

            Field,
            Property,
            List,
            ActionT,
            ActionTU,
            ActionTUV,
            FuncT,
            FuncTU,
            FuncTUV,
            StaticField,
            StaticProperty,
            ConstantValue

        }

        public struct AccessExpressionPart {

            public AccessExpressionPartType type;
            public object data;
            public object data1;
            public object data2;
            public Type outputType;

        }

        public class AccessExpression<T, U, V, W, X> : Expression<T> {

            public override Type YieldedType => typeof(T);
            public readonly LightList<AccessExpressionPart> parts;

            // for each type, if we know how to cast to the next type, apply the cast operator
            
            public override object Evaluate(ExpressionContext context) {
                object target = context.rootObject;

                for (int i = 0; i < parts.Count; i++) {
                    Type targetType = target.GetType();
                    AccessExpressionPart part = parts[i];

                    switch (part.type) {
                        case AccessExpressionPartType.Field: {
                            Type cachedType = (Type) part.data;
                            FieldInfo cachedFieldInfo = (FieldInfo) part.data1;
                            
                            if (targetType != cachedType) {
                                cachedType = targetType;
                                cachedFieldInfo = targetType.GetField((string) part.data2, ReflectionUtil.InstanceBindFlags);
                                if (cachedFieldInfo == null) {
                                    throw new Exception($"Field {(string) part.data2} does not exist on type {targetType}");
                                }
                            }

                            target = cachedFieldInfo.GetValue(target);
                        }
                            break;
                        case AccessExpressionPartType.Property: {
                            Type cachedType = (Type) part.data;
                            PropertyInfo cachedPropertyInfo = (PropertyInfo) part.data1;

                            if (targetType != cachedType) {
                                cachedType = targetType;
                                cachedPropertyInfo = targetType.GetProperty((string) part.data2, ReflectionUtil.InstanceBindFlags);
                                if (cachedPropertyInfo == null) {
                                    throw new Exception($"Property {(string) part.data2} does not exist on type {targetType}");
                                }
                            }

                            target = cachedPropertyInfo.GetValue(target);
                        }
                            break;
                        
                        case AccessExpressionPartType.ConstantValue:
                            target = part.data;
                            break;
                        
                        case AccessExpressionPartType.StaticField:
                            target = ((FieldInfo) part.data).GetValue(null);
                            break;
                        
                        case AccessExpressionPartType.StaticProperty:
                            target = ((PropertyInfo) part.data).GetValue(null);
                            break;
                        
                        case AccessExpressionPartType.List:
                         //   target = ((IndexExpression) part.data).EvaluateTyped(context);
                            break;
                        
                        case AccessExpressionPartType.ActionT:
                           
                        
                        case AccessExpressionPartType.ActionTU:
                            break;
                        
                        case AccessExpressionPartType.ActionTUV:
                            Action<T, U, V> action = (Action<T, U, V>) target;
                            action.Invoke(
                                ((Expression<T>)part.data).EvaluateTyped(context),
                                ((Expression<U>)part.data1).EvaluateTyped(context),
                                ((Expression<V>)part.data2).EvaluateTyped(context)
                            );
                            return null;
                        
                        case AccessExpressionPartType.FuncT:
                            break;
                        case AccessExpressionPartType.FuncTU:
                            break;
                        case AccessExpressionPartType.FuncTUV:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (target == null) {
                        return default(T);
                    }
                    
                }

                return target;
            }

            public override T EvaluateTyped(ExpressionContext context) {
                throw new NotImplementedException();
            }

            public override bool IsConstant() {
                return false;
            }

        }

        // something.$alias();
        // $alias($alias());
        // if identifier is a field / property / event? -> use it
        // if identifier is a known type use that and treat as static / enum 

        // this.something.something.x
        // exp<T>(expr<Something>(this, expr<Something>())
        public class AccessExpressionPart2<T, U> {

            public AccessExpressionPart2<T, U> next;

            public T Execute(object target, ExpressionContext context) {
                FieldInfo fieldInfo = null;
                object val = fieldInfo.GetValue(target);
                return next != null ? next.Execute(val, context) : (T)val;
            }

        }
        
        private Expression VisitAccessExpression(MemberAccessExpressionNode node) {
            if (node.identifier[0] == '$') {
                // alias
                // return aliasResolvers.Find(node.identifier).Resolve(node, Visit);
            }
            else {
                Type accessedType = rootValue.GetType().GetField(node.identifier).FieldType;

                for (int i = 0; i < node.parts.Count; i++) {
                    ASTNodeType partType = node.parts[i].type;
                    // input type
                    // output type
                    
                    switch (partType) {
                        case ASTNodeType.DotAccess:

                            break;
                        case ASTNodeType.IndexExpression:
                            break;
                        case ASTNodeType.MethodCall:
                            break;
                    }
                }
            }

            return null;
        }

        public void SetRoot(object root) {
            this.rootValue = root;
        }

    }

}