using System;
using System.Collections.Generic;
using UIForia.Routing;

namespace UIForia.Compilers {

    public class RouteResolver : ExpressionAliasResolver {

        public RouteResolver(string aliasName) : base(aliasName) { }

        private static readonly UrlReaderExpression s_UrlReaderExpression = new UrlReaderExpression();

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            if (nodeOld.expressionType == ExpressionNodeType.Accessor) {
                AccessExpressionNodeOld accessExpressionNodeOld = (AccessExpressionNodeOld) nodeOld;
                List<AccessExpressionPartNodeOld> parts = accessExpressionNodeOld.parts;
             

                if (parts.Count == 1 && parts[0] is PropertyAccessExpressionPartNodeOld field) {
                    if (field.fieldName == "url") {
                        return s_UrlReaderExpression;
                    }
                    
                }

                if (parts.Count == 2) {
                    PropertyAccessExpressionPartNodeOld paramPart = parts[0] as PropertyAccessExpressionPartNodeOld;
                    if (paramPart == null || (paramPart.fieldName != "query" && paramPart.fieldName != "param")) {
                        throw new ParseException("Expected $route.xxx to be url, param, or query, was " + paramPart?.fieldName);
                    }

                    if (paramPart.fieldName == "param") {
                        PropertyAccessExpressionPartNodeOld argPart = parts[1] as PropertyAccessExpressionPartNodeOld;
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
            
            public override string Evaluate(ExpressionContext context) {
                UIElement element  = (UIElement) context.currentObject;
                return element.GetRouteParameter(parameterName);
            }

            public override bool IsConstant() {
                return false;
            }

        }
        
        public class UrlReaderExpression : Expression<string> {

            public override Type YieldedType => typeof(string);
           
            public override string Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.Router.CurrentUrl;
            }

            public override bool IsConstant() {
                return false;
            }

        }

        public class RouteResolverExpression : Expression<IRouterElement> {

            public override Type YieldedType => typeof(Router);         

            public override IRouterElement Evaluate(ExpressionContext context) {
                return ((UIElement) context.currentObject).GetNearestRouter();
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}