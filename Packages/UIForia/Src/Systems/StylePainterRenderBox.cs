using System.Diagnostics;
using UIForia.Compilers.Style;
using UIForia.Graphics;

namespace UIForia.Rendering {

    [DebuggerDisplay("{element.ToString()}")]
    public class StylePainterRenderBox : RenderBox {

        public StylePainterDefinition painterDefinition;
        
        public override void OnInitialize() {
            base.OnInitialize();
            HasForeground = painterDefinition.paintForeground != null;
            materialId = element.application.ResourceManager.GetMaterialId("UIForiaShape");
        }

        StylePainterContext stylePainterContext = new StylePainterContext();
        
        public override void PaintBackground2(RenderContext2 ctx) {

            if (painterDefinition.paintBackground != null) {
                stylePainterContext.ctx = ctx;
                stylePainterContext._element = element;
                stylePainterContext._variables = painterDefinition.definedVariables;
                painterDefinition.paintBackground(stylePainterContext);
            }

        }
        

    }

}