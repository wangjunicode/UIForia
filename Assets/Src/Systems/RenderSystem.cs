using System;
using System.Collections.Generic;
using Src.Rendering;
using UIForia.Extensions;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    [Flags]
    public enum CullResult {

        NotCulled,
        ClipRectIsZero,
        ActualSizeZero,

    }

    public class RenderSystem : IRenderSystem {

        private readonly LightList<UIElement> m_ToInitialize;
        private readonly LightList<RenderData> m_WillRenderList;
        private readonly LightList<RenderData> m_RenderDataList;

        private Camera m_Camera;
        private readonly List<VirtualScrollbar> m_Scrollbars;

        private static readonly RenderZIndexComparerAscending s_ZIndexComparer = new RenderZIndexComparerAscending();

        public event Action<LightList<RenderData>, LightList<RenderData>, Vector3, Camera> DrawDebugOverlay;

        public RenderSystem(Camera camera, ILayoutSystem layoutSystem) {
            this.m_Camera = camera;
            this.m_WillRenderList = new LightList<RenderData>();
            this.m_RenderDataList = new LightList<RenderData>();
            this.m_ToInitialize = new LightList<UIElement>();
            this.m_Scrollbars = new List<VirtualScrollbar>();
            layoutSystem.onCreateVirtualScrollbar += HandleScrollbarCreated;
            layoutSystem.onDestroyVirtualScrollbar += HandleScrollbarDestroyed;
        }

        public void SetCamera(Camera camera) {
            m_Camera = camera;
        }

        private void HandleScrollbarCreated(VirtualScrollbar scrollbar) {
            m_RenderDataList.EnsureAdditionalCapacity(1);
            m_WillRenderList.EnsureAdditionalCapacity(1);
            RenderData data = new RenderData(scrollbar);
            m_RenderDataList.AddUnchecked(data);
            m_Scrollbars.Add(scrollbar);
        }

        private void HandleScrollbarDestroyed(VirtualScrollbar scrollbar) {
        }

        private void InitializeRenderables() {
            if (m_ToInitialize.Count == 0) return;

            m_RenderDataList.EnsureAdditionalCapacity(m_ToInitialize.Count);
            m_WillRenderList.EnsureAdditionalCapacity(m_ToInitialize.Count);

            UIElement[] list = m_ToInitialize.List;

            for (int i = 0; i < m_ToInitialize.Count; i++) {
                UIElement element = list[i];

                if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                    continue;
                }

                m_RenderDataList.AddUnchecked(new RenderData(element));
            }

            m_ToInitialize.Clear();
        }

        public RenderData GetRenderData(UIElement element) {
            return m_RenderDataList.Find(element, (d, el) => d.element == el);
        }

        public void OnUpdate() {
            InitializeRenderables();

            if (m_Camera == null) {
                return;
            }

            m_Camera.orthographic = true;
            m_Camera.orthographicSize = Screen.height * 0.5f;

            Vector3 origin = m_Camera.transform.position;
            origin.x -= 0.5f * Screen.width;
            origin.y += 0.5f * Screen.height;

            m_WillRenderList.Clear();
            m_WillRenderList.EnsureCapacity(m_RenderDataList.Count);

            RenderData[] renderList = m_RenderDataList.List;
            
            // todo -- can be easily jobified
            for (int i = 0; i < m_RenderDataList.Count; i++) {
                RenderData data = renderList[i];
                LayoutResult layoutResult = data.element.layoutResult;
                Rect screenRect = layoutResult.ScreenRect;

                Rect clipRect = layoutResult.clipRect.Intersect(layoutResult.ScreenRect);

                float clipWAdjustment = 0;
                float clipHAdjustment = 0;

                if (clipRect.width <= 0 || clipRect.height <= 0) {
                    data.CullResult = CullResult.ClipRectIsZero;
                    continue;
                }

                if (layoutResult.actualSize.width * layoutResult.actualSize.height <= 0) {
                    data.CullResult = CullResult.ActualSizeZero;
                    continue;
                }

                if (layoutResult.allocatedSize.height < layoutResult.actualSize.height) {
                    clipHAdjustment = 1 - (layoutResult.allocatedSize.height / layoutResult.actualSize.height);
                    if (clipHAdjustment >= 1) {
                        data.CullResult = CullResult.ClipRectIsZero;
                        continue;
                    }
                }

                if (layoutResult.allocatedSize.width < layoutResult.actualSize.width) {
                    clipWAdjustment = 1 - (layoutResult.allocatedSize.width / layoutResult.actualSize.width);
                    if (clipWAdjustment >= 1) {
                        data.CullResult = CullResult.ClipRectIsZero;
                        continue;
                    }
                }

                float clipX = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.x, screenRect.xMin, screenRect.xMax));
                float clipY = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.y, screenRect.yMin, screenRect.yMax));
                float clipW = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.xMax, screenRect.xMin, screenRect.xMax)) - clipWAdjustment;
                float clipH = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.yMax, screenRect.yMin, screenRect.yMax)) - clipHAdjustment;

                if (clipH <= 0 || clipW <= 0) {
                    data.CullResult = CullResult.ClipRectIsZero;
                    continue;
                }

                data.clipVector = new Vector4(clipX, clipY, clipW, clipH);
                data.CullResult = CullResult.NotCulled;

                m_WillRenderList.AddUnchecked(data);
            }

            if (m_WillRenderList.Count == 0) {
                return;
            }

            ComputePositions(m_WillRenderList);

            m_WillRenderList.Sort((a, b) => {
                int idA = a.Renderer.id;
                int idB = b.Renderer.id;
                if (idA == idB) {
                    return 0;
                }

                return idA > idB ? 1 : -1;
            });

            int start = 0;
            RenderData[] willRender = m_WillRenderList.List;
            ElementRenderer renderer = willRender[0].Renderer;
            for (int i = 1; i < m_WillRenderList.Count; i++) {
                RenderData data = willRender[i];
                if (data.Renderer != renderer) {
                    renderer.Render(willRender, start, i, origin, m_Camera);
                    renderer = data.Renderer;
                    start = i;
                }
            }

            renderer.Render(willRender, start, m_WillRenderList.Count, origin, m_Camera);

            DrawDebugOverlay?.Invoke(m_RenderDataList, m_WillRenderList, origin, m_Camera);

            m_WillRenderList.Clear();
        }

        public void OnDestroy() {
            OnReset();
        }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnReset() {
            m_WillRenderList.Clear();
            m_RenderDataList.Clear();
            m_ToInitialize.Clear();
        }

        public void OnElementEnabled(UIElement element) {
            Stack<UIElement> stack = StackPool<UIElement>.Get();
            stack.Push(element);
            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                m_ToInitialize.Add(element);

                for (int i = 0; i < current.children.Length; i++) {
                    stack.Push(current.children[i]);
                }
            }

            StackPool<UIElement>.Release(stack);
        }

        public void OnElementDisabled(UIElement element) {
            Stack<UIElement> stack = StackPool<UIElement>.Get();
            stack.Push(element);
            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                int idx = m_RenderDataList.FindIndex(current, (item, el) => item.element == el);

                if (idx != -1) {
                    RenderData data = m_RenderDataList[idx];
                    data.mesh = null;
                    data.element = null;
                    data.material = null;
                    m_RenderDataList.RemoveAt(idx);
                }
                else {
                    m_ToInitialize.Remove(element);
                }

                if (current.children != null) {
                    for (int i = 0; i < current.children.Length; i++) {
                        stack.Push(current.children[i]);
                    }
                }
            }

            StackPool<UIElement>.Release(stack);
        }

        public void OnElementDestroyed(UIElement element) {
            OnElementDisabled(element);
        }

        public void OnElementCreated(UIElement element) {
            m_ToInitialize.Add(element);
            if (element.children == null) {
                return;
            }

            for (int i = 0; i < element.children.Length; i++) {
                OnElementCreated(element.children[i]);
            }
        }
        
        private static void ComputePositions(LightList<RenderData> renderList) {
            if (renderList.Count == 0) {
                return;
            }

            renderList.Sort((a, b) => {
                int layerA = a.element.layoutResult.layer;
                int layerB = b.element.layoutResult.layer;
                if (layerA == layerB) {
                    return 0;
                }

                return (layerA < layerB) ? 1 : -1;
            });

            int layerStart = 0;
            int currentLayer = renderList[0].element.layoutResult.layer;
            RenderData[] list = renderList.List;
            for (int i = 1; i < renderList.Count; i++) {
                RenderData renderData = list[i];
                int layer = renderData.element.layoutResult.layer;
                if (layer != currentLayer) {
                    renderList.Sort(layerStart, i - layerStart, s_ZIndexComparer);
                    currentLayer = layer;
                    layerStart = i;
                }
            }

            int z = 10;
            for (int i = 0; i < renderList.Count; i++) {
                Vector2 screenPosition = list[i].element.layoutResult.screenPosition;
                list[i].renderPosition = new Vector3(screenPosition.x, -screenPosition.y, z++);
            }
        }

