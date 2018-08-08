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

        private Dictionary<int, GameObject> gameObjects;

        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;

        private UIElement root;
        
        public void OnCreate() {
            UIElementTemplate template = TemplateParser.GetParsedTemplate(templateType);
            root = template.CreateElement(this);
        }

        public void Update() {
            for (int i = 0; i < contexts.Count; i++) {
                contexts[i].FlushChanges();
            }
            HandleBindingChanges();
            HandleCreatedElements();
            HandleHidingElements();
            HandleShowingElements();
            HandleDestroyingElements();
            RunLayout();
            HandleMouseEvents();
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
            
            UILayout layout = root.layout;
            RectTransform rectTransform; // view's rect transform
            Rect available; // compute this rect from canvas size offset by view's position on canvas
            layout.Run(/*rect*/);

            Stack<UIElement> stack = new Stack<UIElement>();
            while (stack.Count > 0) {
                UIElement element = stack.Pop();
                
                if (element.style.layoutType == LayoutType.Flexbox) {
                    element.style.ApplyLayout();
                }
                else if (element.style.layoutType == LayoutType.None) {
                    
                }

                for (int i = 0; i < element.children.Length; i++) {
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
        
        public void MarkTransformDirty(UITransform uiTransform) { }

        public TextPrimitive CreateTextPrimitive(UIElement element) {
            GameObject obj;
            // todo -- don't allow duplicates
            
            if (!gameObjects.TryGetValue(element.id, out obj)) {
                obj = new GameObject(GetGameObjectName(element));
                gameObjects[element.id] = obj;
            }

            obj.AddComponent<RectTransform>();
            Text textComponent = obj.AddComponent<Text>();
            // todo -- figure out where this belongs in the hierarchy
            return new UnityTextPrimitive(textComponent);
        }

        public UnityImagePrimitive CreateImagePrimitive(UIElement element) {
            GameObject obj;
            // todo -- don't allow duplicates
            
            if (!gameObjects.TryGetValue(element.id, out obj)) {
                obj = new GameObject(GetGameObjectName(element));
                gameObjects[element.id] = obj;
            }

            obj.AddComponent<RectTransform>();
            RawImage imageComponent = obj.AddComponent<RawImage>();
            // todo -- figure out where this belongs in the hierarchy
            return new UnityImagePrimitive(imageComponent);
        }

        private static string GetGameObjectName(UIElement element) {
            return "UIElement_" + element.GetType().Name + " " + element.id;
        }
    }

}