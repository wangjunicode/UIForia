using System;
using System.Collections.Generic;
using UIForia.Routing;

namespace UIForia.Compilers {

    public class RouteResolver : ExpressionAliasResolver {

        public RouteResolver(string aliasName) : base(aliasName) { }

        private static readonly UrlReaderExpression s_UrlReaderExpression = new UrlReaderExpression();

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.Accessor) {
                AccessExpressionNode accessExpressionNode = (AccessExpressionNode) node;
                List<AccessExpressionPartNode> parts = accessExpressionNode.parts;
             

                if (parts.Count == 1 && parts[0] is PropertyAccessExpressionPartNode field) {
                    if (field.fieldName == "url") {
                        return s_UrlReaderExpression;
                    }
                    
                }

                if (parts.Count == 2) {
                    PropertyAccessExpressionPartNode paramPart = parts[0] as PropertyAccessExpressionPartNode;
                    if (paramPart == null || (paramPart.fieldName != "query" && paramPart.fieldName != "param")) {
                        throw new ParseException("Expected $route.xxx to be url, param, or query, was " + paramPart?.fieldName);
                    }

                    if (paramPart.fieldName == "param") {
                        PropertyAccessExpressionPartNode argPart = parts[1] as PropertyAccessExpressionPartNode;
                        if (argPart == null) {
                            return null;
                        }

                        string argName = argPart.fieldName;
                        return new ParameterReaderExpression(argName);
                    }

                    if (paramPart.fieldName == "query") {
                        
                    }
                    
                }
                
            }

            return null;
        }

        public class ParameterReaderExpression : Expression<string> {

            public readonly string parameterName;

            public ParameterReaderExpression(string parameterName) {
                this.parameterName = parameterName;
            }
            
            public override Type YieldedType => typeof(string);
            
            public override object Evaluate(ExpressionContext context) {
                UIElement element  = (UIElement) context.currentObject;
                return element.GetRouteParameter(parameterName);
            }

            public override string EvaluateTyped(ExpressionContext context) {
                UIElement element  = (UIElement) context.currentObject;
                return element.GetRouteParameter(parameterName);
            }

            public override bool IsConstant() {
                return false;
            }

        }
        
        public class UrlReaderExpression : Expression<string> {

            public override Type YieldedType => typeof(string);

            public override object Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.Router.CurrentUrl;
            }

            public override string EvaluateTyped(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.Router.CurrentUrl;
            }

            public override bool IsConstant() {
                return false;
            }

        }

        public class RouteResolverExpression : Expression<IRouterElement> {

            public override Type YieldedType => typeof(Router);

            public override object Evaluate(ExpressionContext context) {
                return ((UIElement) context.currentObject).GetNearestRouter();
            }

            public override IRouterElement EvaluateTyped(ExpressionContext context) {
                return ((UIElement) context.currentObject).GetNearestRouter();
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}