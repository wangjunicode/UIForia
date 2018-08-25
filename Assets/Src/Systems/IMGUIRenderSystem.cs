using System;
using System.Collections.Generic;
using Rendering;
using Src.Extensions;
using UnityEngine;

namespace Src.Systems {

//    public class FontTree {
//
//        public class FontNode : ISkipTreeTraversable {
//
//            public UIElement element;
//
//            public TextData textData;
//            
//            public IHierarchical Element => element;
//            public IHierarchical Parent => element.parent;
//
//            public void OnParentChanged(ISkipTreeTraversable newParent) {
//                
//            }
//
//            public void OnBeforeTraverse() {
//                
//            }
//
//            public void OnAfterTraverse() {
//                
//            }
//
//        }
//
//        private readonly IStyleSystem styleSystem;
//        private readonly SkipTree<FontNode> fontTree;
//        
//        public FontTree(IStyleSystem styleSystem) {
//            this.styleSystem = styleSystem;
//        }
//
//        public TextStyle GetTextStyle(UIElement element) {
//            
//            // start at nearest parent element in tree
//            // walk tree assigning properties that are not set
//            // when all properties set (via a counter check)
//            // return 
//            // root will define defaults
//            return default(TextStyle);
//        }
//        
//
//    }

    public class IMGUIRenderSystem : IRenderSystem {

        private readonly IStyleSystem styleSystem;
        private readonly ILayoutSystem layoutSystem;
        private readonly IElementRegistry elementSystem;
        private readonly SkipTree<IMGUIRenderData> renderSkipTree;
        private readonly Dictionary<Color, Texture2D> textureCache;

        private Rect viewportRect;
        private IMGUIRenderData[] renderData;
        private bool renderDataDirty;

        public IMGUIRenderSystem(IElementRegistry elementSystem, IStyleSystem styleSystem, ILayoutSystem layoutSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;
            this.elementSystem = elementSystem;

            this.textureCache = new Dictionary<Color, Texture2D>();
            this.renderSkipTree = new SkipTree<IMGUIRenderData>();
            this.renderDataDirty = true;
        }

        public void SetViewportRect(Rect viewportRect) {
            this.viewportRect = viewportRect;
        }

        public void OnRender() {
            int count = layoutSystem.RectCount;
            LayoutResult[] layoutResults = layoutSystem.LayoutResults;

            //if (renderDataDirty) {
            //   renderDataDirty = false;
            renderData = renderSkipTree.ToArray();
            //}            

            IMGUIRenderData.s_DrawRect.x = 0f;
            IMGUIRenderData.s_DrawRect.y = 0f;
            IMGUIRenderData.s_DrawRect.width = viewportRect.width;
            IMGUIRenderData.s_DrawRect.height = viewportRect.height;

            for (int i = 0; i < count; i++) {
                LayoutResult result = layoutResults[i];
                UIElement element = elementSystem.GetElement(result.elementId);

                if (element == null) continue;

                IMGUIRenderData data = renderSkipTree.GetItem(element);
                data?.SetLocalLayoutRect(result.rect);
            }

            for (int i = 0; i < renderData.Length; i++) {
                IMGUIRenderData data = renderData[i];
                RenderPrimitiveType primitiveType = data.primitiveType;

                Rect paintRect = new Rect(data.layoutRect);
                paintRect.x += data.element.style.marginLeft;
                paintRect.y += data.element.style.marginTop;
                paintRect.width -= data.element.style.marginRight;
                paintRect.height -= data.element.style.marginBottom;

                switch (primitiveType) {
                    case RenderPrimitiveType.RawImage:
                    case RenderPrimitiveType.ProceduralImage:

                        if (data.borderTexture != null && data.borderSize.IsDefined()) {
                            if (data.backgroundTexture != null) {
                                Rect innerRect = new Rect(paintRect);
                                innerRect.x += data.borderSize.left;
                                innerRect.y += data.borderSize.top;
                                innerRect.width -= data.borderSize.right * 2f;
                                innerRect.height -= data.borderSize.bottom * 2f;
                                GUI.DrawTexture(innerRect, data.backgroundTexture);
                            }

                            // undocumented in Unity, only draws a border!
                            GUI.DrawTexture(
                                paintRect,
                                data.borderTexture,
                                ScaleMode.ScaleToFit,
                                true,
                                paintRect.width / paintRect.height,
                                Color.white,
                                data.borderSize,
                                data.borderRadius
                            );
                        }
                        else {
                            if (data.backgroundTexture != null) {
                                GUI.DrawTexture(paintRect, data.backgroundTexture);
                            }
                        }

                        break;

                    case RenderPrimitiveType.Text:
                        GUIStyle style = new GUIStyle();
                        style.fontSize = 12;
                        style.wordWrap = true;
                        GUI.Label(data.layoutRect, data.textContent, style);
                        break;
                }

            }

//            renderSkipTree.TraverseRecursePreOrder();
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
            renderData = null;
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
            styleSystem.onFontPropertyChanged -= HandleFontPropertyChanged;
            renderData = null;
            renderSkipTree.Clear();
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
//            Debug.Log("Text changed");
            // todo -- add metrics per-element on text changes
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

//            Debug.Log("Style change");
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