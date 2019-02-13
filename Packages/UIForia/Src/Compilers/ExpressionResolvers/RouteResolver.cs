using System;

namespace UIForia.Compilers {

    public class RouteParameterResolver : ExpressionAliasResolver {

        public RouteParameterResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsDotExpression(CompilerContext context, string propertyName) {
            return new ParameterReaderExpression(propertyName);
        }

        public class ParameterReaderExpression : Expression<string> {

            public readonly string parameterName;

            public ParameterReaderExpression(string parameterName) {
                this.parameterName = parameterName;
            }

            public override Type YieldedType => typeof(string);

            public override string Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.GetRouteParameter(parameterName);
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

    public class RouteResolver : ExpressionAliasResolver {

        public RouteResolver(string aliasName) : base(aliasName) { }

        private static readonly UrlReaderExpression s_UrlReaderExpression = new UrlReaderExpression();

        public override Expression CompileAsDotExpression(CompilerContext context, string propertyName) {
            if (propertyName == "url") {
                return s_UrlReaderExpression;
            }

            return null;
        }

        public class UrlReaderExpression : Expression<string> {

            public override Type YieldedType => typeof(string);

            public override string Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.Application.RoutingSystem.FindRouterInHierarchy(element).CurrentUrl;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}