using System;
using UIForia.Elements;
using UIForia.Expressions;
using UnityEngine;

namespace UIForia.Compilers.ExpressionResolvers {

    public class SelectOptionExpression<T> : Expression<ISelectOption<T>> {

        public readonly string itemAlias;
        public readonly bool isInternal;

        public SelectOptionExpression(string itemAlias, bool isInternal = false) {
            this.itemAlias = itemAlias;
            this.isInternal = isInternal;
        }

        public override Type YieldedType => typeof(ISelectOption<T>);

        public override ISelectOption<T> Evaluate(ExpressionContext context) {
            if (isInternal) {
                UIElement trail = (UIElement) context.currentObject;
                UIElement ptr = trail.parent;
                Select<T> select = (Select<T>) context.rootObject;

                while (ptr != null) {
                    if (ptr is UIChildrenElement) {
                        return select.options[trail.siblingIndex];
                    }

                    trail = ptr;
                    ptr = ptr.parent;
                }

                throw new Exception($"Invalid select: could not find select of type {typeof(T)} with alias: {itemAlias}");
            }
            else {
                // find the select
                UIElement trail = (UIElement) context.currentObject;
                UIElement ptr = trail.parent;

                UIElement selectPtr = ptr;
                while (selectPtr != null) {
                    selectPtr = selectPtr.parent;
                    if ((selectPtr is Select<T> select)) {
                        if (select.selectedIndex < 0) {
                            return null;
                        }

                        return select.options[select.selectedIndex];
                    }
                }

                throw new Exception($"Invalid select: could not find select of type {typeof(T)} with alias: {itemAlias}");
            }
        }

        public override bool IsConstant() {
            return false;
        }

    }

}