using System;
using System.Collections.Generic;
using System.Reflection;
using Rendering;
using Src.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Src.Systems {

    public class IMGUIRenderSystem : IRenderSystem {

        private readonly IStyleSystem styleSystem;
        private readonly ILayoutSystem layoutSystem;
        private readonly IElementRegistry elementSystem;
        private readonly SkipTree<IMGUIRenderData> renderSkipTree;
        private readonly Dictionary<Color, Texture2D> textureCache;

        private bool isReady;

        public IMGUIRenderSystem(IElementRegistry elementSystem, IStyleSystem styleSystem, ILayoutSystem layoutSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;
            this.elementSystem = elementSystem;

            this.textureCache = new Dictionary<Color, Texture2D>();
            this.renderSkipTree = new SkipTree<IMGUIRenderData>();
        }

        public void OnRender() {
            if (Event.current.type != EventType.Repaint) return;
            int count = layoutSystem.RectCount;
            LayoutResult[] layoutResults = layoutSystem.LayoutResults;

            for (int i = 0; i < count; i++) {
                LayoutResult result = layoutResults[i];
                UIElement element = elementSystem.GetElement(result.elementId);

                if (element == null) continue;

                IMGUIRenderData data = renderSkipTree.GetItem(element);
                data?.SetLocalLayoutRect(result.rect);
            }

            renderSkipTree.ConditionalTraversePreOrder((renderData) => {
                if (renderData.element.isDisabled) {
                    return false;
                }

                RenderPrimitiveType primitiveType = renderData.primitiveType;

                Rect paintRect = new Rect(renderData.layoutRect);
                paintRect.x += renderData.element.style.marginLeft;
                paintRect.y += renderData.element.style.marginTop;
                paintRect.width -= renderData.element.style.marginRight + renderData.element.style.marginLeft;
                paintRect.height -= renderData.element.style.marginBottom + renderData.element.style.marginTop;

                switch (primitiveType) {
                    case RenderPrimitiveType.RawImage:
                    case RenderPrimitiveType.ProceduralImage:

                        if (renderData.borderTexture != null && renderData.borderSize.IsDefined()) {
                            if (renderData.backgroundTexture != null) {
                                Rect innerRect = new Rect(paintRect);
                                innerRect.x += renderData.borderSize.left;
                                innerRect.y += renderData.borderSize.top;
                                innerRect.width -= renderData.borderSize.right + renderData.borderSize.left;
                                innerRect.height -= renderData.borderSize.bottom + renderData.borderSize.top;
                                GUI.DrawTexture(innerRect, renderData.backgroundTexture);
                            }

                            // undocumented in Unity, only draws a border!
                            GUI.DrawTexture(
                                paintRect,
                                renderData.borderTexture,
                                ScaleMode.ScaleToFit,
                                true,
                                paintRect.width / paintRect.height,
                                Color.white,
                                renderData.borderSize,
                                renderData.borderRadius
                            );
                        }
                        else {
                            if (renderData.backgroundTexture != null) {
                                GUI.DrawTexture(paintRect, renderData.backgroundTexture);
                            }
                        }

                        break;

                    case RenderPrimitiveType.Text:
                        GUIStyle style = new GUIStyle();
                        style.font = renderData.element.style.font;
                        style.fontSize = renderData.element.style.fontSize;
                        style.fontStyle = renderData.element.style.fontStyle;
                        style.alignment = renderData.element.style.textAnchor;
                        style.wordWrap = true;
                        GUI.Label(renderData.layoutRect, renderData.textContent, style);
//                        MethodInfo info = typeof(GUIStyle).GetMethod("DrawWithTextSelection", BindingFlags.Instance | BindingFlags.NonPublic,
//                            null,
//                            new [] {
//                                typeof(Rect), typeof(GUIContent), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool)
//                            }, null);
//                        info.Invoke(style, new object[] {
//                            renderData.layoutRect,
//                            new GUIContent(renderData.textContent),
//                            true, true, 0, 5, false
//                        });
//                        style.DrawWithTextSelection(renderData.layoutRect, new GUIContent(renderData.textContent), 0, 0, 5);
                        break;
                }

                return true;
            });
        }

        public void OnReady() {
            isReady = true;
            styleSystem.onBorderChanged += HandleBorderChanged;
            styleSystem.onMarginChanged += HandleMarginChanged;
            styleSystem.onPaintChanged += HandlePaintChanged;
            styleSystem.onBorderRadiusChanged += HandleBorderRadiusChanged;
            styleSystem.onTextContentChanged += HandleTextChanged;
            renderSkipTree.TraversePreOrder(this, (self, renderData) => {
                renderData.primitiveType = self.DeterminePrimitiveType(renderData.element);
                self.ApplyStyles(renderData);
            });
        }

        public void OnInitialize() { }

        public void OnReset() {
            OnDestroy();
        }

        public void OnUpdate() {
            OnRender();
        }

        public void OnDestroy() {
            foreach (KeyValuePair<Color, Texture2D> kvp in textureCache) {
                Object.DestroyImmediate(kvp.Value);
            }

            styleSystem.onBorderChanged -= HandleBorderChanged;
            styleSystem.onMarginChanged -= HandleMarginChanged;
            styleSystem.onPaintChanged -= HandlePaintChanged;
            styleSystem.onBorderRadiusChanged -= HandleBorderRadiusChanged;
            styleSystem.onFontPropertyChanged -= HandleFontPropertyChanged;
            styleSystem.onTextContentChanged -= HandleTextChanged;
            renderSkipTree.Clear();
            textureCache.Clear();
        }

        public void OnElementCreated(InitData data) {
            if ((data.element.flags & UIElementFlags.RequiresRendering) != 0) {
                IMGUIRenderData renderData = new IMGUIRenderData(data.element);
                renderSkipTree.AddItem(renderData);
                if (isReady) {
                    renderData.primitiveType = DeterminePrimitiveType(renderData.element);
                    ApplyStyles(renderData);
                }
            }

            for (int i = 0; i < data.children.Count; i++) {
                OnElementCreated(data.children[i]);
            }
        }

        private void HandleTextChanged(int elementId, string text) {
            // todo -- add metrics per-element on text changes
            IMGUIRenderData data = renderSkipTree.GetItem(elementId);

            if (data == null) {
                return;
            }
            // if the item isn't in our tree we don't care about it

            if ((data.element.flags & UIElementFlags.TextElement) == 0) {
                throw new Exception("Trying to set text on a non text element: " + elementId);
            }

            data.SetText(text);
        }

        public void OnElementEnabled(UIElement element) {
            // render skip tree handles this, no need to do anything fancy
        }

        public void OnElementDisabled(UIElement element) {
            // render skip tree handles this, no need to do anything fancy
        }

        public void OnElementDestroyed(UIElement element) {
            renderSkipTree.RemoveHierarchy(element);
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        private Texture2D GetTexture(Color color) {
            Texture2D texture;
            if (textureCache.TryGetValue(color, out texture)) {
                return texture;
            }

            texture = MakeSimpleTexture(color);
            textureCache[color] = texture;
            return texture;
        }

        private void OnElementStyleChanged(UIElement element) {
            IMGUIRenderData data = renderSkipTree.GetItem(element);
            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);

            if (data == null) {
                if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                    return;
                }

                data = new IMGUIRenderData(element);
                renderSkipTree.AddItem(data);
                data.primitiveType = primitiveType;
                ApplyStyles(data);
                return;
            }

            if (primitiveType == data.primitiveType) {
                ApplyStyles(data);
                return;
            }

            data.primitiveType = primitiveType;
            // todo -- mask goes here I think
        }

        private void HandleBorderRadiusChanged(int elementId, BorderRadius radius) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                OnElementStyleChanged(element);
            }
        }

        private void HandleBorderChanged(int elementId, ContentBoxRect borderRect) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                OnElementStyleChanged(element);
            }
        }

        private void HandleMarginChanged(int elementId, ContentBoxRect marginRect) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                OnElementStyleChanged(element);
            }
        }

        private void HandlePaintChanged(int elementId, Paint paint) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                OnElementStyleChanged(element);
            }
        }

        private void HandleFontPropertyChanged(int elementId, TextStyle textStyle) {
            UIElement element = elementSystem.GetElement(elementId);
            if (element != null) {
                IMGUIRenderData data = renderSkipTree.GetItem(element);
                // todo -- change to use font tree
                data?.SetFontProperties(textStyle);
            }
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
                    UITextElement textElement = (UITextElement) data.element;
                    data.textContent = textElement.GetText();
                    break;
            }
        }

        private RenderPrimitiveType DeterminePrimitiveType(UIElement element) {
            if ((element.flags & UIElementFlags.TextElement) != 0) {
                return RenderPrimitiveType.Text;
            }

            UIStyleSet styleSet = element.style;
            if (styleSet.backgroundImage == null
                && styleSet.borderColor == ColorUtil.UnsetValue
                && styleSet.backgroundColor == ColorUtil.UnsetValue) {
                return RenderPrimitiveType.None;
            }

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