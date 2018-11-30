using System;

namespace UIForia.Compilers {

    public class RepeatLengthAliasResolver : ExpressionAliasResolver {

        public RepeatLengthAliasResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            if (nodeOld.expressionType == ExpressionNodeType.AliasAccessor) {
                return new RepeatLengthExpression(aliasName);
            }

            return null;
        }
        
        public class RepeatLengthExpression : Expression<int> {

            public readonly string lengthAlias;
            
            public RepeatLengthExpression(string lengthAlias) {
                this.lengthAlias = lengthAlias;
            }
            
            public override Type YieldedType => typeof(int);

            public override int Evaluate(ExpressionContext context) {
                UIElement ptr = ((UIElement)context.currentObject).parent;
                while (ptr != null) {
                    if (ptr is UIRepeatElement repeatElement) {
                        if (repeatElement.lengthAlias == lengthAlias) {
                            return repeatElement.children.Length;
                        }
                    }

                    ptr = ptr.parent;
                }

                return -1;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}