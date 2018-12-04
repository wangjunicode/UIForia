using System;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public class RepeatIndexAliasResolver : ExpressionAliasResolver {

        public RepeatIndexAliasResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsValueExpression(ASTNode node, Func<ASTNode, Expression> visit) {

            return null;
        }
        
        public class RepeatIndexExpression : Expression<int> {

            public readonly string indexAlias;
            
            public RepeatIndexExpression(string indexAlias) {
                this.indexAlias = indexAlias;
            }
            
            public override Type YieldedType => typeof(int);           

            public override int Evaluate(ExpressionContext context) {
                UIElement ptr = ((UIElement)context.currentObject).parent;
                while (ptr != null) {
                    if (ptr is UIRepeatElement repeatElement) {
                        if (repeatElement.indexAlias == indexAlias) {
                            return repeatElement.currentIndex;
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