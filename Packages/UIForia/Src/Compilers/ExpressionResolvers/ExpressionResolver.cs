using System;
using System.Collections.Generic;
using UIForia.Routing;

namespace UIForia.Compilers {

    public abstract class ExpressionAliasResolver {

        public string aliasName;

        public ExpressionAliasResolver(string aliasName) {
            if (aliasName[0] != '$') {
                aliasName = "$" + aliasName;
            }

            this.aliasName = aliasName;
        }

        public virtual bool CanCompile(ExpressionNode node) {
            return true;
        }

        public abstract Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit);

    }


    public class ElementResolver : ExpressionAliasResolver {

        public ElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIElementExpression<UIElement> s_Expression = new UIElementExpression<UIElement>();

        public override Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.Accessor) {
                return s_Expression;
            }

            return null;
        }

        public class UIElementExpression<T> : Expression<T> {

            public override Type YieldedType => typeof(T);

            public override object Evaluate(ExpressionContext context) {
                return context.current;
            }

            public override T EvaluateTyped(ExpressionContext context) {
                return (T) context.current;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

    public class ParentElementResolver : ExpressionAliasResolver {

        public ParentElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIParentElementExpression<UIElement> s_Expression = new UIParentElementExpression<UIElement>();

        public override Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.Accessor) {
                return s_Expression;
            }

            if (node.expressionType == ExpressionNodeType.AliasAccessor) { }

            return null;
        }

        public class UIParentElementExpression<T> : Expression<T> where T : UIElement {

            public override Type YieldedType => typeof(T);

            public override object Evaluate(ExpressionContext context) {
                return ((UIElement) context.current).parent;
            }

            public override T EvaluateTyped(ExpressionContext context) {
                return (T) ((UIElement) context.current).parent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

    public class RouteResolver : ExpressionAliasResolver {

        public RouteResolver(string aliasName) : base(aliasName) { }

        private static readonly UrlReaderExpression s_UrlReaderExpression = new UrlReaderExpression();

        public override Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.Accessor) {
                AccessExpressionNode accessExpressionNode = (AccessExpressionNode) node;
                List<AccessExpressionPartNode> parts = accessExpressionNode.parts;
             
                if (parts.Count > 1) {
                    return null;
                }

                if (parts[0] is PropertyAccessExpressionPartNode field) {
                    if (field.fieldName == "url") {
                        return s_UrlReaderExpression;
                    }
                    
                }
                
            }

            return null;
        }

        public class UrlReaderExpression : Expression<string> {

            public override Type YieldedType => typeof(string);

            public override object Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.current;
                return element.view.Application.Router.CurrentUrl;
            }

            public override string EvaluateTyped(ExpressionContext context) {
                UIElement element = (UIElement) context.current;
                return element.view.Application.Router.CurrentUrl;
            }

            public override bool IsConstant() {
                return false;
            }

        }

        public class RouteResolverExpression : Expression<IRouterElement> {

            public override Type YieldedType => typeof(Router);

            public override object Evaluate(ExpressionContext context) {
                return ((UIElement) context.current).GetNearestRouter();
            }

            public override IRouterElement EvaluateTyped(ExpressionContext context) {
                return ((UIElement) context.current).GetNearestRouter();
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}