using System;
using SVGX;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace SeedLib {

    public enum PopOverEdge {

        Left,
        Right,
        Top,
        Bottom

    }

    [CustomPainter("SeedLib::Popover::TipPart")]
    public class PopOverTipPainter : StandardRenderBox {

        // We paint the popover tip by computing where the target element and popover element 
        // should align. We take the alignment point and add its x/y values to our matrix position.
        // This is done in a painter because when layout runs, we don't have enough information
        // to know where the popover rendered, because of alignment boundaries.
        public override void PaintBackground(RenderContext ctx) {

            // todo -- this is super cheating, new render system makes this not so horrible
            SVGXMatrix matrix = element.layoutResult.matrix;
            Popover popover = element.FindParent<Popover>();

            if (popover.targetElement == null) {
                return;
            }

            switch (popover.edge) {

                default:
                case PopOverEdge.Left:
                    matrix.m4 += popover.layoutResult.ActualWidth - (element.layoutResult.ActualWidth * 0.5f);
                    break;

                case PopOverEdge.Right:
                    break;

                case PopOverEdge.Top:
                    matrix.m5 += popover.layoutResult.ActualHeight - (element.layoutResult.ActualHeight * 0.5f);
                    break;

                case PopOverEdge.Bottom:
                    break;

            }

            if (popover.edge == PopOverEdge.Left || popover.edge == PopOverEdge.Right) {
                Vector2 popupPosition = popover.layoutResult.screenPosition;
                Rect targetRect = popover.targetElement.layoutResult.ScreenRect;
                float targetY = targetRect.y + (targetRect.height * 0.5f);

                float distance = targetY - popupPosition.y;
                matrix.m5 += distance - (element.layoutResult.ActualHeight * 0.5f);

                element.layoutResult.matrix = matrix;
            }
            else {
                Vector2 popupPosition = popover.layoutResult.screenPosition;
                Rect targetRect = popover.targetElement.layoutResult.ScreenRect;
                float targetX = targetRect.x + (targetRect.width * 0.5f);

                float distance = targetX - popupPosition.x;
                matrix.m4 += distance - (element.layoutResult.ActualWidth * 0.5f);

                element.layoutResult.matrix = matrix;
            }

            base.PaintBackground(ctx);
        }

    }

    [Template("SeedLib/Popover/Popover.xml")]
    public class Popover : UIElement {

        public UIElement targetElement;
        public PopOverEdge edge;
        private UIElement tipUnderlay;
        private UIElement tipOverlay;

        [OnPropertyChanged(nameof(edge))]
        public void OnEdgeChanged() {
            if (edge == PopOverEdge.Left) {
                FindById("underlay").SetAttribute("edge", "left");
                FindById("overlay").SetAttribute("edge", "left");
            }
            else if (edge == PopOverEdge.Right) {
                FindById("underlay").SetAttribute("edge", "right");
                FindById("overlay").SetAttribute("edge", "right");
            }
            else if (edge == PopOverEdge.Top) {
                FindById("underlay").SetAttribute("edge", "top");
                FindById("overlay").SetAttribute("edge", "top");
            }
            else if (edge == PopOverEdge.Bottom) {
                FindById("underlay").SetAttribute("edge", "bottom");
                FindById("overlay").SetAttribute("edge", "bottom");
            }
        }

        public override void OnEnable() {
            OnEdgeChanged();
        }

        public override void OnUpdate() {

            if (targetElement == null) {
                return;
            }

            style.SetVisibility(targetElement.layoutResult.isCulled ? Visibility.Hidden : Visibility.Visible, StyleState.Normal);
            
            Rect rect = targetElement.layoutResult.ScreenRect;
            
            // todo -- make these accessible from a theme definition in C#
            const float sp4 = 16;
            const float sp3 = 12;
            
            switch (edge) {

                default:
                case PopOverEdge.Left: {
                    float xOffset = -(sp4 + sp3); 
                    style.SetAlignmentOriginX(rect.xMin - layoutResult.ActualWidth + xOffset, StyleState.Normal);
                    style.SetAlignmentOriginY(rect.y + (rect.height * 0.5f), StyleState.Normal);
                    style.SetAlignmentOffsetY(new OffsetMeasurement(-0.5f, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    break;
                }

                case PopOverEdge.Right: {
                    float xOffset = sp4 + sp3;
                    style.SetAlignmentOriginX(xOffset + rect.xMax, StyleState.Normal);
                    style.SetAlignmentOriginY(rect.y + (rect.height * 0.5f), StyleState.Normal);
                    style.SetAlignmentOffsetY(new OffsetMeasurement(-0.5f, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    break;
                }

                case PopOverEdge.Top: {
                    float yOffset = -(sp4 + sp3);
                    style.SetAlignmentOriginY(rect.yMin - layoutResult.ActualHeight + yOffset, StyleState.Normal);
                    style.SetAlignmentOriginX(rect.x + (rect.width * 0.5f), StyleState.Normal);
                    style.SetAlignmentOffsetX(new OffsetMeasurement(-0.5f, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    break;
                }

                case PopOverEdge.Bottom: {

                    float yOffset = (sp4 + sp3);
                    style.SetAlignmentOriginY(yOffset + rect.yMax, StyleState.Normal);
                    style.SetAlignmentOriginX(rect.x + (rect.width * 0.5f), StyleState.Normal);
                    style.SetAlignmentOffsetX(new OffsetMeasurement(-0.5f, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    break;
                }

            }

        }

    }

}