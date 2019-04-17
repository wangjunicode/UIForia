using System;
using UIForia.Elements;
using UIForia.Expressions;

namespace UIForia.Bindings {

    public class PropertySetterBinding<U, T> : Binding where U : UIElement {

        private readonly Expression<T> expression;
        private readonly Func<U, T> getter;
        private readonly Func<U, T, T> setter;

        public PropertySetterBinding(string bindingId, Expression<T> expression, Func<U, T> getter, Func<U, T, T> setter) : base(bindingId) {
            this.expression = expression;
            this.getter = getter;
            this.setter = setter;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            U castElement = (U) element;
            T currentValue = getter(castElement);
            T newValue = expression.Evaluate(context);

            if (!Equals(currentValue, newValue)) {
                setter(castElement, newValue);
                // ReSharper disable once SuspiciousTypeConversion.Global
                IPropertyChangedHandler changedHandler = element as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(bindingId, currentValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}