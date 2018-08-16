using Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class BackgroundColorBinding : Binding {

        private readonly StyleStateType stateType;
        private readonly Expression expression;
        
        public BackgroundColorBinding(StyleStateType stateType, Expression expression) {
            this.stateType = stateType;
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            Color color = element.style.GetBackgroundColorForState(stateType);
            Color newColor = (Color)expression.Evaluate(context);
            if(color != newColor) {
            // todo should get style for state and apply to that instead
                element.style.backgroundColor = newColor;
            }
        }

    }

}