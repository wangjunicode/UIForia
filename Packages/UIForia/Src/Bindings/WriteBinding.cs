using System;
using UIForia.Elements;
using UIForia.Expressions;

namespace UIForia.Bindings {

    public class WriteBinding<U, T> : Binding where U : UIElement {

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

    }

}