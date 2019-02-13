using System;

namespace UIForia.Compilers {

    public class RepeatLengthAliasResolver : ExpressionAliasResolver {

        public RepeatLengthAliasResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsValueExpression(CompilerContext context) {
            return new RepeatLengthExpression(aliasName);
        }

        public class RepeatLengthExpression : Expression<int> {

            public readonly string lengthAlias;

            public RepeatLengthExpression(string lengthAlias) {
                this.lengthAlias = lengthAlias;
            }

            public override Type YieldedType => typeof(int);

            public override int Evaluate(ExpressionContext context) {
                UIElement ptr = ((UIElement) context.currentObject).parent;
                while (ptr != null) {
                    if (ptr is UIRepeatElement repeatElement) {
                        if (repeatElement.lengthAlias == lengthAlias) {
                            return repeatElement.children.Count;
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