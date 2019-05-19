using System;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;
using Random = System.Random;

namespace Src.Systems {

    [Flags]
    public enum RenderMethod {

        None = 0,
        Text = 1 << 0,
        Painter = 1 << 1,
        SelfPainter = 1 << 2,

        Border = 1 << 3,
        BorderRadius = 1 << 4,
        Fill = 1 << 5,
        UniformBorder = 1 << 6,
        UniformBorderRadius = 1 << 7,
        Color = 1 << 8,
        Texture = 1 << 9,
        UniformBorderFill = Border | UniformBorder | Fill,
        UniformBorderNoFill = Border | UniformBorder,
        UniformBorderRadiusUniformBorderFill = Border | UniformBorder | BorderRadius | UniformBorderRadius | Fill,
        UniformBorderRadiusUniformBorderNoFill,
        MixedBorderFill,
        NoBorderTextureFill = Fill | Texture,
        NoBorderTextureColorFill = Fill | Texture | Color,
        NoBorderColorFill = Color | Fill,
        MixedBorderNoFill = Border,
        NoBorderFilled

    }

    public struct RenderInfo {

        public int elementId;
        public Color32 backgroundColor;
        public Color32 backgroundTint;
        public Color32 textColor;
        public Color32 borderColorTop;
        public Color32 borderColorRight;
        public Color32 borderColorBottom;
        public Color32 borderColorLeft;
        public float opacity;

        // todo -- some / all of these can be packed 

        public Rect uvRect;
        public Vector2 uvTiling;
        public Vector2 uvOffset;
        public Vector2 backgroundScale;
        public float backgroundRotation;
        public ResolvedBorderRadius borderRadius;
        public Vector4 borderSize;

        // todo -- border style

        public Color32 textOutlineColor;
        public Color32 textGlowColor;
        public Vector4 clipRect;
        public Visibility visibility;
        public ISVGXPaintable painter;
        public ISVGXElementPainter selfPainter;
        public RenderMethod renderMethod;
        public int geometryId;
        public Texture backgroundImage;
        public VertigoMaterial material;
        public bool isText;

    }
    
    public class VertigoRenderSystem : IRenderSystem {

        private Camera camera;
        private ILayoutSystem layoutSystem;
        private CommandBuffer commandBuffer;
        private LightList<UIView> views;
        private IStyleSystem styleSystem;
        private IntMap<RenderInfo> renderInfos;
        private LightList<UIElement> elementsToRender;
        private readonly GraphicsContext gfx;

     
        public VertigoRenderSystem(Camera camera, ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
            this.camera = camera;
            this.layoutSystem = layoutSystem;
            this.views = new LightList<UIView>();
            this.commandBuffer = new CommandBuffer(); // todo -- per view
            this.styleSystem = styleSystem;
            this.styleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
            this.renderInfos = new IntMap<RenderInfo>();
            this.elementsToRender = new LightList<UIElement>(0);
            this.camera?.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            this.gfx = new GraphicsContext();
           
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> propertyList) {
            // todo -- only update changed things
            UpdateElementStyles(element);
        }

        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void OnReset() { }

        public void OnUpdate() {

            camera.orthographicSize = Screen.height * 0.5f;

            for (int i = 0; i < views.Count; i++) {
                RenderView(views[i]);
            }
            
        }

        private readonly Material defaultMaterial = new Material(Shader.Find("Vertigo/VertigoSDF"));
        ShapeCache geometry = new ShapeCache();

