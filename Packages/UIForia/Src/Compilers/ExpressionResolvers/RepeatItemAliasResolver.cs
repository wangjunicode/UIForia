using System;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Compilers.ExpressionResolvers {

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

    public class SelectOptionAliasResolver<T> : ExpressionAliasResolver {

        public SelectOptionAliasResolver(string itemAlias) : base(itemAlias) { }

        public override Expression CompileAsValueExpression(CompilerContext context) {
            ReflectionUtil.ObjectArray1[0] = aliasName;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(SelectOptionExpression<>),
                new GenericArguments(typeof(T)),
                ReflectionUtil.ObjectArray1
            );
        }

    }

    public class SelectOptionExpression<T> : Expression<ISelectOption<T>> {

        public readonly string itemAlias;

        public SelectOptionExpression(string itemAlias) {
            this.itemAlias = itemAlias;
        }

        public override Type YieldedType => typeof(ISelectOption<T>);

        public override ISelectOption<T> Evaluate(ExpressionContext context) {
            UIElement trail = (UIElement) context.currentObject;
            UIElement ptr = trail.parent;
            while (ptr != null) {
                if (ptr is Select<T> select) {
                    return select.options[trail.siblingIndex];
                }

                trail = ptr;
                ptr = ptr.parent;
            }

            throw new Exception($"Invalid select: could not find select of type {typeof(T)} with alias: {itemAlias}");
        }

        public override bool IsConstant() {
            return false;
        }

    }

}