using System;
using System.Collections.Generic;
using Src;
using Src.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Rendering {

    public class UIView {

        private List<UITransform> dirtyTransforms;
        private List<TemplateContext> contexts;
        public Type templateType;
        public Font font;

        private Dictionary<int, GameObject> gameObjects;
        private Dictionary<int, ImagePrimitive> renderables;

        public List<UIElement> renderQueue;

        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;

        public UIElement root;
        public GameObject gameObject;

        public UIView() {
            this.gameObjects = new Dictionary<int, GameObject>();
            this.renderables = new Dictionary<int, ImagePrimitive>();
            renderQueue = new List<UIElement>();
        }

        public void OnCreate() {
            root = TemplateParser.GetParsedTemplate(templateType).Instantiate(this, null, null);
        }

        public void Update() {
            // for now every element has a template context
            // I know that this is dumb, it will be addressed later
            // traverse contexts in a tree
            
            //   for (int i = 0; i < contexts.Count; i++) {
            //      contexts[i].FlushChanges();
            //    }
            
            HandleBindingChanges();
            HandleCreatedElements();
            HandleHidingElements();
            HandleShowingElements();
            HandleDestroyingElements();
            HandleRenderUpdates();
            RunLayout();
            HandleMouseEvents();
        }

        private void HandleRenderUpdates() {
            for (int i = 0; i < renderQueue.Count; i++) {
                UIElement element = renderQueue[i];
                element.flags &= ~(UIElement.UIElementFlags.RequiresRendering);
                RenderElement(element);
            }

            renderQueue.Clear();
        }

        private void HandleBindingChanges() { }

        private void HandleDestroyingElements() { }

        private void HandleCreatedElements() { }

        private void HandleHidingElements() { }

        private void HandleShowingElements() { }

        private void HandleMouseEvents() { }

        private void HandleKeyboardEvents() { }

        private void HandleFocusEvents() { }

        private void RunLayout() {
            //if nothing new, moved, destroyed, enabled, or disabled and no dirty layout styles and transforms don't run layout

            // find highest level element with children that changed
            // run layout from there instead of root -- profile 

            RectTransform rectTransform = gameObject.transform as RectTransform; // view's rect transform
            Rect available; // compute this rect from canvas size offset by view's position on canvas
            root.style.contentBox.totalWidth = rectTransform.sizeDelta.x;
            root.style.contentBox.totalHeight = rectTransform.sizeDelta.y;

            Stack<UIElement> stack = new Stack<UIElement>();
            stack.Push(root);
            while (stack.Count > 0) {
                UIElement element = stack.Pop();

                if (element.children.Count == 0) break;

                element.style.layout.Run(element);

                for (int i = 0; i < element.children.Count; i++) {
                    stack.Push(element.children[i]);
                }
            }
        }

        public void CreateListElement(UIElementTemplate template, int idx, TemplateContext context) { }

        public void MarkForRendering(List<UIElement> elements) {
            for (int i = 0; i < elements.Count; i++) {
                UIElement element = elements[i];
                // elements to render.Add(element);
            }
        }

        public void MarkForRendering(UIElement element) {
            // if element is text -> traverse font tree & find settings
            // if element is mask -> ??
            // if element is image | raw image -> apply material, background, etc
            if ((element.flags & UIElement.UIElementFlags.RequiresRendering) == 0) {
                if (element is UIText || element.style.RequiresRendering()) {
                    element.flags |= UIElement.UIElementFlags.RequiresRendering;
                    renderQueue.Add(element);
                }
            }
        }

        public void RenderElement(UIElement element) {
            if (element is UIText) {
                UIText text = (UIText) element;
                text.ApplyFontSettings(GetFontSettings(element));
            }
            else {
                ImagePrimitive image;
                // if doesn't exist, create it
                if (!renderables.TryGetValue(element.id, out image)) {
                    image = CreateImagePrimitive(element);
                    renderables[element.id] = image;
                }

                image.ApplyStyleSettings(element.style);
            }
        }

        // todo optimize w/ a font tree (using skip tree)
        public TextStyle GetFontSettings(UIElement element) {
            TextStyle retn = new TextStyle();

            retn.font = font;
            retn.fontSize = 12;
            retn.color = Color.black;
            retn.alignment = TextAnchor.MiddleLeft;
            retn.fontStyle = FontStyle.Normal;

            UIElement ptr = element;
            while (ptr != null) {
                if (ptr.style.textStyle.font != null) {
                    retn.font = ptr.style.textStyle.font;
                    break;
                }

                ptr = ptr.parent;
            }

            ptr = element;
            while (ptr != null) {
                if (ptr.style.textStyle.alignment != null) {
                    retn.alignment = ptr.style.textStyle.alignment;
                    break;
                }

                ptr = ptr.parent;
            }

            ptr = element;
            while (ptr != null) {
                if (ptr.style.textStyle.fontSize != -1) {
                    retn.fontSize = ptr.style.textStyle.fontSize;
                    break;
                }

                ptr = ptr.parent;
            }

            ptr = element;
            while (ptr != null) {
                if (ptr.style.textStyle.fontStyle != null) {
                    retn.fontStyle = ptr.style.textStyle.fontStyle;
                    break;
                }

                ptr = ptr.parent;
            }

            ptr = element;
            while (ptr != null) {
                if (ptr.style.textStyle.color != null) {
                    retn.color = ptr.style.textStyle.color;
                    break;
                }

                ptr = ptr.parent;
            }

            return retn;
        }

        public void MarkTransformDirty(UITransform uiTransform) { }

        private GameObject GetOrCreateGameObject(UIElement element) {
            GameObject obj;

            if (element == null) return gameObject;

            if (!gameObjects.TryGetValue(element.id, out obj)) {
                obj = new GameObject(GetGameObjectName(element));
                gameObjects[element.id] = obj;
                RectTransform transform = obj.AddComponent<RectTransform>();
                GameObject parentObject = GetOrCreateGameObject(element.parent);
                transform.SetParent(parentObject.transform);
                transform.anchorMin = new Vector2(0, 1);
                transform.anchorMax = new Vector2(0, 1);
                transform.pivot = new Vector2(0, 1);
                transform.anchoredPosition = new Vector2();
            }

            return obj;
        }

        public TextPrimitive CreateTextPrimitive(UIElement element) {
            GameObject obj = GetOrCreateGameObject(element);
            Text textComponent = obj.AddComponent<Text>();
            return new UnityTextPrimitive(textComponent);
        }

        public UnityImagePrimitive CreateImagePrimitive(UIElement element) {
            GameObject obj = GetOrCreateGameObject(element);
            RawImage imageComponent = obj.AddComponent<RawImage>();
            UnityImagePrimitive imagePrimitive = new UnityImagePrimitive(imageComponent);
            renderables[element.id] = imagePrimitive;
            return imagePrimitive;
        }

        private static string GetGameObjectName(UIElement element) {
            return "UIElement_" + element.GetType().Name + " " + element.id;
        }

        public void RegisterStyle(UIElement element, object computeInitialStyle) {
            throw new NotImplementedException();
        }

    }

}