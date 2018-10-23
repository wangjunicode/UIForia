using System.Collections.Generic;
using Rendering;
using UnityEngine;
using BucketList = System.Collections.Generic.List<System.Collections.Generic.List<Src.Systems.RenderData>>;

namespace Src.Systems {

    public class DirectRenderSystem : IRenderSystem {

        private readonly IStyleSystem m_StyleSystem;
        private readonly List<IDrawable> m_DirtyGraphicList;
        private readonly List<UIElement> m_ToInitialize;
        private readonly List<RenderData> m_RenderList; //todo -- to array
        private readonly BucketList m_Buckets;
        private readonly Camera m_Camera;
        private readonly IntMap<RenderData> m_RenderDataMap;

        private static readonly RenderZIndexComparerAscending s_ZIndexComparer = new RenderZIndexComparerAscending();

        public DirectRenderSystem(Camera camera, IStyleSystem styleSystem) {
            this.m_Camera = camera;
            this.m_StyleSystem = styleSystem;
            this.m_RenderList = new List<RenderData>();
            this.m_DirtyGraphicList = new List<IDrawable>();
            this.m_RenderDataMap = new IntMap<RenderData>();
            this.m_ToInitialize = new List<UIElement>();
            this.m_Buckets = new BucketList();
        }

        private void InitializeRenderables() {
            for (int i = 0; i < m_ToInitialize.Count; i++) {
                UIElement element = m_ToInitialize[i];

                if ((element.flags & UIElementFlags.RequiresRendering) == 0) {
                    continue;
                }

                RenderData renderData = new RenderData(element, this);
                m_RenderList.Add(renderData);

                if (element.isEnabled) {
                    m_DirtyGraphicList.Add(renderData.drawable);
                    renderData.drawable.onMeshDirty += MarkGeometryDirty;
                    renderData.drawable.onMaterialDirty += MarkMaterialDirty;
                }
            }

            m_ToInitialize.Clear();
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

        public void OnUpdate() {

            /*
             *  for meshes of type IDrawableInstanced -> use draw instanced and pass in material block
             */

            InitializeRenderables();

            if (m_Camera == null) {
                return;
            }

            MaterialPropertyBlock block;
            Material mat = Resources.Load<Material>("Materials/UIForia");
         //   Texture2D tex = Resources.Load<Texture2D>("icon_1");
            mat.color = Color.white;

            m_Camera.orthographic = true;
            m_Camera.orthographicSize = Screen.height * 0.5f;

            Vector3 origin = m_Camera.transform.position;
            origin.x -= 0.5f * Screen.width;
            origin.y += 0.5f * Screen.height;
            origin.z = 10f;

            SortGeometry();

            float z = -m_RenderList.Count;

            for (int i = 0; i < m_RenderList.Count; i++) {
                RenderData data = m_RenderList[i];
                LayoutResult layoutResult = data.element.layoutResult;
                Vector3 position = layoutResult.screenPosition;
                position.z = z++;
                position.y = -position.y;
                Rect clipRect = data.element.layoutResult.clipRect;

                if (clipRect.width <= 0 || clipRect.height <= 0) {
                    continue;
                }
                if (layoutResult.actualSize.width == 0 || layoutResult.actualSize.height == 0) {
                    continue;
                }

                float clipX = (clipRect.x - position.x) / layoutResult.actualSize.width;
                float clipY = ((clipRect.y - position.y) / layoutResult.actualSize.height);
                float clipW = clipX + (clipRect.width / layoutResult.actualSize.width);
                float clipH = clipY + (clipRect.height / layoutResult.actualSize.height);
                mat.mainTexture = null;
                mat.SetVector("_ClipRect", new Vector4(clipX, clipY, clipW, clipH));
                Graphics.DrawMesh(data.drawable.GetMesh(), origin + position, Quaternion.identity, mat, 0, m_Camera, 0, null, false, false, false);
            }

        }

        public List<RenderData> GetRenderList() {
            return m_RenderList;
        }

        private void SortGeometry() {
            if (m_RenderList.Count == 0) {
                return;
            }

            m_RenderList.Sort((a, b) => (a.element.layoutResult.layer < b.element.layoutResult.layer) ? 1 : -1);

            int layerStart = 0;
            int currentLayer = m_RenderList[0].element.layoutResult.layer;
            for (int i = 1; i < m_RenderList.Count; i++) {
                RenderData renderData = m_RenderList[i];
                int layer = renderData.element.layoutResult.layer;
                if (layer != currentLayer) {
                    m_RenderList.Sort(layerStart, i - layerStart, s_ZIndexComparer);
                    currentLayer = layer;
                    layerStart = i;
                }
            }

            for (int i = 0; i < m_RenderList.Count; i++) {
                m_RenderList[i].zOffset = i;
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
                        return first.depthIndex > second.depthIndex ? 1 : -1;
                    }

                    // otherwise resolve using raw depth
                    return first.depth > second.depth ? 1 : -1;
                }
                else {
                    return first.layoutResult.zIndex > second.layoutResult.zIndex ? 1 : -1;
                }
            }

        }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() {
            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        private void HandleStylePropertyChanged(UIElement element, StyleProperty property) {
            RenderData renderData = m_RenderDataMap.GetValueOrDefault(element.id);
            if (renderData?.drawable == null) {
                return;
            }

            // todo figure out pivots, unity re-builds meshes. figure out how rotation affects position
            if (property.propertyId == StylePropertyId.TransformPivotX || property.propertyId == StylePropertyId.TransformPivotY) {
                renderData.drawable.OnAllocatedSizeChanged();
            }

            renderData.drawable.OnStylePropertyChanged(property);

        }

        public void OnReset() {
            m_RenderList.Clear();
        }

        public void OnElementCreated(UIElement element) {
            m_ToInitialize.Add(element);
        }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            // remember to recurse, maybe rename to OnElementHierarchyDestroyed
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnElementCreatedFromTemplate(MetaData creationData) {
            m_ToInitialize.Add(creationData.element);
            for (int i = 0; i < creationData.children.Count; i++) {
                OnElementCreatedFromTemplate(creationData.children[i]);
            }
        }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) { }

    }

}