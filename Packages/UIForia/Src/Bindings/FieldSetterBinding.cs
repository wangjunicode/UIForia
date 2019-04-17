using System;
using System.Reflection;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Bindings {

    public class EventSetterBinding_Delegate<U, T> : Binding where U : UIElement where T : Delegate {

        private readonly Func<U, T> getter;
        public readonly EventInfo eventInfo;
        private readonly Expression<T> expression;

        public EventSetterBinding_Delegate(EventInfo eventInfo, Expression<T> expr, Func<U, T> getter) : base(eventInfo.Name) {
            this.eventInfo = eventInfo;
            this.expression = expr;
            this.getter = getter;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            T lastAction = getter((U) element);
            T newAction = expression.Evaluate(context);
            
            if (lastAction != newAction) {
                if (lastAction != null) {
                    eventInfo.RemoveEventHandler(element, lastAction);
                }

                if (newAction != null) {
                    eventInfo.AddEventHandler(element, newAction);
                }

            }
        }

        public override bool IsConstant() {
            return false;
        }

    }

    // todo make overloads for numeric types, value types, and class type to avoid boxing or just use linq expression
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

            // todo remove boxing, maybe with linq expression
            if (!Equals(currentValue, newValue)) {
                setter(castElement, newValue);
                // ReSharper disable once SuspiciousTypeConversion.Global
                IPropertyChangedHandler changedHandler = element as IPropertyChangedHandler;
                // todo remove boxing, maybe with linq expression
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

            // todo remove boxing, maybe with linq expression
            if (!Equals(currentValue, newValue)) {
                setter(castElement, newValue);
                for (int i = 0; i < callbacks.Length; i++) {
                    callbacks[i].Invoke(castElement, bindingId);
                }

                // ReSharper disable once SuspiciousTypeConversion.Global
                IPropertyChangedHandler changedHandler = element as IPropertyChangedHandler;
                // todo remove boxing, maybe with linq expression
                changedHandler?.OnPropertyChanged(bindingId, currentValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}