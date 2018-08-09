using System.Collections;

namespace Src {

    public class ArrayAccessorBinding : ExpressionBinding {

        public readonly ExpressionBinding expressionBinding;

        public ArrayAccessorBinding(ExpressionBinding expressionBinding) {
            this.expressionBinding = expressionBinding;
        }

        public object Evaluate(TemplateContext context, IList list) {
            int idx = (int) expressionBinding.Evaluate(context);
            if ((uint) idx >= (uint) list.Count) {
                return null;
            }
            return list[idx];
        }

    }

}