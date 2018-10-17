using System;
using System.Collections.Generic;
using Rendering;
using Src.Elements;
using Src.Util;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;
using TreeNode = Src.SkipTree<Src.Systems.RenderData>.TreeNode;

namespace Src.Systems {

    public class DirectRenderSystem : IRenderSystem {

        private readonly IStyleSystem m_StyleSystem;
        private readonly ILayoutSystem m_LayoutSystem;
        private readonly List<IDrawable> m_DirtyGraphicList;
        private readonly List<UIElement> m_ToInitialize;
        private readonly SkipTree<RenderData> m_RenderSkipTree;
        private readonly List<RenderData> m_RenderList; //todo -- to array
        private readonly List<UIElement> m_UpdatedElements;
        private bool m_TreeDirty;
        private TreeNode m_SkipTreeRoot;

        public DirectRenderSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
            this.m_LayoutSystem = layoutSystem;
            this.m_RenderSkipTree = new SkipTree<RenderData>();
            this.m_StyleSystem = styleSystem;
            this.m_RenderList = new List<RenderData>();
            this.m_UpdatedElements = new List<UIElement>();
            this.m_DirtyGraphicList = new List<IDrawable>();
//            this.m_LayoutSystem.onCreateVirtualScrollbar += OnVirtualScrollbarCreated;
            this.m_ToInitialize = new List<UIElement>();
            this.m_TreeDirty = true;
        }

        private void InitializeRenderables() {
            for (int i = 0; i < m_ToInitialize.Count; i++) {
                UIElement element = m_ToInitialize[i];

                if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                    continue;
                }

                RenderData renderData = new RenderData(element, this);
                m_RenderList.Add(renderData);
                m_RenderSkipTree.AddItem(renderData);

                if ((element is VirtualElement)) {
//                    transform.SetSiblingIndex(int.MaxValue);
                }
                else {
                    if (element.style.HandlesOverflowX || element.style.HandlesOverflowY) {
                        renderData.clips = true;
                    }
                }

                if (element.isEnabled) {
                    m_DirtyGraphicList.Add(renderData.drawable);
                    renderData.drawable.onMeshDirty += MarkGeometryDirty;
                    renderData.drawable.onMaterialDirty += MarkMaterialDirty;
                }
            }

            m_ToInitialize.Clear();
            m_TreeDirty = true;
        }

        public void MarkMaterialDirty(IDrawable element) {
            if (!m_DirtyGraphicList.Contains(element)) {
                m_DirtyGraphicList.Add(element);
            }
        }

        public void MarkGeometryDirty(IDrawable element) {
            if (!m_DirtyGraphicList.Contains(element)) {
                m_DirtyGraphicList.Add(element);
            }
        }

        public void OnReset() { }

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

