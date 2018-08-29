using System;

namespace Src {

    // todo make overloads for numeric types, value types, and class type to avoid boxing
    public class FieldSetterBinding<U, T> : Binding where U : UIElement {

        private readonly Expression<T> expression;
        private readonly Func<U, T> getter;
        private readonly Func<U, T, T> setter;

        public FieldSetterBinding(Expression<T> expression, Func<U, T> getter, Func<U, T, T> setter) {
            this.expression = expression;
            this.getter = getter;
            this.setter = setter;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            U castElement = (U) element;
            T currentValue = getter(castElement);
            T newValue = expression.EvaluateTyped(context);
           
            if (!Equals(currentValue, newValue)){
                setter(castElement, newValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }


}