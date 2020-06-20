using System;
using UIForia.Compilers.Style;
using UIForia.Rendering;

namespace UIForia.Systems {

    public class RenderBoxPool {

        private ResourceManager resourceManager;

        public RenderBoxPool(ResourceManager resourceManager) {
            this.resourceManager = resourceManager;
        }

        // todo -- this doesn't actually pool right now
        public RenderBox GetCustomPainter(string painterId) {

            if (painterId == "self") {
                return new SelfPaintedRenderBox();
            }

            if (Application.s_CustomPainters.TryGetValue(painterId, out Type boxType)) {
                return (RenderBox) Activator.CreateInstance(boxType);
            }

            if (resourceManager.TryGetStylePainter(painterId, out StylePainterDefinition painter)) {

                StylePainterRenderBox stylePainterRenderBox = new StylePainterRenderBox();
                stylePainterRenderBox.painterDefinition = painter;
                return stylePainterRenderBox;

            }

            return null;
        }

    }

}