//        private void DrawScrollbars(Vector3 origin) {
//            for (int i = 0; i < m_Scrollbars.Count; i++) {
//                VirtualScrollbar scrollbar = m_Scrollbars[i];
//                Rect trackRect = scrollbar.trackRect;
//                Rect handleRect = scrollbar.handleRect;
//                scrollbar.trackMesh = MeshUtil.ResizeStandardUIMesh(scrollbar.trackMesh, new Size(trackRect.size));
//            }        
//        }
        
        //sort each group by z-index, use depth index to resolve ties, use origin layer if still tied
        private class RenderZIndexComparerAscending : IComparer<RenderData> {

            // at this point we can assume layers are the same 
            public int Compare(RenderData a, RenderData b) {
                // ReSharper disable once PossibleNullReferenceException
                UIElement first = a.element;
                // ReSharper disable once PossibleNullReferenceException
                UIElement second = b.element;
                // if z-indexes with bonus are equal
                if (first.layoutResult.zIndex == second.layoutResult.zIndex) {
                    // if original depths are the same, resolve using depth index
                    if (first.depth == second.depth) {
                        return first.depthIndex > second.depthIndex ? -1 : 1;
                    }

                    // otherwise resolve using raw depth
                    return first.depth > second.depth ? -1 : 1;
                }
                else {
                    return first.layoutResult.zIndex > second.layoutResult.zIndex ? -1 : 1;
                }
            }

        }

    }

}