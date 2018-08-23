using System;
using System.Collections.Generic;
using Rendering;
using Src.Extensions;
using UnityEngine;

namespace Src.Systems {

    public class IMGUIRenderSystem : IRenderSystem {

        private readonly IStyleSystem styleSystem;
        private readonly LayoutSystem layoutSystem;
        private readonly IElementRegistry elementSystem;
        private readonly SkipTree<IMGUIRenderData> renderSkipTree;
        private readonly Dictionary<Color, Texture2D> textureCache;

        private Rect viewportRect;
        private LayoutResult[] layoutResults;

        public IMGUIRenderSystem(IElementRegistry elementSystem, IStyleSystem styleSystem, LayoutSystem layoutSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;
            this.elementSystem = elementSystem;

            this.textureCache = new Dictionary<Color, Texture2D>();
            this.renderSkipTree = new SkipTree<IMGUIRenderData>();
            this.layoutResults = new LayoutResult[128];
        }


        public void SetViewportRect(Rect viewportRect) {
            this.viewportRect = viewportRect;
        }

        public void OnRender() {
            int count = layoutSystem.RunLayout(viewportRect, ref layoutResults);

            IMGUIRenderData.s_DrawRect.x = 0f;
            IMGUIRenderData.s_DrawRect.y = 0f;
            IMGUIRenderData.s_DrawRect.width = viewportRect.width;
            IMGUIRenderData.s_DrawRect.height = viewportRect.height;

            for (int i = 0; i < count; i++) {
                LayoutResult result = layoutResults[i];
                UIElement element = elementSystem.GetElement(result.elementId);

                if (element == null) continue;

                IMGUIRenderData renderData = renderSkipTree.GetItem(element);
                renderData?.SetLocalLayoutRect(result.rect);
            }

            renderSkipTree.TraverseRecursePreOrder();
        }

        public void OnInitialize() {
            styleSystem.onBorderChanged += HandleBorderChanged;
            styleSystem.onMarginChanged += HandleMarginChanged;
            styleSystem.onPaintChanged += HandlePaintChanged;
            styleSystem.onBorderRadiusChanged += HandleBorderRadiusChanged;

            IReadOnlyList<UIStyleSet> styles = styleSystem.GetActiveStyles();
            for (int i = 0; i < styles.Count; i++) {
                UIElement element = elementSystem.GetElement(styles[i].elementId);
                if (element != null) {
                    OnElementStyleChanged(element);
                }
            }
        }

        public void OnReset() {
            renderSkipTree.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() {
            foreach (KeyValuePair<Color, Texture2D> kvp in textureCache) {
                UnityEngine.Object.DestroyImmediate(kvp.Value);
            }

            styleSystem.onBorderChanged -= HandleBorderChanged;
            styleSystem.onMarginChanged -= HandleMarginChanged;
            styleSystem.onPaintChanged -= HandlePaintChanged;
            styleSystem.onBorderRadiusChanged -= HandleBorderRadiusChanged;
        }

        public void OnElementCreated(UIElementCreationData data) {
            if (data.element.style == null) return;

            UITextElement textElement = data.element as UITextElement;
            if (textElement != null) {
                textElement.onTextChanged += OnTextChanged;
            }

            OnElementStyleChanged(data.element);
        }

        private void OnTextChanged(UIElement element, string text) {
            Debug.Log("Text changed");
            IMGUIRenderData data = renderSkipTree.GetItem(element);

            // if the item isn't in our tree we don't care about it
            if (data == null) return;

            if ((data.element.flags & UIElementFlags.TextElement) == 0) {
                throw new Exception("Trying to set text on a non text element: " + element);
            }

            data.SetText(text);
           
        }

        public void OnElementEnabled(UIElement element) {
            IMGUIRenderData data = renderSkipTree.GetItem(element);
            renderSkipTree.EnableHierarchy(data);
        }

        public void OnElementDisabled(UIElement element) {
            IMGUIRenderData data = renderSkipTree.GetItem(element);
            renderSkipTree.DisableHierarchy(data);
        }

        public void OnElementDestroyed(UIElement element) {
            UITextElement textElement = element as UITextElement;
            if (textElement != null) {
                textElement.onTextChanged -= OnTextChanged;
            }

            IMGUIRenderData data = renderSkipTree.GetItem(element);
            if (data == null) return;
            renderSkipTree.RemoveItem(data);
            data.element = null;
        }

        private Texture2D GetTexture(Color color) {
            Texture2D texture;
            if (textureCache.TryGetValue(color, out texture)) {
                return texture;
            }

            texture = MakeSimpleTexture(color);
            textureCache[color] = texture;
            return texture;
        }

        public void OnElementStyleChanged(UIElement element) {
            if (element.style == null) return;

            Debug.Log("Style change");
            IMGUIRenderData data = renderSkipTree.GetItem(element);
            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);

            if (data == null) {
                if (primitiveType == RenderPrimitiveType.None) {
                    // probably not needed but just to be safe unset the flag
                    element.flags &= ~(UIElementFlags.RequiresRendering);
                    return;
                }

                element.flags |= UIElementFlags.RequiresRendering;

                data = CreateRenderData(element, primitiveType);

                renderSkipTree.AddItem(data);
                return;
            }

            if (primitiveType == data.primitiveType) {
                ApplyStyles(data);
                return;
            }

            data.primitiveType = primitiveType;

            if (primitiveType != RenderPrimitiveType.None) {
                // todo -- mask goes here I think
                return;
            }

            element.flags &= ~(UIElementFlags.RequiresRendering);

            renderSkipTree.RemoveItem(data);
            data.element = null;
        }

