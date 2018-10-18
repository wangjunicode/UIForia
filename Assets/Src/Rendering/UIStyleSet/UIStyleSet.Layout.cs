using System.Collections.Generic;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public LayoutType GetLayoutType(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutType, state);
            return property.IsDefined ? (LayoutType) property.valuePart0 : LayoutType.Unset;
        }

        public void SetLayoutType(LayoutType layoutType, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.LayoutType = layoutType;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.LayoutType)) {
                computedStyle.LayoutType = layoutType;
            }
        }

        public LayoutBehavior GetLayoutBehavior(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutBehavior, state);
            return property.IsDefined ? (LayoutBehavior) property.valuePart0 : LayoutBehavior.Unset;
        }

        public void SetLayoutBehavior(LayoutBehavior behavior, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.LayoutBehavior = behavior;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.LayoutBehavior)) {
                computedStyle.LayoutBehavior = behavior;
            }
        }

        public void SetOverflowX(Overflow value, StyleState state) {
            SetEnumProperty(StylePropertyId.OverflowX, (int) value, state);
        }

        public Overflow GetOverflowX(StyleState state) {
            return (Overflow) GetEnumProperty(StylePropertyId.OverflowX, state);
        }

        public void SetOverflowY(Overflow value, StyleState state) {
            SetEnumProperty(StylePropertyId.OverflowY, (int) value, state);
        }

        public Overflow GetOverflowY(StyleState state) {
            return (Overflow) GetEnumProperty(StylePropertyId.OverflowY, state);
        }

        public void SetRenderLayer(RenderLayer layer, StyleState state) {
            SetEnumProperty(StylePropertyId.RenderLayer, (int) layer, state);
        }

        public RenderLayer GetRenderLayer(StyleState state) {
            return (RenderLayer) GetEnumProperty(StylePropertyId.RenderLayer, state);
        }

        public void SetRenderLayerOffset(int offset, StyleState state) {
            SetIntProperty(StylePropertyId.RenderLayerOffset, offset, state);
        }

        public int GetRenderLayerOffset(StyleState state) {
            return GetIntValue(StylePropertyId.RenderLayerOffset, state);
        }

        public void SetZIndex(int zIndex, StyleState state) {
            SetIntProperty(StylePropertyId.ZIndex, zIndex, state);
        }

        public int GetZIndex(StyleState state) {
            return GetIntValue(StylePropertyId.ZIndex, state);
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
            return GetFixedLengthValue(StylePropertyId.AnchorTop, state);
        }

        public UIFixedLength GetAnchorRight(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.AnchorRight, state);
        }

        public UIFixedLength GetAnchorBottom(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.AnchorBottom, state);
        }

        public UIFixedLength GetAnchorLeft(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.AnchorLeft, state);
        }

        public void SetAnchorTarget(AnchorTarget target, StyleState state) {
            SetEnumProperty(StylePropertyId.AnchorTarget, (int)target, state);
        }

        public AnchorTarget GetAnchorTarget(StyleState state) {
            return (AnchorTarget) GetEnumProperty(StylePropertyId.AnchorTarget, state);
        }

    }

}