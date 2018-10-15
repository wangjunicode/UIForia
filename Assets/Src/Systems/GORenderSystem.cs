using System.Collections.Generic;
using Rendering;
using Src.Elements;
using Src.Extensions;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class GORenderSystem : IRenderSystem, IGraphicUpdateManager {

        private readonly RectTransform rectTransform;

        private readonly ILayoutSystem m_LayoutSystem;
        private readonly IStyleSystem styleSystem;

        private readonly SkipTree<RenderData> m_RenderSkipTree;
        private readonly Dictionary<int, RectTransform> m_TransformMap;

        private readonly List<IDrawable> m_DirtyGraphicList;
        private readonly Dictionary<int, CanvasRenderer> m_CanvasRendererMap;
        private readonly List<RenderData> m_VirtualScrollbarElements;
        private readonly List<UIElement> m_ToInitialize;
        private readonly SkipTree<RenderData> m_ClipTree;
        private readonly Canvas m_Canvas;
        
        private static readonly List<RenderData> s_ClipChain = new List<RenderData>();

        public GORenderSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem, RectTransform rectTransform) {
            this.m_LayoutSystem = layoutSystem;
            this.rectTransform = rectTransform;
            this.m_RenderSkipTree = new SkipTree<RenderData>();

            this.m_DirtyGraphicList = new List<IDrawable>();
            this.m_CanvasRendererMap = new Dictionary<int, CanvasRenderer>();
            this.m_TransformMap = new Dictionary<int, RectTransform>();
            this.m_VirtualScrollbarElements = new List<RenderData>();
            this.m_LayoutSystem.onCreateVirtualScrollbar += OnVirtualScrollbarCreated;
            this.m_ToInitialize = new List<UIElement>();
            this.m_ClipTree = new SkipTree<RenderData>();
            this.m_Canvas = rectTransform.GetComponentInParent<Canvas>();
            
            this.m_RenderSkipTree.onItemParentChanged += (item, newParent, oldParent) => {
                RectTransform transform = m_TransformMap.GetOrDefault(item.UniqueId);
                if (newParent == null) {
                    transform.SetParent(rectTransform);
                }
                else {
                    RectTransform parentTransform = m_TransformMap.GetOrDefault(newParent.UniqueId);
                    transform.SetParent(parentTransform == null ? rectTransform : parentTransform);
                }

                transform.anchorMin = new Vector2(0, 1);
                transform.anchorMax = new Vector2(0, 1);
                transform.pivot = new Vector2(0, 1);
                transform.anchoredPosition = Vector3.zero;
                transform.SetSiblingIndex(FindRenderedSiblingIndex(item.element));
            };

            this.styleSystem = styleSystem;
        }

        private static int FindRenderedSiblingIndex(UIElement element) {
            // if parent is not rendered
            // we want to replace
            // so find parent's sibling index
            // spin through rendered children until finding target
            // use parent index + child index
            if (element.parent == null) return 0;

            int idx = 0;
            for (int i = 0; i < element.parent.ownChildren.Length; i++) {
                UIElement sibling = element.parent.ownChildren[i];
                if (sibling == element) {
                    break;
                }

                if ((sibling.flags & UIElementFlags.RequiresRendering) != 0) {
                    idx++;
                }
            }

            if ((element.parent.flags & UIElementFlags.RequiresRendering) == 0) {
                idx += FindRenderedSiblingIndex(element.parent);
            }

            return idx;
        }

        public void OnReady() { }

        public void OnInitialize() {
            this.styleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        public void OnElementCreated(UIElement element) {
            m_ToInitialize.Add(element);
        }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

        public void OnElementCreatedFromTemplate(MetaData creationData) {
            m_ToInitialize.Add(creationData.element);
            for (int i = 0; i < creationData.children.Count; i++) {
                OnElementCreatedFromTemplate(creationData.children[i]);
            }
        }

        private void HandleStylePropertyChanged(UIElement element, StyleProperty property) {
            m_RenderSkipTree.GetItem(element.id)?.drawable?.OnStylePropertyChanged(property);
        }

        private void InitializeRenderables() {
            for (int i = 0; i < m_ToInitialize.Count; i++) {
                UIElement element = m_ToInitialize[i];
                if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                    continue;
                }

                GameObject go = new GameObject(element.ToString());
                RectTransform transform = go.AddComponent<RectTransform>();
                CanvasRenderer canvasRenderer = go.AddComponent<CanvasRenderer>();
                StyleDebugView debugView = go.AddComponent<StyleDebugView>();
                debugView.element = element;
                transform.anchorMin = new Vector2(0, 1);
                transform.anchorMax = new Vector2(0, 1);
                transform.pivot = new Vector2(0, 1);

                m_TransformMap.Add(element.id, transform);
                m_CanvasRendererMap.Add(element.id, canvasRenderer);

                RenderData renderData = new RenderData(element, this);
                renderData.canvasRenderer = canvasRenderer;

                if (element.style.HandlesOverflowX || element.style.HandlesOverflowY) {
                    renderData.clips = true;
                    m_ClipTree.AddItem(renderData);
                }

                m_RenderSkipTree.AddItem(renderData);
                if (element.isEnabled) {
                    m_DirtyGraphicList.Add(renderData.drawable);
                }
            }

            m_ToInitialize.Clear();
        }

        public void OnUpdate() {
            InitializeRenderables();

            List<UIElement> updatedElements = m_LayoutSystem.GetUpdatedLayoutElements(ListPool<UIElement>.Get());
            List<RenderData> clipList = ListPool<RenderData>.Get();
            int clipUpdateStartDepth = -1;

            if (updatedElements.Count > 0) {
                clipUpdateStartDepth = updatedElements[0].depth;
            }

            for (int i = 0; i < updatedElements.Count; i++) {
                RectTransform transform;
                UIElement element = updatedElements[i];
                LayoutResult layoutResult = updatedElements[i].layoutResult;

                if (!m_TransformMap.TryGetValue(element.id, out transform)) {
                    continue;
                }

                RenderData renderData = m_RenderSkipTree.GetItem(element.id);

                if (element.depth == clipUpdateStartDepth) {
                    clipList.Add(renderData);
                }

                Vector2 position = layoutResult.localPosition;
                Vector2 size = new Vector2(layoutResult.allocatedWidth, layoutResult.allocatedHeight);

                float rotation = element.style.computedStyle.TransformRotation * Mathf.Deg2Rad;

                UIElement ptr = element.parent;

                // while parent is not rendered, localPosition += nonRenderedParent.localPosition
                while (ptr != null) {
                    RectTransform ancestorTransform;

                    if (m_TransformMap.TryGetValue(ptr.id, out ancestorTransform)) {
                        break;
                    }

                    position += ptr.layoutResult.localPosition;

                    ptr = ptr.parent;
                }

                Vector3 outputPosition = new Vector3(position.x, Screen.height - position.y);
                Quaternion outputRotation;

                if (Mathf.Approximately(rotation, 0)) {
                    outputRotation = Quaternion.identity;
                }
                else {
                    outputRotation = Quaternion.AngleAxis(rotation, Vector3.forward);
                }

                transform.anchoredPosition = new Vector3(outputPosition.x, -position.y);
                transform.rotation = outputRotation;

                if (transform.sizeDelta != size) {
                    transform.sizeDelta = size;

                    renderData.drawable?.OnAllocatedSizeChanged();
                }

                Rect overflowRect = element.layoutResult.ScreenOverflowRect;
                Rect screenRect = element.layoutResult.ScreenRect;

                // if element is overflowing we need to update it's clip rect if parent handles clipping

                // culling
                // clipping self
                // masking children

                // update clipping when:
                    // - size changes
                    // - position changes
                    // - scroll bar added / size change / attachment change
                    // - clipped ancestor moves
                    // - clipped ancestor size changes
                    // - clipped ancestor scroll bar add / remove / size change
                
                if (element.style.HandlesOverflow && (overflowRect.width >= 0 || overflowRect.height >= 0)) {
                    // if overflowing & ancestor handles overflow, we need to mask and maybe cull
                    // also need to apply mask changes to children

                    Rect localRect = element.layoutResult.LocalRect;

                    Vector2 canvasSize = ((RectTransform) m_Canvas.transform).sizeDelta * 0.5f;

                    // clip rects are relative to CANVAS CENTER!
                    Rect clipRect = new Rect(
                        screenRect.x - canvasSize.x,
                        screenRect.y + canvasSize.y - localRect.height,
                        localRect.width,
                        localRect.height
                    );

                    renderData.canvasRenderer.EnableRectClipping(clipRect);

                    SkipTree<RenderData>.TreeNode node = m_RenderSkipTree.GetTraversableTree(renderData);

                    for (int j = 0; j < node.children.Length; j++) {
                        SkipTree<RenderData>.TreeNode child = node.children[j];
                        LayoutResult r = child.item.element.layoutResult;
                        Rect nodeClipRect = new Rect(
                            r.ScreenRect.x - canvasSize.x,
                            r.ScreenRect.y + canvasSize.y - r.allocatedHeight,
                            r.allocatedWidth,
                            r.allocatedHeight
                        );
                        Rect intersect = RectIntersect(clipRect, nodeClipRect);
                        child.item.canvasRenderer.EnableRectClipping(intersect);
                    }

//                    if (element.style.HandlesOverflowX) {
//                        Debug.Log("Should clip X " + element);
//                        float x = overflowRect.x - screenRect.x;
//                        float width = overflowRect.width - screenRect.width;
//                    }
//
//                    if (element.style.HandlesOverflowY) {
//                        Debug.Log("Should clip Y " + element);
//
//                    }
                    
                }
            }

            // run through first level of changes
            // apply clipping / culling updates to children

//            Stack<SkipTree<RenderData>.TreeNode> cullClipStack = StackPool<SkipTree<RenderData>.TreeNode>.Get();
//            for (int i = 0; i < clipList.Count; i++) {
//                RenderData renderdata = clipList[i];
//
//                m_ClipTree.GetAncestors(renderdata, s_ClipChain);
//                Rect clipRect = s_ClipChain[0].clipRect;
//
//                if (s_ClipChain.Count == 0 && !renderdata.clips) {
//                    return;
//                }
//
//                if (s_ClipChain.Count > 0) {
//                    
//                    for (int j = 0; j < s_ClipChain.Count; j++) {
//                        clipRect = RectIntersect(clipRect, s_ClipChain[j].clipRect);
//                    }
//                    
//                }
//                
//                SkipTree<RenderData>.TreeNode node = m_RenderSkipTree.GetTraversableTree(renderdata);
//
//                while (cullClipStack.Count > 0) {
//                    node = cullClipStack.Pop();
//
//                   // clipRect.Intersect();
//
//                    for (int j = 0; j < node.children.Length; j++) {
//                        cullClipStack.Push(node.children[j]);
//                    }
//                    
//                }
//
//                s_ClipChain.Clear();
//            }
//
//            StackPool<SkipTree<RenderData>.TreeNode>.Release(cullClipStack);

            for (int i = 0; i < m_VirtualScrollbarElements.Count; i++) {
                RenderData data = m_VirtualScrollbarElements[i];

                if (data.horizontalScrollbar != null) {
                    RenderScrollbar(data.horizontalScrollbar, data.horizontalScrollbarHandle);
                }

                if (data.verticalScrollbar != null) {
                    RenderScrollbar(data.verticalScrollbar, data.verticalScrollbarHandle);
                }
            }

            for (int i = 0; i < m_DirtyGraphicList.Count; i++) {
                IDrawable graphic = m_DirtyGraphicList[i];
                CanvasRenderer canvasRenderer = m_CanvasRendererMap[graphic.Id];
                Mesh mesh = graphic.GetMesh();
                Material material = graphic.GetMaterial();
                Texture text = graphic.GetMainTexture();

                canvasRenderer.materialCount = 1;
                canvasRenderer.SetMesh(mesh);
                canvasRenderer.SetMaterial(material, 0);
                canvasRenderer.SetTexture(text);
            }

            m_DirtyGraphicList.Clear();

            ListPool<UIElement>.Release(ref updatedElements);
        }


        private static Rect RectIntersect(Rect a, Rect b) {
            float xMin = a.x > b.x ? a.x : b.x;
            float xMax = a.x + a.width < b.x + b.width ? a.x + a.width : b.x + b.width;
            float yMin = a.y > b.y ? a.y : b.y;
            float yMax = a.y + a.height < b.y + b.height ? a.y + a.height : b.y + b.height;

            if (xMax >= xMin && yMax >= yMin) {
                return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            }

            return new Rect(0f, 0f, 0f, 0f);
        }

        public void OnReset() {
            foreach (KeyValuePair<int, RectTransform> kvp in m_TransformMap) {
                Object.Destroy(kvp.Value.gameObject);
            }

            this.styleSystem.onStylePropertyChanged -= HandleStylePropertyChanged;

            m_RenderSkipTree.Clear();
            m_TransformMap.Clear();
            m_CanvasRendererMap.Clear();
            m_DirtyGraphicList.Clear();
        }

        public void OnDestroy() {
            OnReset();
        }

        private void RenderScrollbar(VirtualScrollbar scrollbar, RectTransform handle) {
            RectTransform transform = m_TransformMap[scrollbar.id];
            Rect trackRect = scrollbar.GetTrackRect();
            Vector2 targetPosition = new Vector2(trackRect.x, trackRect.y);

            targetPosition.y = -targetPosition.y;
            transform.anchoredPosition = targetPosition;
            transform.sizeDelta = new Vector2(trackRect.width, trackRect.height);

            Rect handleRect = scrollbar.HandleRect;
            Vector2 handlePosition = scrollbar.handlePosition;
            handlePosition.y = -handlePosition.y;
            handle.anchoredPosition = handlePosition;
            handle.sizeDelta = new Vector2(handleRect.width, handleRect.height);
        }

        public void OnElementDestroyed(UIElement element) {
            RenderData data = m_RenderSkipTree.GetItem(element);

            if (data == null) return;

            m_RenderSkipTree.RemoveItem(data);

            CanvasRenderer canvasRenderer = m_CanvasRendererMap.GetOrDefault(element.id);
            if (canvasRenderer != null) {
                Object.Destroy(canvasRenderer);
            }

            RectTransform transform = m_TransformMap.GetOrDefault(element.id);
            if (transform != null) {
                m_TransformMap.Remove(element.id);
                Object.Destroy(transform.gameObject);
            }

            // todo -- release to pool
            data.element = null;
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) {
            RenderData data = m_RenderSkipTree.GetItem(element);
            if (data != null) {
                m_RenderSkipTree.UpdateItemParent(element);
            }
            else {
                //OnElementStyleChanged(element);
            }
        }

        public void OnRender() {
            OnUpdate();
        }

        public void OnElementEnabled(UIElement element) {
            if (m_TransformMap.ContainsKey(element.id)) {
                m_TransformMap[element.id].gameObject.SetActive(true);
            }

            RenderData renderData = m_RenderSkipTree.GetItem(element.id);
            if (renderData != null) {
                m_DirtyGraphicList.Add(renderData.drawable);
            }
        }

        public void OnElementDisabled(UIElement element) {
            if (m_TransformMap.ContainsKey(element.id)) {
                m_TransformMap[element.id].gameObject.SetActive(false);
            }

            if (m_CanvasRendererMap.ContainsKey(element.id)) {
                m_CanvasRendererMap[element.id].Clear();
            }

            RenderData renderData = m_RenderSkipTree.GetItem(element.id);
            if (renderData != null) {
                m_DirtyGraphicList.Remove(renderData.drawable);
            }
        }

        public void MarkGeometryDirty(IDrawable element) {
            if (!m_DirtyGraphicList.Contains(element)) {
                m_DirtyGraphicList.Add(element);
            }
        }

        public void MarkMaterialDirty(IDrawable element) {
            if (!m_DirtyGraphicList.Contains(element)) {
                m_DirtyGraphicList.Add(element);
            }
        }

        public void OnVirtualScrollbarCreated(VirtualScrollbar scrollbar) {
            RenderData renderData = m_RenderSkipTree.GetItem(scrollbar.targetElement);

            if (renderData != null) {
                if (renderData.element.style.HandlesOverflow) {
                    renderData.clips = true;
                    m_ClipTree.AddItem(renderData);
                    // update clipping here?
                }
            }
        }
