using System;

namespace UIForia.Compilers {

    public class RepeatItemAliasResolver : ExpressionAliasResolver {

        public readonly Type itemType;

        public RepeatItemAliasResolver(string itemAlias, Type itemType) : base(itemAlias) {
            this.itemType = itemType;
        }

        public override Expression CompileAsValueExpression(CompilerContext context) {
            ReflectionUtil.ObjectArray1[0] = aliasName;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(RepeatItemExpression<>),
                itemType,
                ReflectionUtil.ObjectArray1
            );
        }

        public class RepeatItemExpression<T> : Expression<T> {

            public readonly string itemAlias;

            public RepeatItemExpression(string itemAlias) {
                this.itemAlias = itemAlias;
            }

            public override Type YieldedType => typeof(T);

            public override T Evaluate(ExpressionContext context) {
                UIElement trail = (UIElement) context.currentObject;
                UIElement ptr = trail.parent;
                while (ptr != null) {
                    if (ptr is UIRepeatElement<T> repeatElement && repeatElement.itemAlias == itemAlias) {
                        return repeatElement.list[trail.siblingIndex];
                    }

                    trail = ptr;
                    ptr = ptr.parent;
                }

                throw new Exception($"Invalid repeat: could not find repeat of type {typeof(T)} with alias: {itemAlias}");
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}