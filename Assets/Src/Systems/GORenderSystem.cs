using System.Collections.Generic;
using Rendering;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Src.Systems {

    public class GORenderSystem : IRenderSystem {

        private readonly ILayoutSystem layoutSystem;
        private readonly IStyleSystem styleSystem;
        private readonly RectTransform rectTransform;

        private readonly SkipTree<RenderData> renderSkipTree;
        private readonly Dictionary<int, RectTransform> transforms;

        public Font tempFont;
        public GORenderSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem, RectTransform rectTransform) {
            this.layoutSystem = layoutSystem;
            this.rectTransform = rectTransform;
            this.renderSkipTree = new SkipTree<RenderData>();
            this.transforms = new Dictionary<int, RectTransform>();
            this.renderSkipTree.onItemParentChanged += (item, newParent, oldParent) => {
                item.unityTransform.SetParent(newParent == null ? rectTransform : newParent.unityTransform);
            };
            this.styleSystem = styleSystem;
            this.styleSystem.onTextContentChanged += (elementId, text) => {
                if (transforms.ContainsKey(elementId)) {
                    RectTransform transform = transforms[elementId];
                    transform.GetComponent<Text>().text = text;
                }
            };
        }

        public void OnReady() { }

        public void OnInitialize() { }

        public void OnElementCreated(InitData creationData) {
            OnElementStyleChanged(creationData.element);
            for (int i = 0; i < creationData.children.Count; i++) {
                OnElementCreated(creationData.children[i]);
            }
        }

        public void OnUpdate() {
            // todo -- only run layout when its actually needed
            int count = layoutSystem.RectCount;
            LayoutResult[] layoutResults = layoutSystem.LayoutResults;

            for (int i = 0; i < count; i++) {
                if (transforms.ContainsKey(layoutResults[i].elementId)) {
                    RectTransform transform = transforms[layoutResults[i].elementId];
                    Vector2 position = layoutResults[i].localRect.position;
                    transform.anchoredPosition = new Vector3(position.x, -position.y, 0);
                    transform.sizeDelta = layoutResults[i].localRect.size;
                }
            }
        }

        public void OnReset() {
            renderSkipTree.TraversePreOrder((data => {
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
            if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                return;
            }

            RenderData data = renderSkipTree.GetItem(element);

            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);

            if (data == null) {
                if (primitiveType == RenderPrimitiveType.None) {
                    return;
                }

                GameObject obj = new GameObject(element.ToString());

                RectTransform unityTransform = obj.AddComponent<RectTransform>();
                unityTransform.anchorMin = new Vector2(0, 1);
                unityTransform.anchorMax = new Vector2(0, 1);
                unityTransform.pivot = new Vector2(0, 1);
                transforms[element.id] = unityTransform;

                obj.SetActive(element.isEnabled);
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

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnRender() {
            OnUpdate();
        }

        public void OnElementEnabled(UIElement element) {
//            RenderData data = renderSkipTree.GetItem(element);
//            if (data == null) {
//                // todo probably only works for 1 element not heirarhcy
////                OnElementStyleChanged(element);
//                
//                return;
//            }
            renderSkipTree.ConditionalTraversePreOrder(element, (item) => {
                if (item.element.isDisabled) return false;
                item.unityTransform.gameObject.SetActive(true);
                return true;
            });
//            renderSkipTree.EnableHierarchy(data);
//            if (transforms.ContainsKey(element.id)) {
//                transforms[element.id].gameObject.SetActive(true);
//            }
        }

        public void OnElementDisabled(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);
            renderSkipTree.DisableHierarchy(data);
            if (transforms.ContainsKey(element.id)) {
                transforms[element.id].gameObject.SetActive(false);
            }
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
                case RenderPrimitiveType.Text:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<Text>();
                    Text text = (Text) data.imageComponent;
                    text.text = ((UITextElement) data.element).GetText();
                    text.font = tempFont;
                    text.color = data.element.style.textColor;
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    text.verticalOverflow = VerticalWrapMode.Overflow;
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
                    text.text = ((UITextElement) data.element).GetText();
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
                && styleSet.borderColor == ColorUtil.UnsetColorValue
                && styleSet.backgroundColor == ColorUtil.UnsetColorValue) {
                return RenderPrimitiveType.None;
            }

//            if (styleSet.borderRadius != UIStyle.UnsetFloatValue) {
//                return RenderPrimitiveType.ProceduralImage;
//            }

            return RenderPrimitiveType.RawImage;
        }

    }

}