using System;
using UIForia.Elements;
using UIForia.Expressions;

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

}