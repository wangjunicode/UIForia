using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Systems {

    public class IMGUIRenderSystem : IRenderSystem {

        private readonly IStyleSystem styleSystem;
        private readonly LayoutSystem layoutSystem;
        private readonly SkipTree<IMGUIRenderData> renderSkipTree;
        private LayoutResult[] layoutResults;
        private Rect viewportRect;

        // temp
        private readonly Dictionary<int, UIElement> elementMap;

        public IMGUIRenderSystem(IStyleSystem styleSystem, LayoutSystem layoutSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;

            this.renderSkipTree = new SkipTree<IMGUIRenderData>();
            this.layoutResults = new LayoutResult[128];
            this.elementMap = new Dictionary<int, UIElement>();
        }

        private Texture2D MakeSimpleTexture(Color color) {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public void SetViewportRect(Rect viewportRect) {
            this.viewportRect = viewportRect;
        }

        public void OnRender() {
            int count = layoutSystem.RunLayout(viewportRect, ref layoutResults);

            IMGUIRenderData.s_DrawRect = viewportRect;

            for (int i = 0; i < count; i++) {
                LayoutResult result = layoutResults[i];
                UIElement element;
                if (elementMap.TryGetValue(result.elementId, out element)) {
                    renderSkipTree.GetItem(element).SetLocalLayoutRect(result.rect);
                }
            }

            renderSkipTree.TraverseRecursePreOrder();
        }

        public void OnInitialize() { }

        public void OnReset() {
            renderSkipTree.Clear();
            elementMap.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() { }

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

            elementMap.Remove(element.id);
            IMGUIRenderData data = renderSkipTree.GetItem(element);
            if (data == null) return;
            renderSkipTree.RemoveItem(data);
            data.element = null;
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

                GUIStyle style = GUIStyleFromStyleSet(element.style);

                data = new IMGUIRenderData(element, primitiveType, style);
                data.texture = MakeSimpleTexture(element.style.backgroundColor);

                renderSkipTree.AddItem(data);
                elementMap[element.id] = element;
                return;
            }

            if (primitiveType == data.primitiveType) {
                ApplyStyles(data);
                return;
            }

            data.primitiveType = primitiveType;

            if (primitiveType != RenderPrimitiveType.None) {
                return;
            }

            element.flags &= ~(UIElementFlags.RequiresRendering);

            renderSkipTree.RemoveItem(data);
            elementMap.Remove(data.element.id);
            data.element = null;
        }

        private GUIStyle GUIStyleFromStyleSet(UIStyleSet styleSet) {
            GUIStyle retn = new GUIStyle();
            return retn;
        }

        private void ApplyStyles(IMGUIRenderData data) {
            UIElement element = data.element;
            UIStyleSet style = element.style;

            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                case RenderPrimitiveType.ProceduralImage:
                    data.backgroundColor = style.backgroundColor;
                    break;
                case RenderPrimitiveType.Text:
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