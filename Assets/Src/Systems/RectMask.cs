using System;
using System.Collections.Generic;
using Src.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Src.Systems {



//    [AddComponentMenu("UI/Rect Mask 2D", 13)]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class RectMask : UIBehaviour, IClipper, ICanvasRaycastFilter {

//        [NonSerialized] private readonly RectangularVertexClipper m_VertexClipper = new RectangularVertexClipper();

        [NonSerialized] private RectTransform m_RectTransform;

        [NonSerialized] private readonly HashSet<IClippable> m_ClipTargets = new HashSet<IClippable>();
        [NonSerialized] private readonly List<RectMask2D> m_Clippers = new List<RectMask2D>();

        [NonSerialized] private bool m_ShouldRecalculateClipRects;


        [NonSerialized] private Rect m_LastClipRectCanvasSpace;
        [NonSerialized] private bool m_LastValidClipRect;
        [NonSerialized] private bool m_ForceClip;

        public Rect canvasRect {
            get {
                Canvas canvas = null;
                List<Canvas> list = ListPool<Canvas>.Get();
                gameObject.GetComponentsInParent(false, list);
                if (list.Count > 0) {
                    canvas = list[list.Count - 1];
                }

                ListPool<Canvas>.Release(list);

                return rectTransform.rect;
            }
        }

        public RectTransform rectTransform => m_RectTransform ? m_RectTransform : (m_RectTransform = GetComponent<RectTransform>());

        protected override void OnEnable() {
            base.OnEnable();
            m_ShouldRecalculateClipRects = true;
            ClipperRegistry.Register(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

        protected override void OnDisable() {
            // we call base OnDisable first here
            // as we need to have the IsActive return the
            // correct value when we notify the children
            // that the mask state has changed.
            base.OnDisable();
            m_ClipTargets.Clear();
            m_Clippers.Clear();
            ClipperRegistry.Unregister(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            m_ShouldRecalculateClipRects = true;

            if (!IsActive()) {
                return;
            }

            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#endif

        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera) {
            if (!isActiveAndEnabled)
                return true;

            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
        }

        public virtual void PerformClipping() {
            if (m_ShouldRecalculateClipRects) {
               // MaskUtilities.GetRectMasksForClip(this, m_Clippers);
                m_ShouldRecalculateClipRects = false;
            }

            // get the compound rects from
            // the clippers that are valid
            bool validRect = true;
            Rect clipRect = Clipping.FindCullAndClipWorldRect(m_Clippers, out validRect);
            bool clipRectChanged = clipRect != m_LastClipRectCanvasSpace;
            if (clipRectChanged || m_ForceClip) {
                foreach (IClippable clipTarget in m_ClipTargets) {
                    clipTarget.SetClipRect(clipRect, validRect);
                }

                m_LastClipRectCanvasSpace = clipRect;
                m_LastValidClipRect = validRect;
            }

            foreach (IClippable clipTarget in m_ClipTargets) {
                MaskableGraphic maskable = clipTarget as MaskableGraphic;
                if (maskable != null && !maskable.canvasRenderer.hasMoved && !clipRectChanged) {
                    continue;
                }

                clipTarget.Cull(m_LastClipRectCanvasSpace, m_LastValidClipRect);
            }
        }

        public void AddClippable(IClippable clippable) {
            if (clippable == null) {
                return;
            }

            m_ShouldRecalculateClipRects = true;
            if (!m_ClipTargets.Contains(clippable)) {
                m_ClipTargets.Add(clippable);
            }

            m_ForceClip = true;
        }

        public void RemoveClippable(IClippable clippable) {
            if (clippable == null) {
                return;
            }

            m_ShouldRecalculateClipRects = true;
            clippable.SetClipRect(new Rect(), false);
            m_ClipTargets.Remove(clippable);

            m_ForceClip = true;
        }

        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();
            m_ShouldRecalculateClipRects = true;
        }

        protected override void OnCanvasHierarchyChanged() {
            base.OnCanvasHierarchyChanged();
            m_ShouldRecalculateClipRects = true;
        }

    }

}