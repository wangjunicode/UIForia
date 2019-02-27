using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Extensions;
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

            ctx.SetStrokeColor(Color.red);
            ctx.SetStrokeWidth(2);
            
            ctx.MoveTo(-300, 0);
            ctx.LineTo(300, 0);
            ctx.Stroke();
            
            ctx.BeginPath();
            ctx.MoveTo(-300, 200);
            ctx.LineTo(300, 200);
            ctx.Stroke();
            
            ctx.BeginPath();
            ctx.MoveTo(0, -200);
            ctx.LineTo(0, 400);
            ctx.Stroke();
            
            ctx.BeginPath();
            ctx.MoveTo(200, -200);
            ctx.LineTo(200, 400);
            ctx.Stroke();
            
            
            UIElement[] elementArray = visibleElements.Array;
            for (int i = 0; i < visibleElements.Count; i++) {
                UIElement current = elementArray[i];
                LayoutResult layoutResult = current.layoutResult;


                // todo -- if no paint properties are set, don't draw (ie no bg, no colors, no border, etc)

                // todo -- opacity will need to be inherited

                SVGXMatrix matrix = SVGXMatrix.TRS(layoutResult.screenPosition, 0, Vector2.one);

                ctx.SetTransform(matrix);

                if (current is UITextElement textElement) {
                    ctx.Text(layoutResult.screenPosition.x, layoutResult.screenPosition.y, textElement.textInfo);
                    ctx.SetFill(textElement.style.TextColor);
                    ctx.Fill();
                }
//                else if (current.customShape != null) {
//                    ctx.BeginPath();
//                    current.customShape(ctx, current.element);
//                    // restore?
//                }
                else {
                    ctx.BeginPath();

                    OffsetRect borderRect = layoutResult.border; //new OffsetRect(10, 10, 10, 10);

                    ctx.Rect(borderRect.left, borderRect.top, layoutResult.actualSize.width - borderRect.Horizontal, layoutResult.actualSize.height - borderRect.Vertical);

                    Vector4 border = current.style.ResolvedBorder;
                    bool hasBorder = border.x > 0 || border.y > 0 || border.z > 0 || border.w > 0;
                    if (!hasBorder) {
//                        DrawNormalFill(current);
                    }
                    else {
                        bool hasUniformBorder = border.x == border.y && border.z == border.x && border.w == border.x;

                        if (hasUniformBorder) {
//                            DrawNormalFill(current);
                            ctx.SetStrokePlacement(StrokePlacement.Outside);
                            ctx.SetStrokeWidth(border.x);
                            ctx.SetStrokeColor(current.style.BorderColor);
                            ctx.Stroke();
                        }
                        else {
                            
                            DrawNormalFill(current);
                            
                            ctx.SetStrokeOpacity(1f);
                            ctx.SetStrokePlacement(StrokePlacement.Outside);
                            ctx.SetStrokeColor(current.style.BorderColor);
                            
                            float width = layoutResult.actualSize.width;
                            float height = layoutResult.actualSize.height;

                            if (borderRect.top > 0) {
                                ctx.BeginPath();
                                float topWidth = width - borderRect.Horizontal;
                                float leftWidth = borderRect.left;
                                ctx.MoveTo(leftWidth + (topWidth * 0.5f), 0);
                                ctx.LineTo(leftWidth + (topWidth * 0.5f), borderRect.top);
                                ctx.SetStrokeWidth(topWidth);
                                ctx.Stroke();
                            }

                            if (borderRect.right > 0) {
                                ctx.BeginPath();
                                float rightWidth = borderRect.right;
                                ctx.MoveTo(width - (rightWidth * 0.5f), 0);
                                ctx.LineTo(width - (rightWidth * 0.5f), height);
                                ctx.SetStrokeWidth(rightWidth);
                                ctx.Stroke();
                            }
                            
                            if (borderRect.left > 0) {
                                ctx.BeginPath();
                                float leftWidth = borderRect.left;
                                ctx.MoveTo(leftWidth * 0.5f, 0);
                                ctx.LineTo(leftWidth * 0.5f, height);
                                ctx.SetStrokeWidth(leftWidth);
                                ctx.Stroke();
                            }

                            if (borderRect.bottom > 0) {
                                ctx.BeginPath();
                                float bottomWidth = width - borderRect.Horizontal;
                                float leftWidth = borderRect.left;
                                ctx.MoveTo(leftWidth + (bottomWidth * 0.5f), height - borderRect.bottom);
                                ctx.LineTo(leftWidth + (bottomWidth * 0.5f), height);
                                ctx.SetStrokeWidth(bottomWidth);
                                ctx.Stroke();
                            }
                            
                        }
                    }
                }
            }


//                    switch (current.borderType) {
//                        case BorderType.None:
//                          
//                            break;
//                        case BorderType.UniformNormal:
//                            break;
//                        case BorderType.VaryingNormal:
//                            break;
//                        case BorderType.UniformRounded:
//                            break;
//                        case BorderType.VaryingRounded:
//                            break;
//                    }


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