//        Dictionary<int, DrawInfo> m_DrawInfoMap
        public void OnUpdate() {
            InitializeRenderables();

            m_UpdatedElements.Clear();
            m_LayoutSystem.GetUpdatedLayoutElements(m_UpdatedElements);

            for (int i = 0; i < m_UpdatedElements.Count; i++) {
                UIElement element = m_UpdatedElements[i];
                DrawInfo drawInfo; // = m_DrawInfoMap.GetOrDefault(element.id);

                RenderData renderData = m_RenderSkipTree.GetItem(element.id);

                if (renderData == null) {
                    continue;
                }
            }

            if (m_UpdatedElements.Count > 0) {
                UpdateLayersAndZIndex();
                UpdateClipAndCulling();
            }


            MaterialPropertyBlock block;
            Material mat = Resources.Load<Material>("Materials/UIForia");
            mat.color = Color.white;
            mat.SetVector("_ClipRect", new Vector4(-500, -500, 1000, 1000));

            Camera.main.orthographic = true;
            Camera.main.orthographicSize = Screen.height * 0.5f;

            Vector3 origin = Camera.main.transform.position;
            origin.x -= 0.5f * Screen.width;
            origin.y += 0.5f * Screen.height;
            origin.z = 10f;

            m_RenderList.Sort(new RenderDepthComparerAscending());

            float z = -m_RenderList.Count;

            for (int i = 0; i < m_RenderList.Count; i++) {
                RenderData data = m_RenderList[i];
                Vector3 position = data.element.layoutResult.screenPosition;
                position.z = z++;
                position.y = -position.y;
                mat.SetVector("_ClipRect", new Vector4(-500, -500, 1000, 1000));
                Graphics.DrawMesh(data.drawable.GetMesh(), origin + position, Quaternion.identity, mat, 0, Camera.main, 0, null, false, false, false);
            }

            // clipping updates when
            // moved
            // resize
            // scaled?
            // rotated
            // enabled 
            // disabled
            // scrolled
            // overflow x/y changed
        }


        private void UpdateClipAndCulling() {
            if (m_TreeDirty) {
                m_RenderSkipTree.RecycleTree(m_SkipTreeRoot);
                m_SkipTreeRoot = m_RenderSkipTree.GetTraversableTree();
            }

            m_RenderList.Clear();

            Stack<TreeNode> stack = StackPool<TreeNode>.Get();
            Stack<Rect> clipRects = StackPool<Rect>.Get();
            stack.Push(m_SkipTreeRoot);

            clipRects.Push(new Rect(0, 0, Screen.width, Screen.height));

            // todo -- to handle layers we need a different tree (ClipTree)
            // in that tree items are parented based on what should be clipping them
            while (stack.Count > 0) {
                TreeNode current = stack.Pop();
                RenderData renderData = current.item;
                UIElement element = renderData.element;

                Rect screenRect = element.layoutResult.ScreenRect;

                Rect currentClipRect = RectIntersect(screenRect, clipRects.Peek());

                current.item.clipRect = currentClipRect;

                if (currentClipRect.width > 0 && currentClipRect.height > 0) {
                    m_RenderList.Add(current.item);
                }

                if (element.style.HandlesOverflow) {
                    clipRects.Push(currentClipRect);

                    for (int i = 0; i < current.children.Length; i++) {
                        stack.Push(current.children[i]);
                    }

                    clipRects.Pop();
                }
                else {
                    for (int i = 0; i < current.children.Length; i++) {
                        stack.Push(current.children[i]);
                    }
                }
            }

            StackPool<TreeNode>.Release(stack);
            StackPool<Rect>.Release(clipRects);
        }

        private void UpdateLayersAndZIndex() {
            DrawInfo[] drawInfos = new DrawInfo[m_RenderList.Count];

            for (int i = 0; i < m_RenderList.Count; i++) {
                RenderData data = m_RenderList[i];
                DrawInfo info = new DrawInfo();
                info.elementId = data.element.id;
                info.depth = data.element.depth;
                info.layerBase = ResolveRenderLayer(data.element);
                info.layerOffset = data.element.ComputedStyle.LayerOffset;
                info.zIndex = data.element.ComputedStyle.ZIndex;
                info.computedLayer = info.layerBase + info.layerOffset;

                //wrong?
                info.parentLayer = data.element.parent.depth;

                if (info.depth > info.computedLayer) {
                    info.zIndexBonus = 1000;
                }
                else if (info.depth < info.computedLayer) {
                    info.zIndexBonus = -1000;
                }

                drawInfos[i] = info;
            }

            Array.Sort(drawInfos, (a, b) => (a.computedLayer > b.computedLayer) ? 1 : 0);

            int currentLayer = drawInfos[0].computedLayer;

            List<List<DrawInfo>> layerBuckets = new List<List<DrawInfo>>();

            int layerStart = 0;
            for (int i = 0; i < drawInfos.Length; i++) {
                if (drawInfos[i].computedLayer != currentLayer) {
                    currentLayer = drawInfos[i].computedLayer;
                    Array.Sort(drawInfos, layerStart, i, (a, b) => 1);
                    layerStart = i;
                }
            }
            
        }

        private int ResolveRenderLayer(UIElement element) {
            RenderLayer layer = element.style.computedStyle.RenderLayer;
            switch (layer) {
                case RenderLayer.Unset:
                case RenderLayer.Default:
                    return element.depth;

                case RenderLayer.Parent:
                    return element.depth - 1;

                case RenderLayer.Template:
                    return element.templateParent.depth;

                case RenderLayer.Modal:
                case RenderLayer.View:
                case RenderLayer.Screen:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public struct DrawInfo {

            public int zIndexBonus;
            public int elementId;
            public int parentLayer;
            public int depth;
            public int meshId;
            public int materialId;
            public Rect clipRect;
            public bool clips;
            public int parentInfoId;
            public int layerBase;
            public int layerOffset;
            public int zIndex;
            public float rotation;
            public Vector2 scale;
            public Vector3 position;
            public int computedLayer;

        }

        public class RenderDepthComparerAscending : IComparer<RenderData> {

            public int Compare(RenderData first, RenderData second) {
                UIElement a = first.element;
                UIElement b = second.element;

                if (a.depth != b.depth) {
                    return a.depth < b.depth ? 1 : -1;
                }

                if (a.parent == b.parent) {
                    return a.siblingIndex < b.siblingIndex ? 1 : -1;
                }

                if (a.parent == null) return 1;
                if (b.parent == null) return -1;

                return a.parent.siblingIndex < b.parent.siblingIndex ? 1 : -1;
            }

        }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() {
            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        private void HandleStylePropertyChanged(UIElement element, StyleProperty property) {
            var renderData = m_RenderSkipTree.GetItem(element.id);
            if (renderData?.drawable == null) {
                return;
            }

            // todo figure out pivots, unity re-builds meshes. figure out how rotation affects position
            if (property.propertyId == StylePropertyId.TransformPivotX || property.propertyId == StylePropertyId.TransformPivotY) {
                renderData.drawable.OnAllocatedSizeChanged();
            }

            renderData.drawable.OnStylePropertyChanged(property);

            switch (property.propertyId) {
                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPivotY:
                case StylePropertyId.TransformRotation:
                    break;
                case StylePropertyId.ZIndex:
                case StylePropertyId.RenderLayer:
                case StylePropertyId.LayerOffset:
                    break;
            }
        }

        public void OnElementCreated(UIElement element) {
            m_ToInitialize.Add(element);
        }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnElementCreatedFromTemplate(MetaData creationData) {
            m_ToInitialize.Add(creationData.element);
            for (int i = 0; i < creationData.children.Count; i++) {
                OnElementCreatedFromTemplate(creationData.children[i]);
            }
        }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) { }

        public void OnRender() { }

    }

}

//
//Render() {
//
//sort by resolved layer
//    
//for each layer
//find z-index bonus:
//origin layer > current layer -> zindex bonus = 1000
//origin layer == current layer = zindex bonus = 0
//orign layer < current layer = zindex bonus = -1000
//split into 3 groups:
//positive z-index
//0 z-index
//    negative z-index
//
//    sort each group by z-index, use depthindex to resolve ties, use origin layer if still tied
//
//float z = layerBase
//    for each item
//    item.z = z;
//    z += 1f
//
//culling ->
//    for each item
//    find all parents w/ clipping where parent layer > own layer
//    clip rect = combined cliprect of parents
//    culled = no intersection
//
//    are children not in the parent layer + 1 still part of layout?
//    unless I explicitly set layout behavior, I expect elements in normal layout position regardless of layer
//
//LayerClipBehavior = 
//ClipNever
//    ClipByParent -> regardless of layer
//    ClipByLayer
//    ClipByView
//    ClipByScreen
//    ClipByTemplateRoot
//
//}