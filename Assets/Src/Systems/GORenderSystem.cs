using System.Collections.Generic;
using Debugger;
using Rendering;
using Src.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Src.Systems {

    public class GORenderSystem : IRenderSystem {

        private readonly ILayoutSystem layoutSystem;
        private readonly IStyleSystem styleSystem;
        private readonly RectTransform rectTransform;
        private readonly IElementRegistry elementRegistry;

        private readonly SkipTree<RenderData> renderSkipTree;
        private readonly Dictionary<int, RectTransform> transforms;

        private bool ready;

        public GORenderSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem, IElementRegistry elementRegistry, RectTransform rectTransform) {
            this.layoutSystem = layoutSystem;
            this.rectTransform = rectTransform;
            this.elementRegistry = elementRegistry;
            this.renderSkipTree = new SkipTree<RenderData>();

            this.transforms = new Dictionary<int, RectTransform>();
            this.renderSkipTree.onItemParentChanged += (item, newParent, oldParent) => {
                item.unityTransform.SetParent(newParent == null ? rectTransform : newParent.unityTransform);
            };

            this.styleSystem = styleSystem;
        }


        public void OnReady() {
            ready = true;
        }

        public void OnInitialize() {
            this.styleSystem.onFontPropertyChanged += HandleFontPropertyChanged;
            this.styleSystem.onTextContentChanged += HandleTextContentChanged;
            this.styleSystem.onBorderChanged += HandleStyleChange;
            this.styleSystem.onPaddingChanged += HandleStyleChange;
            this.styleSystem.onBorderChanged += HandleStyleChange;
            this.styleSystem.onPaintChanged += HandlePaintChange;
            this.styleSystem.onBorderRadiusChanged += HandleBorderRadiusChange;
        }

        public void OnElementCreated(InitData creationData) {
            UIGraphicElement directDraw = creationData.element as UIGraphicElement;

            OnElementStyleChanged(creationData.element);

            for (int i = 0; i < creationData.children.Count; i++) {
                OnElementCreated(creationData.children[i]);
            }

            UITextContainerElement container = creationData.element as UITextContainerElement;

            if (directDraw != null) {
                RectTransform transform = transforms[directDraw.id];
                GOCanvasElement canvasElement = transform.gameObject.AddComponent<GOCanvasElement>();
                canvasElement.m_GraphicElement = directDraw;
                directDraw.updateManager = canvasElement;
            }

            if (container != null) {
                RectTransform containerTransform = transforms[container.id];
                Transform child = containerTransform.GetChild(0);
                // todo -- need the font ref to update when font changes
                container.textInfo = child.GetComponent<TextMeshProUGUI>().textInfo;
                container.fontAsset = child.GetComponent<TextMeshProUGUI>().font;
                container.textInfo.textComponent.text = container.textInfo.textComponent.text ?? string.Empty;
            }
        }

//        private void HandleMeshUpdate(int elementId, Mesh mesh) {
//            UIGraphicElement graphicElement = (UIGraphicElement) renderSkipTree.GetItem(elementId).element;
//            Transform transform = transforms[elementId];
//
//            CanvasRenderer canvasRenderer = transform.GetComponent<CanvasRenderer>();
//            if (canvasRenderer == null) {
//                canvasRenderer = transform.gameObject.AddComponent<CanvasRenderer>();
//                canvasRenderer.SetMaterial(graphicElement.GetMaterial(), Texture2D.whiteTexture);
//            }
//
//            canvasRenderer.SetMesh(mesh);
//        }

        public void OnUpdate() {
            int count = layoutSystem.RectCount;
            LayoutResult[] layoutResults = layoutSystem.LayoutResults;

            for (int i = 0; i < count; i++) {
                if (!transforms.ContainsKey(layoutResults[i].elementId)) {
                    continue;
                }

                RenderData renderData = renderSkipTree.GetItem(layoutResults[i].elementId);
                ContentBoxRect margin = renderData.element.style.margin;
                RectTransform transform = transforms[layoutResults[i].elementId];
                Vector2 position = layoutResults[i].localRect.position;
                position.x = (int) position.x + margin.left;
                position.y = (int) position.y + margin.top;
                Vector2 size = layoutResults[i].localRect.size;
                size.x = (int) size.x - (margin.left + margin.right);
                size.y = (int) size.y - (margin.top + margin.bottom);

                // TextMeshPro text elements need a bit more space or they wrap weirdly
                if (renderData.element is UITextElement) {
                    TextMeshProUGUI tmp = renderData.renderComponent as TextMeshProUGUI;
                    if (tmp != null) {
                        int characterCount = tmp.textInfo.characterCount;
                        if (characterCount > 0) {
                            size.x += (tmp.textInfo.characterInfo[tmp.textInfo.characterCount - 1].xAdvance * 2f);
                        }
                    }
                }

                transform.anchoredPosition = new Vector3(position.x, -position.y, 0);
                transform.sizeDelta = size;
            }
        }

        public void OnReset() {
            ready = false;
            renderSkipTree.TraversePreOrder((data => { data.unityTransform = null; }));

            foreach (var kvp in transforms) {
                Object.Destroy(kvp.Value.gameObject);
            }

            this.styleSystem.onTextContentChanged -= HandleTextContentChanged;
            this.styleSystem.onBorderChanged -= HandleStyleChange;
            this.styleSystem.onPaddingChanged -= HandleStyleChange;
            this.styleSystem.onBorderChanged -= HandleStyleChange;
            this.styleSystem.onPaintChanged -= HandlePaintChange;
            this.styleSystem.onBorderRadiusChanged -= HandleBorderRadiusChange;
            renderSkipTree.Clear();
            transforms.Clear();
        }

        public void OnDestroy() {
            OnReset();
        }

        private void OnElementStyleChanged(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);

            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);

            if (data == null) {
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

            data.primitiveType = primitiveType;

            if (data.renderComponent != null) {
                Object.Destroy(data.renderComponent);
            }

            if (data.maskComponent != null) {
                Object.Destroy(data.maskComponent);
            }

            if (primitiveType == RenderPrimitiveType.None) {
                return;
            }

            data.primitiveType = primitiveType;
            CreateComponents(data);
        }

        public void OnElementDestroyed(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);

            if (data == null) return;

            renderSkipTree.RemoveItem(data);
            transforms.Remove(element.id);

            if (data.renderComponent != null) {
                Object.Destroy(data.renderComponent);
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
            if (transforms.ContainsKey(element.id)) {
                transforms[element.id].gameObject.SetActive(true);
            }

            renderSkipTree.ConditionalTraversePreOrder(element, (item) => {
                if (item.element.isDisabled) return false;
                item.unityTransform.gameObject.SetActive(true);
                return true;
            });
        }

        public void OnElementDisabled(UIElement element) {
            if (transforms.ContainsKey(element.id)) {
                transforms[element.id].gameObject.SetActive(false);
            }

            // do I need to do the whole hierarchy?
            renderSkipTree.TraversePreOrder(element, (item) => { item.unityTransform.gameObject.SetActive(false); });
        }

        private void CreateComponents(RenderData data) {
            GameObject gameObject = data.unityTransform.gameObject;
            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                    data.renderComponent = gameObject.AddComponent<RawImage>();
                    break;

                case RenderPrimitiveType.ProceduralImage:
                    data.renderComponent = gameObject.AddComponent<BorderedImage>();
                    break;

                case RenderPrimitiveType.Mask:
                case RenderPrimitiveType.Mask2D:
                    break;

                case RenderPrimitiveType.Text:
                    data.renderComponent = gameObject.AddComponent<TextMeshProUGUI>();
                    break;

            }

            ApplyStyles(data);
        }


        private void ApplyStyles(RenderData data) {
            UIElement element = data.element;
            UIStyleSet style = element.style;

            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                    RawImage rawImage = (RawImage) data.renderComponent;
                    rawImage.texture = style.backgroundImage;
                    rawImage.color = style.backgroundColor;
                    rawImage.uvRect = new Rect(0, 0, 1, 1);
                    break;

                case RenderPrimitiveType.ProceduralImage:
//                    Shape shape = data.shape;
//                    shape.settings.outlineColor = style.borderColor;
//                    shape.settings.outlineSize = style.borderLeft;
//                    shape.settings.fillColor = style.backgroundColor;
//                    shape.settings.roundnessPerCorner = false;
//                    shape.settings.roundness = style.borderRadiusBottomLeft;
//                    shape.settings.fillType = FillType.SolidColor;
//                    shape.settings.fill
                    BorderedImage procImage = (BorderedImage) data.renderComponent;
                    procImage.color = style.backgroundColor;
                    procImage.borderColor = style.borderColor;
                    procImage.border = style.border;
                    break;

                case RenderPrimitiveType.Text:
                    TextMeshProUGUI textMesh = (TextMeshProUGUI) data.renderComponent;
                    textMesh.text = style.textContent;
                    textMesh.fontSize = style.fontSize;
                    textMesh.color = style.textColor;
                    break;

                case RenderPrimitiveType.Mask:
                case RenderPrimitiveType.Mask2D:
                    break;
            }
        }

        private RenderPrimitiveType DeterminePrimitiveType(UIElement element) {
            if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                return RenderPrimitiveType.None;
            }

            if ((element.flags & UIElementFlags.TextElement) != 0) {
                return RenderPrimitiveType.Text;
            }

            UIStyleSet styleSet = element.style;
            if (styleSet.backgroundImage == null
                && styleSet.borderColor == ColorUtil.UnsetValue
                && styleSet.backgroundColor == ColorUtil.UnsetValue) {
                return RenderPrimitiveType.None;
            }

            ContentBoxRect border = styleSet.border;
            if (border.left > 0 || border.right > 0 || border.top > 0 || border.bottom > 0) {
                return RenderPrimitiveType.ProceduralImage;
            }

            return RenderPrimitiveType.RawImage;
        }


        private void HandleBorderRadiusChange(int elementId, BorderRadius radius) {
            if (!ready) return;
            UIElement element = elementRegistry.GetElement(elementId);
            OnElementStyleChanged(element);
        }

        private void HandlePaintChange(int elementId, Paint paint) {
            if (!ready) return;
            UIElement element = elementRegistry.GetElement(elementId);
            OnElementStyleChanged(element);
        }

        private void HandleStyleChange(int elementId, ContentBoxRect rect) {
            if (!ready) return;
            UIElement element = elementRegistry.GetElement(elementId);
            OnElementStyleChanged(element);
        }

        private void HandleTextContentChanged(int elementId, string text) {
            if (!ready) return;
            RenderData data = renderSkipTree.GetItem(elementId);
            if (data == null) return;
            TextMeshProUGUI textMesh = data.renderComponent as TextMeshProUGUI;
            if (textMesh != null) {
                textMesh.text = text;
            }
        }

        private void HandleFontPropertyChanged(int elementId, TextStyle style) {
            if (!ready) return;
            RenderData data = renderSkipTree.GetItem(elementId);
            if (data == null) return;
            TextMeshProUGUI textMesh = data.renderComponent as TextMeshProUGUI;
            if (textMesh != null) {
                ApplyStyles(data);
            }
        }

    }

}