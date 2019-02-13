using System;
using UIForia.Util;

namespace UIForia {

    // todo [OnPropChanged(nameof(someproperty))]
    // todo enable OnPropChanged annotations

    public interface IPropertyChangedHandler {

        void OnPropertyChanged(string propertyName, object oldValue);

    }

    // todo make overloads for numeric types, value types, and class type to avoid boxing
    public class FieldSetterBinding<U, T> : Binding where U : UIElement {

        private readonly Expression<T> expression;
        private readonly Func<U, T> getter;
        private readonly Func<U, T, T> setter;

        public FieldSetterBinding(string bindingId, Expression<T> expression, Func<U, T> getter, Func<U, T, T> setter) : base(bindingId) {
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
                IPropertyChangedHandler changedHandler = element as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(bindingId, currentValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

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
                IPropertyChangedHandler changedHandler = element as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(bindingId, currentValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class FieldSetterBinding_WithCallbacks<U, T> : Binding where U : UIElement {

        private readonly Expression<T> expression;
        private readonly Func<U, T> getter;
        private readonly Func<U, T, T> setter;
        private readonly Action<U, string>[] callbacks;

        public FieldSetterBinding_WithCallbacks(string bindingId, Expression<T> expression, Func<U, T> getter, Func<U, T, T> setter, LightList<object> callbacks) : base(bindingId) {
            this.expression = expression;
            this.getter = getter;
            this.setter = setter;
            this.callbacks = new Action<U, string>[callbacks.Count];
            for (int i = 0; i < callbacks.Count; i++) {
                this.callbacks[i] = (Action<U, string>) callbacks[i];
            }
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            U castElement = (U) element;
            T currentValue = getter(castElement);
            T newValue = expression.Evaluate(context);

            if (!Equals(currentValue, newValue)) {
                setter(castElement, newValue);
                for (int i = 0; i < callbacks.Length; i++) {
                    callbacks[i].Invoke(castElement, bindingId);
                }

                IPropertyChangedHandler changedHandler = element as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(bindingId, currentValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}