using System;
using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_LayoutFlowType : StyleBinding {

        private readonly Expression<LayoutFlowType> expression;

        public StyleBinding_LayoutFlowType(StyleState state, Expression<LayoutFlowType> expression) : base(RenderConstants.LayoutFlow, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            throw new NotImplementedException();
            
//            LayoutFlowType flow = element.style.GetLayoutFlow(state);
//            LayoutFlowType newFlow = expression.EvaluateTyped(context);
//            if (flow != newFlow) {
//                element.style.SetLayoutFlow(newFlow, state);
//            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.LayoutInFow = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            throw new NotImplementedException();
//            styleSet.SetLayoutFlow(expression.EvaluateTyped(context), state);
        }

    }

}