using System;

namespace UIForia.Compilers {

    public class RepeatItemAliasResolver : ExpressionAliasResolver {

        public readonly Type itemType;

        public RepeatItemAliasResolver(string itemAlias, Type itemType) : base(itemAlias) {
            this.itemType = itemType;
        }

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            if (nodeOld.expressionType == ExpressionNodeType.AliasAccessor) {
                ReflectionUtil.ObjectArray1[0] = aliasName;
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(RepeatItemExpression<>),
                    itemType,
                    ReflectionUtil.ObjectArray1
                );
            }

            return null;
        }

        public class RepeatItemExpression<T> : Expression<T> {

            public readonly string itemAlias;

            public RepeatItemExpression(string itemAlias) {
                this.itemAlias = itemAlias;
            }

            public override Type YieldedType => typeof(T);

            public override T Evaluate(ExpressionContext context) {
                UIElement ptr = ((UIElement) context.currentObject).parent;
                while (ptr != null) {
                    if (ptr is UIRepeatElement<T> repeatElement && repeatElement.itemAlias == itemAlias) {
                        return repeatElement.currentItem;
                    }

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