        private void HandleBorderRadiusChanged(int elementId, BorderRadius radius) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                IMGUIRenderData data = renderSkipTree.GetItem(element);
                if (data != null) {
                    OnElementStyleChanged(element);
                }
            }
        }

        private void HandleBorderChanged(int elementId, ContentBoxRect borderRect) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                IMGUIRenderData data = renderSkipTree.GetItem(element);
                if (data != null) {
                    OnElementStyleChanged(element);
                }
            }
        }

        private void HandleMarginChanged(int elementId, ContentBoxRect marginRect) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                IMGUIRenderData data = renderSkipTree.GetItem(element);
                if (data != null) {
                    OnElementStyleChanged(element);
                }
            }
        }

        private void HandlePaintChanged(int elementId, Paint paint) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                IMGUIRenderData data = renderSkipTree.GetItem(element);
                if (data != null) {
                    OnElementStyleChanged(element);
                }
            }
        }

        private void HandleTextChanged(int elementId, string text) { }

        private void HandleTextSizeChanged(int elementId, Vector2 size) { }

        private IMGUIRenderData CreateRenderData(UIElement element, RenderPrimitiveType primitiveType) {
            IMGUIRenderData data = new IMGUIRenderData(element, primitiveType);

            ApplyStyles(data);

            return data;
        }

        private void ApplyStyles(IMGUIRenderData data) {
            UIElement element = data.element;
            UIStyleSet style = element.style;

            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                case RenderPrimitiveType.ProceduralImage:

                    Color backgroundColor = style.backgroundColor;
                    Color borderColor = style.borderColor;
                    Texture2D backgroundImage = style.backgroundImage;
                    BorderRadius borderRadius = style.borderRadius;
                    ContentBoxRect borderSize = style.border;

                    // todo -- ref count since we need to actually call Destroy on textures
                    data.borderTexture = null;
                    data.backgroundTexture = null;

                    if (backgroundImage != null) {
                        data.backgroundTexture = backgroundImage;
                    }

                    if (backgroundColor.IsDefined()) {
                        data.backgroundTexture = GetTexture(backgroundColor);
                    }

                    if (borderRadius.IsDefined()) {
                        data.borderRadius = borderRadius;
                    }

                    if (borderColor.IsDefined()) {
                        data.borderTexture = GetTexture(borderColor);
                    }

                    if (borderSize.IsDefined()) {
                        data.borderSize = borderSize;
                    }

                    break;
                case RenderPrimitiveType.Text:
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

        private static Texture2D MakeSimpleTexture(Color color) {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

    }

}