using System;
using System.Collections.Generic;
using Src;
using Src.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Rendering {

    public class UIView {

        public Type templateType;
        public Font font;

        private Dictionary<int, GameObject> gameObjects;

        public List<UIElement> renderQueue;
        public SkipTree<TemplateBinding> bindingSkipTree;

        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;

        public UIElement root;
        public GameObject gameObject;

        private RectTransform rectTransform;

        public UIView() {
            gameObjects = new Dictionary<int, GameObject>();
            renderQueue = new List<UIElement>();
        }

        public void Refresh() {

            foreach (KeyValuePair<int, GameObject> go in gameObjects) {
                UnityEngine.Object.Destroy(go.Value);
            }

            gameObjects.Clear();
            renderQueue.Clear();
//            bindingSkipTree = new SkipTree<TemplateBinding>();

            root = TemplateParser.GetParsedTemplate(templateType, true).CreateWithoutScope(this);
            InitializeElements();
        }

        public void OnCreate() {
            root = TemplateParser.GetParsedTemplate(templateType).CreateWithoutScope(this);
            rectTransform = gameObject.transform as RectTransform;
            InitializeElements();
        }

        public void Update() {
            HandleBindingChanges();
            HandleCreatedElements();
            HandleHidingElements();
            HandleShowingElements();
            HandleDestroyingElements();
            HandleRenderUpdates();
            RunLayout();
            HandleMouseEvents();
        }

        private void InitializeElements() {
            UIElement.Traverse(root, (element) => {

                UITextElement textElement = element as UITextElement;

                if (textElement != null) {
                    CreateTextPrimitive(textElement);
                }
                // todo -- skip tree for render nodes?
                // todo -- debug view for tree?
                else if (true) { //element.style.RequiresRendering()) {
                    CreateImagePrimitive(element);
                }
                else if (element is UIRepeatElement) {
                    // create children based on bindings
                }

            });
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

            Rect viewport = rectTransform.rect; // compute this rect from canvas size offset by view's position on canvas

            Stack<LayoutData> layoutData = new Stack<LayoutData>();

            layoutData.Push(root.style.layout.Run(viewport, null, root));
            
            float xOffset = 0;
            float yOffset = 0;

            while (layoutData.Count != 0) {
                LayoutData data = layoutData.Pop();

                data.x += xOffset;
                data.y += yOffset;
                // convert to world space
                // apply to unity transforms
                // add / update masks
                // add / update scroll views

                SetLayoutRect(data.elementId, data.layoutRect);

                for (int i = 0; i < data.children.Count; i++) {
                    layoutData.Push(data.children[i]);
                }
            }

        }

        public void SetLayoutRect(int id, Rect rect) {
            GameObject obj;
            if (gameObjects.TryGetValue(id, out obj)) {
                RectTransform rectTransform = obj.transform as RectTransform;
                rectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
                rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
            }
        }

        public void MarkForRendering(UIElement element) { }

        public void RenderElement(UIElement element) {
//            if (element is UITextElement) {
//                UITextElement textElement = (UITextElement) element;
//                textElement.ApplyFontSettings(GetFontSettings(element));
//            }
//            else {
//                ImagePrimitive image;
//                // if doesn't exist, create it
//                // todo remove this
//                if (!renderables.TryGetValue(element.id, out image)) {
//                    image = CreateImagePrimitive(element);
//                    renderables[element.id] = image;
//                }
//
//                image.ApplyStyleSettings(element.style);
//            }
        }

        public TextStyle GetFontSettings(UIElement element) {
            TextStyle retn = new TextStyle();

            retn.font = font;
            retn.fontSize = 12;
            retn.color = Color.black;
            retn.alignment = TextAnchor.MiddleLeft;
            retn.fontStyle = FontStyle.Normal;
            retn.verticalOverflow = VerticalWrapMode.Overflow;
            retn.horizontalOverflow = HorizontalWrapMode.Overflow;

            return retn;
        }

        private GameObject GetOrCreateGameObject(UIElement element) {
            GameObject obj;

            if (element == null) return gameObject;

            if (gameObjects.TryGetValue(element.id, out obj)) {
                return obj;
            }

            obj = new GameObject(GetGameObjectName(element));
            gameObjects[element.id] = obj;
            RectTransform transform = obj.AddComponent<RectTransform>();
            GameObject parentObject = GetOrCreateGameObject(element.parent);
            transform.SetParent(parentObject.transform);
            transform.anchorMin = new Vector2(0, 1);
            transform.anchorMax = new Vector2(0, 1);
            transform.pivot = new Vector2(0, 1);
            transform.anchoredPosition = new Vector2();

            return obj;
        }

        public void CreateTextPrimitive(UITextElement textElement) {
            GameObject obj = GetOrCreateGameObject(textElement);
            Text textComponent = obj.AddComponent<Text>();
            UnityTextPrimitive textPrimitive = new UnityTextPrimitive(textComponent);
//            renderables[textElement.id] = textPrimitive;
            // stitch up parent reference since text might be created before we have the actual parent
            gameObjects[textElement.id].transform.SetParent(gameObjects[textElement.parent.id].transform);
            textElement.textRenderElement = textPrimitive;
            textElement.ApplyFontSettings(GetFontSettings(textElement));
        }

        public UnityImagePrimitive CreateImagePrimitive(UIElement element) {
            GameObject obj = GetOrCreateGameObject(element);
            ProceduralImage imageComponent = obj.AddComponent<ProceduralImage>();
            UnityImagePrimitive imagePrimitive = new UnityImagePrimitive(imageComponent);
            imageComponent.color = element.style.backgroundColor;
            return imagePrimitive;
        }

        private static string GetGameObjectName(UIElement element) {
            return "<" + element.GetType().Name + "> " + element.id;
        }

        public void RegisterBindings(UIElement element, Binding[] bindings, TemplateContext context) {
//            if (bindings.Length == 0) return;
//            bindingSkipTree.AddItem(new TemplateBinding(element, bindings, context));
        }

        public void SetEnabled(UIElement element, bool isEnabled) {
            throw new NotImplementedException();
        }

    }

}