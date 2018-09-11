using Src.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public interface IGraphicUpdateManager {

        void MarkGeometryDirty(UIGraphicElement element);

    }

    public class GOCanvasElement : MonoBehaviour, ICanvasElement, IGraphicUpdateManager {

        public UIGraphicElement m_GraphicElement;
        private CanvasRenderer m_CanvasRenderer;

        private void Start() {
            m_CanvasRenderer = gameObject.AddComponent<CanvasRenderer>();
            m_CanvasRenderer.SetMaterial(m_GraphicElement.GetMaterial(), Texture2D.whiteTexture);
        }

        public void Rebuild(CanvasUpdate executing) {
            if (executing != CanvasUpdate.LatePreRender) {
                return;
            }
            
            m_GraphicElement.RebuildGeometry();
            m_CanvasRenderer.SetMesh(m_GraphicElement.GetMesh());
        }

        public void LayoutComplete() { }

        public void GraphicUpdateComplete() { }

        public bool IsDestroyed() {
            return false;
        }

        public void MarkGeometryDirty(UIGraphicElement element) {
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

    }

}