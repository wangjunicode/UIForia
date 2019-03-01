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

        public class ElementRenderData {

            public UIElement element;
            public Color32 backgroundColor;
            public Color32 borderColor;
            public Texture2D backgroundImage;
            public float opacity;
            public SVGXMatrix matrix;
            public CullResult cullResult;
            public int elementType;

            public Action<ImmediateRenderContext, UIElement> customShape;

            public bool isTextElement;
            public BorderType borderType;

            public ElementRenderData parent;
            public ElementRenderData nextSibling;

        }

        public void OnUpdate() {
            ctx.Clear();

            LightList<UIElement> visibleElements = layoutSystem.GetVisibleElements();

            for (int i = 0; i < views.Count; i++) {
                RenderView(views[i], visibleElements);
            }

            gfx.Render(ctx);
        }

        private void DrawNormalFill(UIElement element) {
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

                Vector2 pivot = Vector2.zero;//new Vector2(0.5f, 0.5f); //layoutResult.pivot);layoutResult.pivot; // [0, 1]

                Vector2 offset = new Vector2(layoutResult.actualSize.width * pivot.x, layoutResult.actualSize.height * pivot.y);
                SVGXMatrix matrix = SVGXMatrix.TRS(layoutResult.screenPosition + offset, layoutResult.rotation, Vector2.one);

                ctx.SetTransform(matrix);

                if (current is UITextElement textElement) {
                    ctx.BeginPath();
                    ctx.Text(-offset.x, -offset.y, textElement.textInfo);
                    ctx.SetFill(textElement.style.TextColor);
                    ctx.Fill();
                }
//                else if (current.customPainter != null) {
//                    ctx.BeginPath();
//                    current.customShape(ctx, current.element);
//                    // restore?
//                }
                else {
                    ctx.BeginPath();

                    OffsetRect borderRect = layoutResult.border;

                    Vector4 border = current.style.ResolvedBorder;
                    Vector4 resolveBorderRadius = current.style.ResolvedBorderRadius;
                    float width = layoutResult.actualSize.width;
                    float height = layoutResult.actualSize.height;
                    bool hasUniformBorder = border.x == border.y && border.z == border.x && border.w == border.x;
                    bool hasBorder = border.x > 0 || border.y > 0 || border.z > 0 || border.w > 0;

                    if (resolveBorderRadius == Vector4.zero) {
                        //ctx.Rect(-offset.x, -offset.y, layoutResult.actualSize.width - borderRect.Horizontal, layoutResult.actualSize.height - borderRect.Vertical);
                        ctx.Rect(borderRect.left - offset.x, borderRect.top - offset.y, layoutResult.actualSize.width - borderRect.Horizontal, layoutResult.actualSize.height - borderRect.Vertical);

                        if (!hasBorder) {
                            DrawNormalFill(current);
                        }
                        else {
                            if (hasUniformBorder) {
                                DrawNormalFill(current);
                                ctx.SetStrokePlacement(StrokePlacement.Outside);
                                ctx.SetStrokeWidth(border.x);
                                ctx.SetStrokeColor(current.style.BorderColor);
                                ctx.Stroke();
                            }
                            else {
                                DrawNormalFill(current);

//                                ctx.SetUVBounds();
                                ctx.SetStrokeOpacity(1f);
                                ctx.SetStrokePlacement(StrokePlacement.Inside);
                                ctx.SetStrokeColor(current.style.BorderColor);
                                ctx.SetFill(current.style.BorderColor);

                                // todo this isn't really working correctly,
                                // compute single stroke path on cpu. current implementation has weird blending overlap artifacts with transparent border color

                                if (borderRect.top > 0) {
                                    ctx.BeginPath();
                                    ctx.Rect(borderRect.left, 0, width - borderRect.Horizontal, borderRect.top - 1);
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
                        ctx.RoundedRect(new Rect(borderRect.left - offset.x, borderRect.top - offset.y, width - borderRect.Horizontal, height - borderRect.Vertical), resolveBorderRadius.x, resolveBorderRadius.y, resolveBorderRadius.z, resolveBorderRadius.w);
                        DrawNormalFill(current);
                        if (hasBorder) {
                            ctx.SetStrokeWidth(borderRect.top);
                            ctx.SetStrokeColor(current.style.BorderColor);
                            ctx.Stroke();
                        }
                    }
                }
            }


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

        public event Action<LightList<RenderData>, LightList<RenderData>, Vector3, Camera> DrawDebugOverlay;

        public RenderData GetRenderData(UIElement element) {
            return new RenderData(element);
        }

        public void SetCamera(Camera camera) {
            m_Camera = camera;
            gfx.SetCamera(camera);
        }

    }

}