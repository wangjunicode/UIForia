using System;

namespace UIForia.Compilers {

    public class RepeatIndexAliasResolver : ExpressionAliasResolver {

        public RepeatIndexAliasResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsValueExpression(CompilerContext context) {
            return new RepeatIndexExpression(aliasName);
        }

        public class RepeatIndexExpression : Expression<int> {

            public readonly string indexAlias;

            public RepeatIndexExpression(string indexAlias) {
                this.indexAlias = indexAlias;
            }

            public override Type YieldedType => typeof(int);

            public override int Evaluate(ExpressionContext context) {
                UIElement trail = ((UIElement) context.currentObject);
                UIElement ptr = trail.parent;
                while (ptr != null) {
                    if (ptr is UIRepeatElement repeatElement && repeatElement.indexAlias == indexAlias) {
                        return trail.siblingIndex;
                    }

                    trail = ptr;
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