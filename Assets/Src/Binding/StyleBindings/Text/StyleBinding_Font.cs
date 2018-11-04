using Src.Rendering;
using TMPro;

namespace Src.StyleBindings.Text {

    public class StyleBinding_Font : StyleBinding {

        private readonly Expression<TMP_FontAsset> expression;

        public StyleBinding_Font(StyleState state, Expression<TMP_FontAsset> expression) : base(RenderConstants.Font, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            TMP_FontAsset current = element.ComputedStyle.TextFontAsset;
            TMP_FontAsset newFont = expression.EvaluateTyped(context);
            if (current != newFont) {
                element.style.SetFont(newFont, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.TextFontAsset = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            TMP_FontAsset current = styleSet.GetFont(state);
            TMP_FontAsset newFont = expression.EvaluateTyped(context);
            if (current != newFont) {
                styleSet.SetFont(newFont, state);
            }
        }

    }

}