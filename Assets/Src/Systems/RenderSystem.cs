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

        private Dictionary<int, RawImage> rawImages;
        private Dictionary<int, Text> texts;
        private Dictionary<int, ProceduralImage> proceduralImages;
        private Dictionary<int, Mask> masks;
        private Dictionary<int, RectMask2D> masks2d;
        private Rect[] layoutResults;

        private readonly GameObject gameObject;
        private SkipTree<LayoutData> layoutTree;
        private SkipTree<RenderData> renderSkipTree;

        public Font font; // temp
        private List<GameObject> renderedGameObjects;

        public RenderSystem(GameObject gameObject) {
            this.gameObject = gameObject;
            renderedGameObjects = new List<GameObject>();
            this.renderSkipTree = new SkipTree<RenderData>();
            this.layoutTree = new SkipTree<LayoutData>();
            this.layoutResults = new Rect[128];
        }
        
        public void Update() {
            Rect viewport = gameObject.GetComponent<RectTransform>().rect;//)new Rect(0, 0, 1024, 1024);
            
            // todo -- I'd like to handle this in a better / faster way eventually
            // todo avoid closure here
            layoutTree.TraversePreOrderWithCallback((data => {
                
                data.children.Clear();
                
                if (data.parent == null) return;
                
                data.parent.children.Add(data);

                if (layoutResults.Length < data.parent.children.Count) {
                    Array.Resize(ref layoutResults, layoutResults.Length * 2);
                }

            }));

            // todo -- there needs to be pseudo root in the layout tree in order to handle layout of root level things
            Stack<LayoutDataSet> stack = new Stack<LayoutDataSet>();

            LayoutData[] roots = layoutTree.GetRootItems();
            
            LayoutData pseudoRoot = new LayoutData(null);
            pseudoRoot.layoutDirection = LayoutDirection.Column;
            pseudoRoot.mainAxisAlignment = MainAxisAlignment.Default;
            pseudoRoot.crossAxisAlignment = CrossAxisAlignment.Stretch;
            pseudoRoot.layout = UILayout.Flex;
            
            pseudoRoot.preferredWidth = viewport.width;
            pseudoRoot.preferredWidth = viewport.height;
            pseudoRoot.maxWidth = float.MaxValue;
            pseudoRoot.maxHeight = float.MaxValue;
            pseudoRoot.minWidth = viewport.width;
            pseudoRoot.minHeight = viewport.height;
            
            pseudoRoot.children.AddRange(roots);

            stack.Push(new LayoutDataSet(pseudoRoot, viewport));
//            for (int i = 0; i < roots.Length; i++) {
//                stack.Push(new LayoutDataSet(roots[i], viewport));
//            }

            while (stack.Count > 0) {
                LayoutDataSet layoutSet = stack.Pop();
                LayoutData data = layoutSet.data;
                
                data.layout.Run(viewport, layoutSet, layoutResults);

                if (data.unityTransform != null) {
                    // todo -- when ecs supports RectTransformAccess:
                   // transformRects[next++] = layoutSet.result;
                    data.unityTransform.SetLeftTopPosition(new Vector2(layoutSet.result.x, layoutSet.result.y));
                    data.unityTransform.SetSize(layoutSet.result.width, layoutSet.result.height);
                }
                
                // note: we never need to clear the layoutResults array
                for (int i = 0; i < data.children.Count; i++) {
                    stack.Push(new LayoutDataSet(data.children[i], layoutResults[i]));        
                }
            }
           
        }

        public void Reset() {
            // todo -- destroy game objects in trees

            renderSkipTree.TraversePreOrderWithCallback((data => { data.unityTransform = null; }));
            layoutTree.TraversePreOrderWithCallback((data => { data.unityTransform = null; }));
            
            renderSkipTree = new SkipTree<RenderData>();
            layoutTree = new SkipTree<LayoutData>();

            for (int i = 0; i < renderedGameObjects.Count; i++) {
                Object.Destroy(renderedGameObjects[i]);
            }

            renderedGameObjects.Clear();

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
                unityTransform.SetLeftTopPosition(Vector2.zero);
                renderedGameObjects.Add(obj);
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

        public void SetRectWidth(UIElement element, UIMeasurement width) {
            layoutTree.GetItem(element).preferredWidth = width.value;
        }

        private static string GetGameObjectName(UIElement element) {
            return "<" + element.GetType().Name + "> " + element.id;
        }

        public void Register(UIElement instance) {
            RegisterStyleStateChange(instance);
            // todo -- if instance is layout-able
            // todo -- temporary
            LayoutData data = new LayoutData(instance);
            data.layout = UILayout.Flex;
            data.crossAxisAlignment = CrossAxisAlignment.Stretch;
            data.layoutDirection = LayoutDirection.Row;
            data.unityTransform = renderSkipTree.GetItem(instance).unityTransform;
            layoutTree.AddItem(data);
        }

    }

}