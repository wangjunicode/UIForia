using System.Diagnostics;
using UIForia.Compilers.Style;
using UIForia.Graphics;
using Debug = UnityEngine.Debug;

namespace UIForia.Rendering {

    [DebuggerDisplay("{element.ToString()}")]
    public class StylePainterRenderBox : RenderBox {

        public StylePainterDefinition painterDefinition;
        public StylePainterContext stylePainterContext;

        public StylePainterRenderBox(StylePainterDefinition painterDefinition) {
            this.painterDefinition = painterDefinition;
        }
        
        public override void OnInitialize() {
            base.OnInitialize();
            HasForeground = painterDefinition.paintForeground != null;
            materialId = element.application.ResourceManager.GetMaterialId("UIForiaShape");
            stylePainterContext = new StylePainterContext();
        }

        public override void PaintBackground3(RenderContext3 ctx) {

            if (painterDefinition.paintBackground != null) {
                stylePainterContext.Setup(ctx, element, painterDefinition.definedVariables);
                painterDefinition.paintBackground(stylePainterContext);
            }

        }

    }

}