        private void RenderView(UIView view) {
            // todo -- figure out per view rendering, probably want per view render & layout systems & maybe input too 
            geometry.ClearGeometryData();
            gfx.BeginFrame();
            gfx.SetMaterial(defaultMaterial);

        

            UIForiaGraphicsInterface ctx = gfx.GetRenderInterface<UIForiaGraphicsInterface>();
           

            gfx.SetTransform(Matrix4x4.TRS(new Vector3(-Screen.width * 0.5f, Screen.height * 0.5f), Quaternion.identity, Vector3.one));

            layoutSystem.GetVisibleElements(elementsToRender);

            UIElement[] visibleElements = elementsToRender.Array;
            int count = elementsToRender.Count;

            if (count == 0) return;

            for (int i = count - 1; i >= 0; i--) {
                UIElement element = visibleElements[i];

                UpdateElementStyles(element);

                if (!renderInfos.TryGetValue(element.id, out RenderInfo renderInfo)) {
                    continue;
                }

                if (renderInfo.renderMethod == RenderMethod.None) {
                    continue;
                }

                LayoutResult layoutResult = element.layoutResult;

                Vector2 position = layoutResult.screenPosition;
                Size size = layoutResult.allocatedSize;

                RenderMethod renderMethod = renderInfo.renderMethod;

                if (renderMethod == RenderMethod.None) {
                    continue;
                }

                if ((renderMethod & RenderMethod.Border) != 0) {
                    // border radius
                    // different border sizes
                    // different border colors
                    
                }
                ctx.SetTextureProperty(ShaderKey.MainTexture, renderInfo.backgroundImage);
                ctx.fillColor = renderInfo.backgroundColor;
                ctx.FillRect(position.x, position.y, size.width, size.height, renderInfo);
                switch (renderInfo.renderMethod) {
                    case RenderMethod.Painter:
                        ctx.Save();
//                        renderInfo.painter.Paint();
                        ctx.Restore();
                        break;

                    case RenderMethod.SelfPainter:
                        ctx.Save();
//                        renderInfo.painter.Paint();
                        ctx.Restore();
                        break;

                    case RenderMethod.NoBorderColorFill:
                        ctx.fillColor = renderInfo.backgroundColor;
                        ctx.FillRect(position.x, position.y, size.width, size.height, renderInfo);
                        break;
                    
                    case RenderMethod.UniformBorderNoFill:
                        
                        // maskSoftness, shapeType, color flags, line data?
                        // sdf data                 texcoord 1
                        // border colors (packed)   texcoord 2
                        // clip rect                texcoord 3
                        
//                        ctx.SetStrokeColor(renderInfo.borderColorTop);
//                        ctx.SetStrokeWidth(layoutResult.border.top);
//                        ctx.StrokeRect(position.x, position.y, size.width, size.height);
                        break;
                    
                    case RenderMethod.MixedBorderNoFill:
                        // todo -- take border color properly
//                        ctx.SetStrokeColor(renderInfo.borderColorTop);
//                        ctx.StrokeRectNonUniform(position.x, position.y, size.width, size.height, new OffsetStrokeRect() {
//                            topSize = layoutResult.border.top,
//                            rightSize = layoutResult.border.right,
//                            bottomSize = layoutResult.border.bottom,
//                            leftSize = layoutResult.border.left,
//                        });
                        break;
                    
                    case RenderMethod.NoBorderTextureFill:
//                        ctx.SetTexture(ShaderKey.MainTexture, renderInfo.backgroundImage);
////                        
//                        ctx.FillRect(position.x, position.y, size.width, size.height, renderInfo);
//                        Vector4 renderData = new Vector4((int) ColorType.TextureOnly, 0, 0, 0);
//                        int metaDta = 10;
//                        float packedBorderRadius = 0;
//                        float strokeWidth = 0;
//                        
//                        ctx.SetTexCoord1(shapeId, new Vector4(packedBorderRadius, (int) ShapeType.Rect, 0, 0));
//                        ctx.SetTexCoord2(shapeId, new Vector4((int) ColorType.TextureOnly, 0, 0, 0));
                        break;
                }
            }

            gfx.EndFrame(commandBuffer, camera.worldToCameraMatrix, camera.projectionMatrix);

        }

        public enum ColorType {

            TextureOnly = 1,
            TextureTint = 2,
            TextureColor = 3,
            TextureColorTint = 4

        }

        struct RenderData {

            public ColorType colorType;

        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            views.Add(view);
            // todo -- each view can take its own camera
            // each view has its own camera origin
            // each view can attach to a different camera event
        }

        public void OnViewRemoved(UIView view) {
            views.Remove(view);
        }

