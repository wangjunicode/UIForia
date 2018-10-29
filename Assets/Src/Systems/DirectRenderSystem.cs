using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Elements;
using Src.Util;
using UnityEngine;
using BucketList = System.Collections.Generic.List<System.Collections.Generic.List<Src.Systems.RenderData>>;

namespace Src.Systems {

    public class DirectRenderSystem : IRenderSystem {

        private readonly IStyleSystem m_StyleSystem;

        private readonly LightList<UIElement> m_ToInitialize;

        private readonly LightList<RenderData> m_WillRenderList;
        private readonly LightList<RenderData> m_RenderDataList;

        private readonly Camera m_Camera;
        private readonly List<VirtualScrollbar> m_Scrollbars;

        private static readonly RenderZIndexComparerAscending s_ZIndexComparer = new RenderZIndexComparerAscending();

        public DirectRenderSystem(Camera camera, ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
            this.m_Camera = camera;
            this.m_StyleSystem = styleSystem;
            this.m_WillRenderList = new LightList<RenderData>();
            this.m_RenderDataList = new LightList<RenderData>();
            this.m_ToInitialize = new LightList<UIElement>();
            this.m_Scrollbars = new List<VirtualScrollbar>();
            layoutSystem.onCreateVirtualScrollbar += HandleScrollbarCreated;
            layoutSystem.onDestroyVirtualScrollbar += HandleScrollbarDestroyed;
        }

        private void HandleScrollbarCreated(VirtualScrollbar scrollbar) {
            m_ToInitialize.Add(scrollbar);
            m_ToInitialize.Add(scrollbar.handle);
            m_Scrollbars.Add(scrollbar);
        }

        private void HandleScrollbarDestroyed(VirtualScrollbar scrollbar) {
            m_ToInitialize.Remove(scrollbar);
            m_ToInitialize.Remove(scrollbar.handle);
            m_Scrollbars.Remove(scrollbar);
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


        public void OnUpdate() {
            /*
             *  for meshes of type IDrawableInstanced -> use draw instanced and pass in material block
             */

            InitializeRenderables();

            if (m_Camera == null) {
                return;
            }

            m_Camera.orthographic = true;
            m_Camera.orthographicSize = Screen.height * 0.5f;

            Vector3 origin = m_Camera.transform.position;
            origin.x -= 0.5f * Screen.width;
            origin.y += 0.5f * Screen.height;
            origin.z = 10f;


            m_WillRenderList.Clear();
            m_WillRenderList.EnsureCapacity(m_RenderDataList.Count);

            RenderData[] renderList = m_RenderDataList.List;

            for (int i = 0; i < m_RenderDataList.Count; i++) {
                RenderData data = renderList[i];
                LayoutResult layoutResult = data.element.layoutResult;
                Rect screenRect = layoutResult.ScreenRect;

                Rect clipRect = RectIntersect(layoutResult.clipRect, layoutResult.ScreenRect);

                float clipWAdjustment = 0;
                float clipHAdjustment = 0;

                if (clipRect.width <= 0 || clipRect.height <= 0) {
                    continue;
                }

                if (layoutResult.actualSize.width * layoutResult.actualSize.height <= 0) {
                    continue;
                }

                if (layoutResult.allocatedSize.height < layoutResult.actualSize.height) {
                    clipHAdjustment = 1 - (layoutResult.allocatedSize.height / layoutResult.actualSize.height);
                    if (clipHAdjustment >= 1) {
                        continue;
                    }
                }

                if (layoutResult.allocatedSize.width < layoutResult.actualSize.width) {
                    clipWAdjustment = 1 - (layoutResult.allocatedSize.width / layoutResult.actualSize.width);
                    if (clipWAdjustment >= 1) {
                        continue;
                    }
                }

                float clipX = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.x, screenRect.xMin, screenRect.xMax));
                float clipY = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.y, screenRect.yMin, screenRect.yMax));
                float clipW = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.xMax, screenRect.xMin, screenRect.xMax)) - clipWAdjustment;
                float clipH = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.yMax, screenRect.yMin, screenRect.yMax)) - clipHAdjustment;

                if (clipH <= 0 || clipW <= 0) {
                    continue;
                }

                data.clipVector = new Vector4(clipX, clipY, clipW, clipH);
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
            m_WillRenderList.Clear();
        }

        private static Rect RectIntersect(Rect a, Rect b) {
            float xMin = a.x > b.x ? a.x : b.x;
            float xMax = a.x + a.width < b.x + b.width ? a.x + a.width : b.x + b.width;
            float yMin = a.y > b.y ? a.y : b.y;
            float yMax = a.y + a.height < b.y + b.height ? a.y + a.height : b.y + b.height;
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public List<RenderData> GetRenderList() {
            return null;
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

            int z = renderList.Count;
            for (int i = 0; i < renderList.Count; i++) {
                Vector2 screenPosition = list[i].element.layoutResult.screenPosition;
                list[i].renderPosition = new Vector3(screenPosition.x, -screenPosition.y, z--);
            }
        }

        //sort each group by z-index, use depth index to resolve ties, use origin layer if still tied
        public class RenderZIndexComparerAscending : IComparer<RenderData> {

            // at this point we can assume layers are the same 
            public int Compare(RenderData a, RenderData b) {
                UIElement first = a.element;
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

        public void OnDestroy() {
            OnReset();
        }

        public void OnReady() { }

        public void OnInitialize() {
            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        private void HandleStylePropertyChanged(UIElement element, StyleProperty property) { }

        public void OnReset() {
            m_WillRenderList.Clear();
            m_RenderDataList.Clear();
            m_ToInitialize.Clear();
            m_StyleSystem.onStylePropertyChanged -= HandleStylePropertyChanged;
        }

        public void OnElementEnabled(UIElement element) {
            Stack<UIElement> stack = StackPool<UIElement>.Get();
            stack.Push(element);
            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                m_ToInitialize.Add(element);

                for (int i = 0; i < current.ownChildren.Length; i++) {
                    stack.Push(current.ownChildren[i]);
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

                if (current.ownChildren != null) {
                    for (int i = 0; i < current.ownChildren.Length; i++) {
                        stack.Push(current.ownChildren[i]);
                    }
                }
            }

            StackPool<UIElement>.Release(stack);
        }

        public void OnElementDestroyed(UIElement element) {
            OnElementDisabled(element);
        }

        public void OnElementCreatedFromTemplate(MetaData creationData) {
            m_ToInitialize.Add(creationData.element);
            for (int i = 0; i < creationData.children.Count; i++) {
                OnElementCreatedFromTemplate(creationData.children[i]);
            }
        }

    }

}