//                if (renderData.mask == null) {
//                    renderData.mask = renderData.unityTransform.gameObject.AddComponent<RectMask2D>();
//                    m_VirtualScrollbarElements.Add(renderData);
//                }

//                if (scrollbar.orientation == ScrollbarOrientation.Horizontal) {
//                    renderData.horizontalScrollbar = scrollbar;
//                    RectTransform transform = CreateGameObject("Scrollbar H");
//                    transform.SetParent(m_TransformMap[scrollbar.targetElement.parent.id]);
//                    RawImage img = transform.gameObject.AddComponent<RawImage>();
//                    img.color = Color.cyan;
//                    m_TransformMap.Add(scrollbar.id, transform);
//                    RectTransform handleTransform = CreateGameObject("Scrollbar H - Handle");
//                    handleTransform.SetParent(transform);
//                    img = handleTransform.gameObject.AddComponent<RawImage>();
//                    img.color = Color.blue;
//                    renderData.horizontalScrollbarHandle = handleTransform;
//                }
//                else if (scrollbar.orientation == ScrollbarOrientation.Vertical) {
//                    renderData.verticalScrollbar = scrollbar;
//                    RectTransform transform = CreateGameObject("Scrollbar V");
//                    transform.SetParent(m_TransformMap[scrollbar.targetElement.parent.id]);
//                    RawImage img = transform.gameObject.AddComponent<RawImage>();
//                    img.color = Color.cyan;
//                    m_TransformMap.Add(scrollbar.id, transform);
//                    RectTransform handleTransform = CreateGameObject("Scrollbar V - Handle");
//                    handleTransform.SetParent(transform);
//                    img = handleTransform.gameObject.AddComponent<RawImage>();
//                    img.color = Color.blue;
//                    renderData.verticalScrollbarHandle = handleTransform;
//                }
//            }

    }

}

