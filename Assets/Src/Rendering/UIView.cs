using System;
using System.Collections.Generic;
using Src;
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
        public SkipTree<TemplateBinding> bindingSkipTree;

        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;

        public UIElement root;
        public GameObject gameObject;

        public UIView() {
            gameObjects = new Dictionary<int, GameObject>();
            renderables = new Dictionary<int, ImagePrimitive>();
            renderQueue = new List<UIElement>();
        }

        public void OnCreate() {
            root = TemplateParser.GetParsedTemplate(templateType).CreateWithoutScope(this);

            Stack<UIElement> stack = new Stack<UIElement>();
            stack.Push(root);
            while (stack.Count > 0) {
                UIElement element = stack.Pop();

                if (element is UITextElement) {
                    UITextElement textElement = element as UITextElement;
                    CreateTextPrimitive(textElement);
                    // stitch up parent reference since text might be created before we have the actual parent
                    gameObjects[element.id].transform.SetParent(gameObjects[element.parent.id].transform);
                    ((UITextElement) element).ApplyFontSettings(GetFontSettings(element));

                }
                else if (element.style.RequiresRendering()) {
                    renderables[element.id] = CreateImagePrimitive(element);
                }
                else if (element is UIRepeatElement) {
                    // create children based on bindings
                }

                if (element.HasChildren) {
                    for (int i = 0; i < element.children.Count; i++) {
                        stack.Push(element.children[i]);
                    }
                }
            }

            // traverse -> create unity elements based on type
            // traverse -> call OnInitialize()
            // traverse -> call OnEnable()
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
           // root.style.contentBox.contentWidth = rectTransform.sizeDelta.x;
           // root.style.contentBox.contentHeight = rectTransform.sizeDelta.y;

            Stack<UIElement> stack = new Stack<UIElement>();
            stack.Push(root);
            while (stack.Count > 0) {
                UIElement element = stack.Pop();

                if (element.children.Count == 0) break;

            //    element.style.layout.Run(element);

                for (int i = 0; i < element.children.Count; i++) {
                    stack.Push(element.children[i]);
                }
            }
        }

        /*
         *
         * Creating render items
         *     3 types -> image-type, mask-type, text-type
         *     image-type is created based on style parameters
         *     text-type is created based on text content changes
         * 
         */

        public void MarkForRendering(UIElement element) {
           
           
        }

        public void RenderElement(UIElement element) {
            if (element is UITextElement) {
                UITextElement textElement = (UITextElement) element;
                textElement.ApplyFontSettings(GetFontSettings(element));
            }
            else {
                ImagePrimitive image;
                // if doesn't exist, create it
                // todo remove this
                if (!renderables.TryGetValue(element.id, out image)) {
                    image = CreateImagePrimitive(element);
                    renderables[element.id] = image;
                }

                image.ApplyStyleSettings(element.style);
            }
        }

        public struct TextStyleNode {

            public int id;
            public List<UIElement> dependents;
            
        }
        
        /*
         * need to handle changes in the ancestry
         * when a text style changes
         * traverse the tree from that node downwards
         *  if a child node was listening to parent node move the pointer
         *
         *
         *  a text style higher up can change causing children to re-calculate
         *  a text style on 'this' node can change
         *
         *  
         */
        
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

        public void MarkTransformDirty(UITransform uiTransform) { }

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
            UnityTextPrimitive textPrimitive = new UnityTextPrimitive(textComponent);;
//            renderables[textElement.id] = textPrimitive;
            textElement.textRenderElement = textPrimitive;
        }

        public UnityImagePrimitive CreateImagePrimitive(UIElement element) {
            GameObject obj = GetOrCreateGameObject(element);
            ProceduralImage imageComponent = obj.AddComponent<ProceduralImage>();
            UnityImagePrimitive imagePrimitive = new UnityImagePrimitive(imageComponent);
            renderables[element.id] = imagePrimitive;
            return imagePrimitive;
        }

        private static string GetGameObjectName(UIElement element) {
            return "<" + element.GetType().Name + "> " + element.id;
        }

        public void RegisterBindings(UIElement element, Binding[] bindings, TemplateContext context) {
            if (bindings.Length == 0) return;
            bindingSkipTree.AddItem(new TemplateBinding(element, bindings, context));
        }

        public void SetEnabled(UIElement element, bool isEnabled) {
            throw new NotImplementedException();
        }

    }

}