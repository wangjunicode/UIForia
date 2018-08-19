using System;
using System.Collections.Generic;
using Rendering;
using Src.Layout;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Src.Systems {

    public enum RenderPrimitiveType {

        None,
        RawImage,
        ProceduralImage,
        Mask,
        Mask2D,
        Text

    }

    public struct StyleOperation {

        public enum OpType {

            BackgroundColor,
            BackgroundImage,
            Padding,
            Margin,
            Border,
            BorderColor,
            BorderRadius,
            Opacity,
            Shadow,

        }

    }

    public class RenderData : ISkipTreeTraversable {

        public UIElement element;
        public RectTransform rootTransform;
        public RectTransform unityTransform;
        public Graphic maskComponent;
        public Graphic imageComponent;
        public RenderPrimitiveType primitiveType;

        public RenderData(UIElement element, RenderPrimitiveType primitiveType, RectTransform unityTransform, RectTransform rootTransform) {
            this.element = element;
            this.primitiveType = primitiveType;
            this.unityTransform = unityTransform;
            this.rootTransform = rootTransform;
        }

        public void OnParentChanged(ISkipTreeTraversable newParent) {
            RenderData parent = (RenderData) newParent;
            if (parent == null) {
                unityTransform.SetParent(rootTransform);
            }
            else {
                unityTransform.SetParent(parent.unityTransform);
            }
        }

        public void OnBeforeTraverse() { }

        public void OnAfterTraverse() { }

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

    public class RenderSystem {

        private Dictionary<int, GameObject> gameObjects;
        private Dictionary<int, RawImage> rawImages;
        private Dictionary<int, Text> texts;
        private Dictionary<int, ProceduralImage> proceduralImages;
        private Dictionary<int, Mask> masks;
        private Dictionary<int, RectMask2D> masks2d;

        public GameObject gameObject;

        public SkipTree<LayoutData> layoutTree;
        public SkipTree<RenderData> renderSkipTree;

        public Font font; // temp

        public RenderSystem(GameObject gameObject) {
            this.gameObject = gameObject;
            this.renderSkipTree = new SkipTree<RenderData>();
        }

        public void Update() {
            
            layoutTree.TraversePreOrderWithCallback((data => {
                data.children.Clear();
                data.parent?.children.Add(data);
            }));

            layoutTree.TraversePreOrderWithCallback((data => {
                data.layout.Run(new Rect(0, 0, 1000, 1000), data.parent);
            }));

        }

        public void Reset() {
            renderSkipTree = new SkipTree<RenderData>();
            foreach (KeyValuePair<int, GameObject> go in gameObjects) {
                Object.Destroy(go.Value);
            }

            gameObjects.Clear();

        }

        public void RegisterStyleStateChange(UIElement element) {

            RenderData data = renderSkipTree.GetItem(element);

            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);

            if (data == null) {

                if (primitiveType == RenderPrimitiveType.None) {
                    return;
                }

                GameObject obj = new GameObject(GetGameObjectName(element));
                RectTransform unityTransform = obj.AddComponent<RectTransform>();
                unityTransform.anchorMin = new Vector2(0, 1);
                unityTransform.anchorMax = new Vector2(0, 1);
                unityTransform.pivot = new Vector2(0, 1);
                unityTransform.anchoredPosition = new Vector2();
                data = new RenderData(element, primitiveType, unityTransform, (RectTransform) gameObject.transform);
                renderSkipTree.AddItem(data);
                CreateComponents(data);

                return;
            }

            if (primitiveType == data.primitiveType) {
                ApplyStyles(data);
                return;
            }

            // todo -- pool
            if (data.imageComponent != null) {
                Object.Destroy(data.imageComponent);
            }

            if (data.maskComponent != null) {
                Object.Destroy(data.maskComponent);
            }

            if (primitiveType == RenderPrimitiveType.None) {
                // todo -- pool
                renderSkipTree.RemoveItem(data);
                Object.Destroy(data.unityTransform);
                data.rootTransform = null;
                data.element = null;
                return;
            }

            CreateComponents(data);

        }

        private void CreateComponents(RenderData data) {
            switch (data.primitiveType) {
                case RenderPrimitiveType.RawImage:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<RawImage>();
                    ApplyStyles(data);
                    return;
                case RenderPrimitiveType.ProceduralImage:
                    data.imageComponent = data.unityTransform.gameObject.AddComponent<RawImage>();
                    ApplyStyles(data);
                    return;
                case RenderPrimitiveType.Mask:
                case RenderPrimitiveType.Mask2D:
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
                case RenderPrimitiveType.ProceduralImage:
                    ProceduralImage procImage = (ProceduralImage) data.imageComponent;
                    procImage.color = style.backgroundColor;
                    break;
                case RenderPrimitiveType.Text:
                    Text text = (Text) data.imageComponent;
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

            if (styleSet.borderRadius != UIStyle.UnsetFloatValue) {
                return RenderPrimitiveType.ProceduralImage;
            }

            return RenderPrimitiveType.RawImage;

        }

        public void Destroy(UIElement element) {
            RenderData data = renderSkipTree.GetItem(element);

            if (data == null) return;

            if (data.imageComponent != null) {
                Object.Destroy(data.imageComponent);
            }

            if (data.maskComponent != null) {
                Object.Destroy(data.maskComponent);
            }

            renderSkipTree.RemoveItem(data);
            Object.Destroy(data.unityTransform);
            data.rootTransform = null;
            data.element = null;
        }

        public void SetEnabled(UIElement element, bool enabled) {
            RenderData data = renderSkipTree.GetItem(element);
            if (enabled) {
                renderSkipTree.EnableHierarchy(data);
            }
            else {
                renderSkipTree.DisableHierarchy(data);
            }
        }

        private bool RequiresRawImage(UIStyleSet styleSet) {
            return styleSet.backgroundImage != null
                   || styleSet.backgroundColor != UIStyle.UnsetColorValue;
        }

//        public void CreateTextPrimitive(UITextElement textElement) {
//            GameObject obj = GetOrCreateGameObject(textElement);
//            Text textComponent = obj.AddComponent<Text>();
//            UnityTextPrimitive textPrimitive = new UnityTextPrimitive(textComponent);
////            renderables[textElement.id] = textPrimitive;
//            // stitch up parent reference since text might be created before we have the actual parent
//            gameObjects[textElement.id].transform.SetParent(gameObjects[textElement.parent.id].transform);
//            textElement.textRenderElement = textPrimitive;
//            textElement.ApplyFontSettings(GetFontSettings(textElement));
//        }
//
//        public UnityImagePrimitive CreateImagePrimitive(UIElement element) {
//            GameObject obj = GetOrCreateGameObject(element);
//            ProceduralImage imageComponent = obj.AddComponent<ProceduralImage>();
//            UnityImagePrimitive imagePrimitive = new UnityImagePrimitive(imageComponent);
//            imageComponent.color = element.style.backgroundColor;
//            return imagePrimitive;
//        }

//        public TextStyle GetFontSettings(UIElement element) {
//            TextStyle retn = new TextStyle();
//
//            retn.font = font;
//            retn.fontSize = 12;
//            retn.color = Color.black;
//            retn.alignment = TextAnchor.MiddleLeft;
//            retn.fontStyle = FontStyle.Normal;
//            retn.verticalOverflow = VerticalWrapMode.Overflow;
//            retn.horizontalOverflow = HorizontalWrapMode.Overflow;
//
//            return retn;
//        }

//        private GameObject GetOrCreateGameObject(UIElement element) {
//            GameObject obj;
//
//            if (element == null) return gameObject;
//
//            if (gameObjects.TryGetValue(element.id, out obj)) {
//                return obj;
//            }
//
//            obj = new GameObject(GetGameObjectName(element));
//            gameObjects[element.id] = obj;
//            RectTransform transform = obj.AddComponent<RectTransform>();
//            GameObject parentObject = GetOrCreateGameObject(element.parent);
//            transform.SetParent(parentObject.transform);
//            transform.anchorMin = new Vector2(0, 1);
//            transform.anchorMax = new Vector2(0, 1);
//            transform.pivot = new Vector2(0, 1);
//            transform.anchoredPosition = new Vector2();
//
//            return obj;
//        }

        public void SetRectWidth(UIElement element, UIMeasurement width) {
            layoutTree.GetItem(element).width = width.value;
        }

        private static string GetGameObjectName(UIElement element) {
            return "<" + element.GetType().Name + "> " + element.id;
        }

        public void Register(UIElement instance) {
            RegisterStyleStateChange(instance);
            // todo -- if instance is layout-able
            layoutTree.AddItem(new LayoutData(instance));
        }

    }

}