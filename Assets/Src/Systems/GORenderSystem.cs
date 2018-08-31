using System.Collections.Generic;
using Debugger;
using Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InputField = UnityEngine.UI.InputField;
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
        public Font tempFont;

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

        private void HandleTextContentChanged(int elementId, string text) {
            if (transforms.ContainsKey(elementId)) {
                RectTransform transform = transforms[elementId];
                transform.GetComponent<Text>().text = text;
            }
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

        public void OnReady() {
            ready = true;
        }

        public void OnInitialize() {
            this.styleSystem.onTextContentChanged += HandleTextContentChanged;
            this.styleSystem.onBorderChanged += HandleStyleChange;
            this.styleSystem.onPaddingChanged += HandleStyleChange;
            this.styleSystem.onBorderChanged += HandleStyleChange;
            this.styleSystem.onPaintChanged += HandlePaintChange;
            this.styleSystem.onBorderRadiusChanged += HandleBorderRadiusChange;
        }

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
                if (!transforms.ContainsKey(layoutResults[i].elementId)) continue;
                RenderData renderData = renderSkipTree.GetItem(layoutResults[i].elementId);
                ContentBoxRect margin = renderData.element.style.margin;
                RectTransform transform = transforms[layoutResults[i].elementId];
                Vector2 position = layoutResults[i].localRect.position;
                position.x = (int) position.x + margin.left;
                position.y = (int) position.y + margin.top;
                Vector2 size = layoutResults[i].localRect.size;
                size.x = (int) size.x - (margin.left + margin.right);
                size.y = (int) size.y - (margin.top + margin.bottom);
                transform.anchoredPosition = new Vector3(position.x, -position.y, 0);
                transform.sizeDelta = size;
            }

            // if focusRequiresCursor
            // CreateOrUpdateCursor();
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

            if (data.imageComponent != null) {
                Object.Destroy(data.imageComponent);
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

            renderSkipTree.ConditionalTraversePreOrder(element, (item) => {
                item.unityTransform.gameObject.SetActive(false);
                return true;
            });
        }

        private void CreateComponents(RenderData data) {
            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<RawImage>();
                    ApplyStyles(data);
                    return;

                case RenderPrimitiveType.ProceduralImage:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<BorderedImage>();
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
                    text.fontSize = 12;
                    text.color = data.element.style.textColor;
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    text.verticalOverflow = VerticalWrapMode.Overflow;

                    UIInputFieldElement parent = data.element.parent as UIInputFieldElement;
                    if (parent != null) {
                        RectTransform t = transforms[parent.id];
                        InputField i = t.GetComponent<InputField>();
                        i.textComponent = text;
                        text.supportRichText = false;
                    }

                    break;

                case RenderPrimitiveType.InputField:
                    UIInputFieldElement inputElement = (UIInputFieldElement) data.element;

                    InputField input = data.unityTransform.gameObject.AddComponent<InputField>();

                    input.text = inputElement.content;

                    InputFieldFocusHandler focusHandler = input.gameObject.AddComponent<InputFieldFocusHandler>();

                    input.contentType = InputField.ContentType.Standard;
                    input.lineType = InputField.LineType.SingleLine;
                    input.transition = Selectable.Transition.None;

                    input.onValueChanged.AddListener((value) => {
                        inputElement.SetText(value);
                    });

                    focusHandler.onFocus += () => {
                        //view.FocusElement(element);
                    };

                    focusHandler.onBlur += (str) => {
                        //view.BlurElement(element);
                    };

                    // data.imageComponent = data.unityTransform.gameObject.AddComponent<BorderedImage>();
                    //ApplyStyles(data);
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
                case RenderPrimitiveType.InputField:
                case RenderPrimitiveType.ProceduralImage:
                    BorderedImage procImage = (BorderedImage) data.imageComponent;
                    procImage.color = style.backgroundColor;
                    procImage.borderColor = style.borderColor;
                    procImage.border = style.border;
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
            if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                return RenderPrimitiveType.None;
            }

            if ((element.flags & UIElementFlags.TextElement) != 0) {
                return RenderPrimitiveType.Text;
            }

            if ((element.flags & UIElementFlags.InputField) != 0) {
                return RenderPrimitiveType.InputField;
            }

            UIStyleSet styleSet = element.style;
            if (styleSet.backgroundImage == null
                && styleSet.borderColor == ColorUtil.UnsetColorValue
                && styleSet.backgroundColor == ColorUtil.UnsetColorValue) {
                return RenderPrimitiveType.None;
            }

            if (styleSet.border.IsDefined()) {
                return RenderPrimitiveType.ProceduralImage;
            }

            return RenderPrimitiveType.RawImage;
        }

    }

}