using System.Collections.Generic;
using UIForia;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Layout;
using UnityEngine;

namespace UIForia.Rendering {

    public partial class UIStyleSet {

        public LayoutType GetLayoutType(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutType, state);
            return property.IsDefined ? (LayoutType) property.valuePart0 : LayoutType.Unset;
        }

        public void SetLayoutType(LayoutType layoutType, StyleState state) {
            SetEnumProperty(StylePropertyId.LayoutType, (int)layoutType, state);
        }

        public LayoutBehavior GetLayoutBehavior(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutBehavior, state);
            return property.IsDefined ? (LayoutBehavior) property.valuePart0 : LayoutBehavior.Unset;
        }

        public void SetLayoutBehavior(LayoutBehavior behavior, StyleState state) {
            SetEnumProperty(StylePropertyId.LayoutBehavior, (int)behavior, state);
        }

        public void SetOverflowX(Overflow value, StyleState state) {
            SetEnumProperty(StylePropertyId.OverflowX, (int) value, state);
        }

        public Overflow GetOverflowX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow;
        }

        public void SetOverflowY(Overflow value, StyleState state) {
            SetEnumProperty(StylePropertyId.OverflowY, (int) value, state);
        }

        public Overflow GetOverflowY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow;
        }

        public void SetRenderLayer(RenderLayer layer, StyleState state) {
            SetEnumProperty(StylePropertyId.RenderLayer, (int) layer, state);
        }

        public RenderLayer GetRenderLayer(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RenderLayer, state).AsRenderLayer;
        }

        public void SetRenderLayerOffset(int offset, StyleState state) {
            SetIntProperty(StylePropertyId.RenderLayerOffset, offset, state);
        }

        public int GetRenderLayerOffset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RenderLayerOffset, state).AsInt;
        }

        public void SetZIndex(int zIndex, StyleState state) {
            SetIntProperty(StylePropertyId.ZIndex, zIndex, state);
        }

        public int GetZIndex(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ZIndex, state).AsInt;
        }

        public void SetAnchorTopLeft(FixedLengthVector anchor, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.AnchorLeft, anchor.x, state);
            SetFixedLengthProperty(StylePropertyId.AnchorTop, anchor.y, state);
        }

        public void SetAnchorBottomRight(FixedLengthVector anchor, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.AnchorRight, anchor.x, state);
            SetFixedLengthProperty(StylePropertyId.AnchorBottom, anchor.y, state);
        }

        public void SetAnchorTop(UIFixedLength anchor, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.AnchorTop, anchor, state);
        }

        public void SetAnchorRight(UIFixedLength anchor, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.AnchorRight, anchor, state);
        }

        public void SetAnchorBottom(UIFixedLength anchor, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.AnchorBottom, anchor, state);
        }

        public void SetAnchorLeft(UIFixedLength anchor, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.AnchorLeft, anchor, state);
        }

        public UIFixedLength GetAnchorTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorTop, state).AsUIFixedLength;
        }

        public UIFixedLength GetAnchorRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorRight, state).AsUIFixedLength;
        }

        public UIFixedLength GetAnchorBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorBottom, state).AsUIFixedLength;
        }

        public UIFixedLength GetAnchorLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorLeft, state).AsUIFixedLength;
        }

        public void SetAnchorTarget(AnchorTarget target, StyleState state) {
            SetEnumProperty(StylePropertyId.AnchorTarget, (int)target, state);
        }

        public AnchorTarget GetAnchorTarget(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorTarget, state).AsAnchorTarget;
        }

    }

}