using System.Collections.Generic;
using Rendering;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Src.Systems {

    public class GameObjectRenderSystem : IRenderSystem {

        private LayoutSystem layoutSystem;
        private RectTransform rectTransform;
        private LayoutResult[] layoutResults;

        private SkipTree<RenderData> renderSkipTree;
        private Dictionary<int, RectTransform> transforms;

        public GameObjectRenderSystem(LayoutSystem layoutSystem, RectTransform rectTransform) {
            this.layoutSystem = layoutSystem;
            this.rectTransform = rectTransform;
            this.renderSkipTree = new SkipTree<RenderData>();
            this.layoutResults = new LayoutResult[128];
            this.transforms = new Dictionary<int, RectTransform>();
        }

        public void OnElementCreated(UIElementCreationData creationData) {
            OnElementStyleChanged(creationData.element);
        }

        public void OnUpdate() {
            OnRender();
        }

        public void OnRender() {
            Rect viewport = rectTransform.rect;

            // todo -- only run layout when its actually needed
            int count = layoutSystem.RunLayout(viewport, ref layoutResults);

            for (int i = 0; i < count; i++) {
                RectTransform transform = transforms[layoutResults[i].elementId];
                Vector2 position = layoutResults[i].rect.position;
                transform.anchoredPosition = new Vector2(position.x, -position.y);
                transform.sizeDelta = layoutResults[i].rect.size;
            }

        }

        public void SetViewportRect(Rect viewport) {
            
        }

        public void OnReset() {

            renderSkipTree.TraversePreOrderWithCallback((data => {
                data.unityTransform = null;
            }));

            foreach (var kvp in transforms) {
                Object.Destroy(kvp.Value.gameObject);
            }

            renderSkipTree.Clear();
            transforms.Clear();

        }

        public void OnDestroy() {
            OnReset();
        }

        public void OnElementStyleChanged(UIElement element) {
            
            // todo -- replace w/ flag check and hide style if possible
            
            if(element.style == null) return;
            
            RenderData data = renderSkipTree.GetItem(element);

            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);

            if (data == null) {

                if (primitiveType == RenderPrimitiveType.None) {
                    // probably not needed but just to be save unset the flag
                    element.flags &= ~(UIElementFlags.RequiresRendering);
                    return;
                }

                element.flags |= UIElementFlags.RequiresRendering;

                GameObject obj = new GameObject(element.ToString());

                RectTransform unityTransform = obj.AddComponent<RectTransform>();
                unityTransform.anchorMin = new Vector2(0, 1);
                unityTransform.anchorMax = new Vector2(0, 1);
                unityTransform.pivot = new Vector2(0, 1);
                transforms[element.id] = unityTransform;

                data = new RenderData(element, primitiveType, unityTransform, rectTransform);
                CreateComponents(data);
                renderSkipTree.AddItem(data);

                return;
            }

            if (primitiveType == data.primitiveType) {
                ApplyStyles(data);
                return;
            }

            // todo -- pool
            if (data.imageComponent != null) {
                Object.Destroy(data.imageComponent);
            }

            if (data.maskComponent != null) {
                Object.Destroy(data.maskComponent);
            }

            if (primitiveType == RenderPrimitiveType.None) {
                // todo -- pool

                element.flags &= ~(UIElementFlags.RequiresRendering);

                renderSkipTree.RemoveItem(data);
                Object.Destroy(data.unityTransform);
                data.rootTransform = null;
                data.element = null;
                return;
            }

            CreateComponents(data);

        }
        
        public void OnElementDestroyed(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);

            if (data == null) return;

            renderSkipTree.RemoveItem(data);
            transforms.Remove(element.id);

            if (data.imageComponent != null) {
                Object.Destroy(data.imageComponent);
            }

            if (data.maskComponent != null) {
                Object.Destroy(data.maskComponent);
            }

            Object.Destroy(data.unityTransform);
            data.rootTransform = null;
            data.element = null;
        }

        public void OnElementEnabled(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);
            renderSkipTree.EnableHierarchy(data);
        }

        public void OnElementDisabled(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);
            renderSkipTree.DisableHierarchy(data);
        }

        private void CreateComponents(RenderData data) {
            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<RawImage>();
                    ApplyStyles(data);
                    return;
                case RenderPrimitiveType.ProceduralImage:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<RawImage>();
                    ApplyStyles(data);
                    return;
                case RenderPrimitiveType.Mask:
                case RenderPrimitiveType.Mask2D:
                    break;
            }
        }

        private void ApplyStyles(RenderData data) {
            UIElement element = data.element;
            UIStyleSet style = element.style;

            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                    RawImage rawImage = (RawImage) data.imageComponent;
                    rawImage.texture = style.backgroundImage;
                    rawImage.color = style.backgroundColor;
                    rawImage.uvRect = new Rect(0, 0, 1, 1);
                    break;
                case RenderPrimitiveType.ProceduralImage:
                    ProceduralImage procImage = (ProceduralImage) data.imageComponent;
                    procImage.color = style.backgroundColor;
                    break;
                case RenderPrimitiveType.Text:
                    Text text = (Text) data.imageComponent;
                    break;
                case RenderPrimitiveType.Mask:
                case RenderPrimitiveType.Mask2D:
                    break;
            }
        }

        private RenderPrimitiveType DeterminePrimitiveType(UIElement element) {
            if ((element.flags & UIElementFlags.TextElement) != 0) {
                return RenderPrimitiveType.Text;
            }
            UIStyleSet styleSet = element.style;
            if (styleSet.backgroundImage == null
                && styleSet.borderColor == UIStyle.UnsetColorValue
                && styleSet.backgroundColor == UIStyle.UnsetColorValue) {
                return RenderPrimitiveType.None;
            }

//            if (styleSet.borderRadius != UIStyle.UnsetFloatValue) {
//                return RenderPrimitiveType.ProceduralImage;
//            }

            return RenderPrimitiveType.RawImage;

        }

    }

}