        private static RenderMethod ComputeRenderType(UIElement element, in RenderInfo info) {
            LayoutResult layoutResult = element.layoutResult;

            if (info.visibility == Visibility.Hidden || info.opacity <= 0) {
                return RenderMethod.None;
            }

            if (info.painter != null) {
                return RenderMethod.Painter;
            }
            else if (info.selfPainter != null) {
                return RenderMethod.SelfPainter;
            }
            else if (info.isText) {
                return RenderMethod.Text;
            }

            Texture bg = info.backgroundImage;
            Color32 bgColor = info.backgroundColor;

            bool hasBorderRadius = !layoutResult.borderRadius.IsZero;
            bool hasBorderColor = info.borderColorTop.a + info.borderColorBottom.a + info.borderColorRight.a + info.borderColorLeft.a == 0;
            bool hasBorder = !layoutResult.border.IsZero;

//            if ((bg == null && bgColor.a == 0 && (!hasBorderRadius && !hasBorderColor))) {
//                return RenderMethod.None;
//            }

            RenderMethod retn = 0;

            if (bgColor.a > 0) {
                retn |= RenderMethod.Color;
            }

            if (bg != null) {
                retn |= RenderMethod.Texture;
            }

            if (bg != null || bgColor.a != 0) {
                retn |= RenderMethod.Fill;
            }

            if (hasBorderRadius) {
                retn |= RenderMethod.BorderRadius;
                if (layoutResult.borderRadius.IsUniform) {
                    retn |= RenderMethod.UniformBorderRadius;
                }
            }

            if (hasBorder) {
                retn |= RenderMethod.Border;
                if (layoutResult.border.IsUniform) {
                    retn |= RenderMethod.UniformBorder;
                }
            }

            return retn;
        }

        private void UpdateElementStyles(UIElement element) {
            RenderInfo renderInfo = new RenderInfo();
            UIStyleSet style = element.style;
            renderInfo.backgroundColor = style.BackgroundColor;
            renderInfo.backgroundImage = style.BackgroundImage;
            renderInfo.backgroundRotation = style.BackgroundImageRotation.value; // todo -- resolve this to a float
            renderInfo.opacity = style.Opacity;
            // todo resolve to float
            renderInfo.backgroundScale = new Vector2(style.BackgroundImageScaleX.value, style.BackgroundImageScaleY.value);
            renderInfo.visibility = style.Visibility;
            renderInfo.uvRect = new Rect(0, 0, 1, 1);
            renderInfo.uvOffset = new Vector2(0, 0);
            renderInfo.uvTiling = new Vector2(1, 1);
            renderInfo.backgroundTint = style.BackgroundTint;
            renderInfo.borderRadius = element.layoutResult.borderRadius;
            renderInfo.clipRect = Vector4.zero;
            renderInfo.borderSize = element.layoutResult.border;
            renderInfo.textGlowColor = style.TextGlowColor;
            renderInfo.textColor = style.TextColor;
            renderInfo.borderColorTop = Color.black;
            renderInfo.borderColorRight = Color.black;
            renderInfo.borderColorBottom = Color.black;
            renderInfo.borderColorLeft = Color.black;
            renderInfos.geometryId = -1;

            renderInfo.renderMethod = ComputeRenderType(element, renderInfo);

            renderInfos[element.id] = renderInfo;
        }

        public void OnElementEnabled(UIElement element) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            stack.Push(element);
            while (stack.Count > 0) {
                UIElement current = stack.PopUnchecked();

                if (current.isDisabled) {
                    continue;
                }

                // todo -- only if render-relevant style
                UpdateElementStyles(current);

                int childCount = current.children.Count;
                UIElement[] children = current.children.Array;
                for (int i = 0; i < childCount; i++) {
                    stack.Push(children[i]);
                }
            }

            LightStack<UIElement>.Release(ref stack);
        }

        public void OnElementDisabled(UIElement element) {
            renderInfos.Remove(element.id);
        }

        public void OnElementDestroyed(UIElement element) {
            renderInfos.Remove(element.id);
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        public void SetCamera(Camera camera) {
            this.camera?.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            this.camera = camera; // todo -- should be handled by the view
            this.camera?.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
        }

    }

}