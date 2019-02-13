using System;

namespace UIForia {

    public class SwitchBindingInt : Binding {

        private readonly int id;
        private readonly int[] values;
        private readonly Expression<int> expression;
        private readonly UISwitchDefaultElement defaultCase;

        public SwitchBindingInt(int id, int[] values, Expression<int> expression) : base($"{id}") {
            this.id = id;
            this.values = values;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            throw new NotImplementedException();
//            int value = expression.EvaluateTyped(context);
//            
//            for (int i = 0; i < values.Length; i++) {
//                if (values[i] == value) {
//                    context.SetSwitchValue(id, i);
//                    return;
//                }
//            }
//            
//            context.SetSwitchValue(id, -1);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}