//        private void OnElementStyleChanged(UIElement element) {
//            RenderData data = renderSkipTree.GetItem(element);
//
//            RenderPrimitiveType primitiveType = DeterminePrimitiveType(element);
//
//            if (data == null) {
//                GameObject obj = new GameObject(element.ToString());
//
//#if DEBUG
//                StyleDebugView debugView = obj.AddComponent<StyleDebugView>();
//                debugView.element = element;
//#endif
//
//                RectTransform unityTransform = obj.AddComponent<RectTransform>();
//                unityTransform.anchorMin = new Vector2(0, 1);
//                unityTransform.anchorMax = new Vector2(0, 1);
//                unityTransform.pivot = new Vector2(0, 1);
//                m_TransformMap[element.id] = unityTransform;
//
//                obj.SetActive(element.isEnabled);
//                data = new RenderData(element, primitiveType, unityTransform);
//                CreateComponents(data);
//                renderSkipTree.AddItem(data);
//
//                return;
//            }
//
//            if (primitiveType == data.primitiveType) {
//                ApplyStyles(data);
//                return;
//            }
//
//            data.primitiveType = primitiveType;
//
//            if (data.renderComponent != null) {
//                Object.Destroy(data.renderComponent);
//            }
//
//            // todo -- maybe remove & re-parent children if not rendering
//            if (primitiveType == RenderPrimitiveType.None) {
//                return;
//            }
//
//            data.primitiveType = primitiveType;
//            CreateComponents(data);
//        }

//                // Text elements give me lots of trouble. Here is what needs to happen:
//                // Layout needs to measure the preferred size of the string. It does this on it's own
//                // once that is computed the layout system uses it as normal. When it comes to rendering
//                // we only want to set the text if we have to and then force the mesh to update because
//                // this needs to happen BEFORE we update dirty graphics because things like highlighting
//                // and caret placement need to have up to date data on rendered character layout which 
//                // may differ from what the layout system says. 
//
//                UITextElement textElement = element as UITextElement;
//                if (textElement != null) {
//                    TextMeshProUGUI tmp = renderData.renderComponent as TextMeshProUGUI;
//                    if (tmp != null) {
//                        TextMeshProUGUI textMesh = renderData.renderComponent as TextMeshProUGUI;
//                        if (textMesh != null && textMesh.text != textElement.GetText()) {
//                            textMesh.text = textElement.GetText();
//                            textMesh.ForceMeshUpdate();
//                        }
//                    }
//                }