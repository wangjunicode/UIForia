using System;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public enum BorderType {

        None = 0,
        UniformNormal,
        UniformRounded,
        VaryingNormal,
        VaryingRounded,

    }

    public class SVGXRenderSystem : IRenderSystem {

        private readonly ImmediateRenderContext ctx;
        private readonly GFX gfx;
        private Camera m_Camera;
        private LightList<UIView> views;
        private ILayoutSystem layoutSystem;

        public SVGXRenderSystem(Camera camera, ILayoutSystem layoutSystem) {
            this.m_Camera = camera;
            gfx = new GFX(camera);
            ctx = new ImmediateRenderContext();
            this.views = new LightList<UIView>();
            this.layoutSystem = layoutSystem;
        }

        public void OnReset() {
            this.views.Clear();
        }

        public void OnUpdate() {
            ctx.Clear();

            //todo -- use non alloc version
            LightList<UIElement> visibleElements = layoutSystem.GetVisibleElements();

            RenderView(views[0], visibleElements);

            gfx.Render(ctx);
            
        }

        private static void DrawNormalFill(ImmediateRenderContext ctx, UIElement element) {
            Color bgColor = element.style.BackgroundColor;
            Texture2D bgImage = element.style.BackgroundImage;

            if (bgColor.IsDefined() && bgImage != null) {
                ctx.SetFill(bgImage, bgColor);
                ctx.Fill();
            }
            else if (bgColor.IsDefined()) {
                ctx.SetFill(bgColor);
                ctx.Fill();
            }
            else if (bgImage != null) {
                ctx.SetFill(bgImage);
                ctx.Fill();
            }
        }

        private void RenderView(UIView view, LightList<UIElement> visibleElements) {
            m_Camera.orthographic = true;
            m_Camera.orthographicSize = Screen.height * 0.5f;
            visibleElements.Reverse(); // todo -- dont' do this, also don't get a reference directly to the layout system's visible elements

            UIElement[] elementArray = visibleElements.Array;
            for (int i = 0; i < visibleElements.Count; i++) {
                UIElement current = elementArray[i];

                if (current.style.Visibility == Visibility.Hidden) {
                    continue;
                }

                LayoutResult layoutResult = current.layoutResult;

                // todo -- if no paint properties are set, don't draw (ie no bg, no colors, no border, etc)

                // todo -- opacity will need to be inherited

                Vector2 offset = new Vector2();
                SVGXMatrix matrix = layoutResult.matrix;

                string painterName = current.style.Painter;
                if (painterName != string.Empty) {
                    ctx.BeginPath();
                    if (painterName == "self") {
                        ISVGXPaintable painter = current as ISVGXPaintable;
                        if (painter == null) {
                            Debug.LogWarning($"painter type of self was used with element of type: {current.GetType()} but that types does not implement {nameof(ISVGXPaintable)} ");
                        }
                        else {
                            ctx.SetTransform(SVGXMatrix.identity);
                            painter.Paint(ctx, matrix);
                        }
                    }
                    else if (painterName != "none") {
                        ISVGXElementPainter painter = Application.GetCustomPainter(painterName);
                        ctx.SetTransform(SVGXMatrix.identity);
                        painter?.Paint(current, ctx, matrix);
                    }

                    continue;
                }

                ctx.SetTransform(matrix);

                if (current is UITextElement textElement) {
                    ctx.BeginPath();
                    ctx.SetFillOpacity(textElement.style.Opacity);
                    ctx.SetStrokeOpacity(textElement.style.Opacity);
                    ctx.Text(-offset.x, -offset.y, textElement.textInfo);
                    ctx.SetFill(textElement.style.TextColor);
                    ctx.Fill();
                }
                else {
                    PaintElement(ctx, current);
                }

//                Size scrollbarVerticalSize = current.layoutResult.scrollbarVerticalSize;
//                if (scrollbarVerticalSize.IsDefined()) {
//                    Scrollbar scrollbar = Application.GetCustomScrollbar(null);
//                    Vector2 screenPosition = current.layoutResult.screenPosition;
//                    float width = current.layoutResult.allocatedSize.width;
//                    Vector2 pos = new Vector2(screenPosition.x + width - scrollbarVerticalSize.width, screenPosition.y);
//                    matrix = SVGXMatrix.TRS(pos, 0, Vector2.one);
//                    scrollbar.Paint(current, scrollbarVerticalSize, ctx, matrix);
//                }

            }

            ctx.DisableScissorRect();
            DrawDebugOverlay?.Invoke(ctx);

            // if requires stencil clip -> do stencil clip true if overflow is not hidden and shape is not rect. not sure how to handle z order here
            // children with elevated z probably get lifted into ancestor's clipping scope. 


            // walk the tree top down
            // if not enabled return false
            // if culled continue to children
            // if culled & overflow != visible stop

            // apply clipping
            // apply styles
            // stroke / fill
            // render children
            // pop clip
            // return true
        }

        public static void PaintElement(ImmediateRenderContext ctx, UIElement current) {
            ctx.BeginPath();

            LayoutResult layoutResult = current.layoutResult;
            OffsetRect borderRect = layoutResult.border;

            Vector4 border = layoutResult.border;
            Vector4 resolveBorderRadius = layoutResult.borderRadius;
            float width = layoutResult.allocatedSize.width;
            float height = layoutResult.allocatedSize.height;
            bool hasUniformBorder = border.x == border.y && border.z == border.x && border.w == border.x;
            bool hasBorder = border.x > 0 || border.y > 0 || border.z > 0 || border.w > 0;

            Vector2 screenPos = current.layoutResult.screenPosition;

            // todo -- implement background scrolling style properties

            if (current.style.OverflowX != Overflow.Visible) {
                Size allocated = current.layoutResult.allocatedSize;
                ctx.EnableScissorRect(new Rect(screenPos.x, screenPos.y, allocated.width, allocated.height));
            }
            else {
                ctx.DisableScissorRect();
            }

            ctx.SetFillOpacity(current.style.Opacity);
            ctx.SetStrokeOpacity(current.style.Opacity);
            if (resolveBorderRadius == Vector4.zero) {
                ctx.Rect(borderRect.left, borderRect.top, layoutResult.allocatedSize.width - borderRect.Horizontal, layoutResult.allocatedSize.height - borderRect.Vertical);

                if (!hasBorder) {
                    DrawNormalFill(ctx, current);
                }
                else {
                    if (hasUniformBorder) {
                        DrawNormalFill(ctx, current);
                        ctx.SetStrokePlacement(StrokePlacement.Outside);
                        ctx.SetStrokeWidth(border.x);
                        ctx.SetStroke(current.style.BorderColor);
                        ctx.Stroke();
                    }
                    else {
                        DrawNormalFill(ctx, current);

                        ctx.SetStrokeOpacity(1f);
                        ctx.SetStrokePlacement(StrokePlacement.Inside);
                        ctx.SetStroke(current.style.BorderColor);
                        ctx.SetFill(current.style.BorderColor);

                        // todo this isn't really working correctly,
                        // compute single stroke path on cpu. current implementation has weird blending overlap artifacts with transparent border color

                        if (borderRect.top > 0) {
                            ctx.BeginPath();
                            ctx.Rect(borderRect.left, 0, width - borderRect.Horizontal, borderRect.top);
                            ctx.Fill();
                        }

                        if (borderRect.right > 0) {
                            ctx.BeginPath();
                            ctx.Rect(width - borderRect.right, 0, borderRect.right, height);
                            ctx.Fill();
                        }

                        if (borderRect.left > 0) {
                            ctx.BeginPath();
                            ctx.Rect(0, 0, borderRect.left, height);
                            ctx.Fill();
                        }

                        if (borderRect.bottom > 0) {
                            ctx.BeginPath();
                            ctx.Rect(borderRect.left, height - borderRect.bottom, width - borderRect.Horizontal, borderRect.bottom);
                            ctx.Fill();
                        }
                    }
                }
            }
            // todo -- might need to special case non uniform border with border radius
            else {
                ctx.BeginPath();
                ctx.RoundedRect(new Rect(borderRect.left, borderRect.top, width - borderRect.Horizontal, height - borderRect.Vertical), resolveBorderRadius.x, resolveBorderRadius.y, resolveBorderRadius.z, resolveBorderRadius.w);
                DrawNormalFill(ctx, current);
                if (hasBorder) {
                    ctx.SetStrokeWidth(borderRect.top);
                    ctx.SetStroke(current.style.BorderColor);
                    ctx.Stroke();
                }
            }
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            views.Add(view);
        }

        public void OnViewRemoved(UIView view) {
            views.Remove(view);
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void SetCamera(Camera camera) {
            if (!camera.orthographic) {
                throw new Exception("The camera used to render the UI must be marked as orthographic");
            }

            if (camera.farClipPlane < 50000) {
                Debug.LogWarning("The camera used to render the UI should have a far clip plane set to at least 50000");
            }

            m_Camera = camera;
            gfx.SetCamera(camera);
        }

    }

}