using System;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Bindings {

    public abstract class WriteBinding : Binding {

        public string eventName;
        public Delegate handler;
        public object wrapper;
        public Type[] genericArguments;

        protected WriteBinding(string bindingId) : base(bindingId) { }

        public abstract Type BoundType();
        
    }
    
    public class WriteBinding<U, T> : WriteBinding where U : UIElement {

        private readonly Func<U, T> getter;
        private readonly WriteTargetExpression<T> writeTargetExpression;

        public WriteBinding(string bindingId, WriteTargetExpression<T> writeTargetExpression, Func<U, T> getter) : base(bindingId) {
            this.writeTargetExpression = writeTargetExpression;
            this.getter = getter;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            writeTargetExpression.Assign(context, getter((U)element));
        }

        public override bool IsConstant() {
            return false;
        }

        public override Type BoundType() {
            return typeof(T);
        }

    }
    
    public class WriteBinding_WithCallbacks<U, T> : WriteBinding where U : UIElement {

        private readonly Func<U, T> getter;
        private readonly WriteTargetExpression<T> writeTargetExpression;
        private readonly Action<U, string>[] callbacks;

        public WriteBinding_WithCallbacks(string bindingId, WriteTargetExpression<T> writeTargetExpression, Func<U, T> getter, LightList<object> callbacks) : base(bindingId) {
            this.writeTargetExpression = writeTargetExpression;
            this.getter = getter;
            this.callbacks = new Action<U, string>[callbacks.Count];
            for (int i = 0; i < callbacks.Count; i++) {
                this.callbacks[i] = (Action<U, string>) callbacks[i];
            }
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            T val = getter((U) element);
            writeTargetExpression.Assign(context, getter((U)element));
            
        }

        public override bool IsConstant() {
            return false;
        }

        public override Type BoundType() {
            return typeof(T);
        }

    }

}