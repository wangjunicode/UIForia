using System;
using Src.Systems;

namespace Rendering {

    public sealed class UIViewIMGUI : UIView {

        public UIViewIMGUI(Type elementType) : base(elementType) {
            renderSystem = new IMGUIRenderSystem();
        }

        public override IRenderSystem renderSystem { get; protected set; }
        
        public override void Render() {
            renderSystem.OnRender();
        }

    }

}