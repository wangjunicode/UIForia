using System;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;

namespace Packages.UIForia.Src.Systems {

    public class VertigoRenderSystem : IRenderSystem {

        private VertigoContext ctx;
        private Camera camera;
        private ILayoutSystem layoutSystem;
        private CommandBuffer commandBuffer;
        private LightList<UIView> views;

        // todo -- per view
        private IntMap<RenderInfo> renderInfos;

        public VertigoRenderSystem(Camera camera, ILayoutSystem layoutSystem) {
            this.camera = camera;
            this.ctx = new VertigoContext();
            this.layoutSystem = layoutSystem;
            this.views = new LightList<UIView>();
            this.commandBuffer = new CommandBuffer(); // todo -- per view
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
            public Vector4 borderRadius;
            public Vector4 borderSize;

            // todo -- border style

            public Color32 textOutlineColor;
            public Color32 textGlowColor;
            public Vector4 clipRect;
            public Visibility visibility;
            public ISVGXPaintable painter;
            public bool isSelfPainting;
            public bool hasUniformBorder;
            public bool requiresRendering;
            public RenderMethod renderMethod;
            public int geometryId;
            public Texture backgroundImage;
            public VertigoMaterial material;

        }

        public enum RenderMethod {

            None,
            Text,
            Painter,
            SelfPainter,
            UniformBorder,
            UniformBorderRadius,
            MixedBorder,
            MixedBorderRadius,
            NoBorderFilled

        }

        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void OnReset() { }

        public void OnUpdate() {
            ctx.Clear();

            for (int i = 0; i < views.Count; i++) {
                RenderView(views[i]);
            }

            ctx.Flush(camera, commandBuffer);
        }

        private VertigoMaterial fillMaterial;
        private GeometryCache geometryCache;

        private void RenderView(UIView view) {
            UIElement[] visibleElements = view.visibleElements.Array;
            int count = view.visibleElements.Count;

            if (count == 0) return;
            
            ctx.SetShader(ShaderSlot.FillSDF, "Vertigo/VertigoSDF");
//            ctx.SetShader(ShaderSlot.FillPath, "Vertigo/VertigoDefault");
//            ctx.SetShader(ShaderSlot.StrokePath, "Vertigo/VertigoStrokePath");
//            ctx.fillMaterial.SetMainTexture();
//            ctx.fillMaterial = fillMaterial;
            
            for (int i = 0; i < count; i++) {
                UIElement element = visibleElements[i];
                renderInfos.TryGetValue(element.id, out RenderInfo renderInfo);

                if (renderInfo.renderMethod == RenderMethod.None) {
                    continue;
                }

                // todo -- support using elements as masks and optionally also render the element

                float x = element.layoutResult.screenPosition.x;
                float y = element.layoutResult.screenPosition.y;
                float width = element.layoutResult.actualSize.width;
                float height = element.layoutResult.actualSize.height;

                // if must render
                // material.SetTexture(currentTexture);

                // batcher.SetTexture();
                // if material is the same
                // if material is same shader w/ different keywords
                // if all keywords are additive -> a-ok
                // if material

                // ctx.GetSharedMaterial("MaterialName");
                // material.SetStencilState(stencil);
                // material.SetBlendState(blend);
                // material.GetMaterialProperties(MaterialPropertyBlock block);
                // material.GetMaterialInstance();
                // material.SetTexture("texture", texture);

                switch (renderInfo.renderMethod) {
                    case RenderMethod.None:
                        break;

                    case RenderMethod.Text:
                        TextInfo textInfo = ((UITextElement) element).textInfo;
                        fillMaterial.SetTextureProperty(ShaderKey.FontAtlas, textInfo.spanList[0].textStyle.font.atlas);
                        break;

                    case RenderMethod.Painter:
                        ctx.SaveState();
                        // renderInfo.painter.Paint(element, ctx, SVGXMatrix.identity);
                        ctx.RestoreState();
                        break;

                    case RenderMethod.NoBorderFilled:
                        
                        ctx.SetFillColor(renderInfo.backgroundColor);
                        ctx.SetFillMaterial(fillMaterial);
//                        RangeInt range = ctx.FillRect(x, y, width, height);
//                        ctx.SetTextureCoord2(range, new Vector4(clipRect));
                        break;

                    // need new geometry when layout changes
                    // need new geometry when values stored in geometry change

                    case RenderMethod.UniformBorder:
                        if (renderInfo.backgroundImage != null) {

                            fillMaterial.SetTextureProperty(ShaderKey.MainTexture, renderInfo.backgroundImage);
                            ctx.SetMainTexture(renderInfo.backgroundImage);
                            ctx.SetFillColor(Color.red);
                            ctx.EnableUVTilingOffset(renderInfo.uvTiling, renderInfo.uvOffset);
                            ctx.SetColorSpace(ColorSpace.Gamma);
                            //ctx.FillRect(x, y, width, height, TextureCoordChannel.TextureCoord0 | TextureCoordChannel.Color);
                            // ctx.SetTexCoord2(new Vector4(1, 1, 1, 1)); // sets for last draw call only
                            
                            ctx.SetStrokeWidth(renderInfo.borderSize.x);
                            ctx.StrokeRect(x, y, width, height);
                            
                            // VertigoMaterial.GetPooledMaterial("Vertigo/Default");

                            ShapeGenerator shapeGenerator = new ShapeGenerator();
                            GeometryGenerator geometryGenerator = new GeometryGenerator();

                            geometryGenerator.SetFillColor(renderInfo.backgroundColor);
                            //geometryGenerator.SetUVChannel(VertexChannel.TextureCoord0, renderInfo.uvRect, renderInfo.uvTiling, renderInfo.uvOffset);
                            //int idx = geometryGenerator.FillRectSDF(geometryCache, x, y, width, height, TextureCoordChannel.TextureCoord0 | TextureCoordChannel.Color | TextureCoordChannel.Normal);
                            
                            geometryGenerator.Fill(shapeGenerator, new RangeInt(0, 1), ShapeMode.SDF, geometryCache);
                            
                            geometryGenerator.SetFillColor(Color.red);
                            
                            geometryGenerator.SetUVTiling(TextureCoordChannel.TextureCoord0, 1, 1);
                            geometryGenerator.SetUVOffset(TextureCoordChannel.TextureCoord1, 1, 1);
                         //   geometryGenerator.SetDefaultChannels(TextureCoordChannel.Color);
                            
                          //  geometryGenerator.FillRectSDF(geometryCache, x, y, width, height, TextureCoordChannel.Color | TextureCoordChannel.Normal | TextureCoordChannel.TextureCoord0);
                            // how do i handle pooling materials
                            // accept user materials
                            // respect the moment they drew something
                            // either ignore changes
                            // or clone material 
                            
//                            if (renderInfo.hasBackgroundTransform) {
//                                fillMaterial.EnableKeyword("BackgroundTransform");
//                                fillMaterial.SetFloatProperty(ShaderKey.BackgroundRotation, renderInfo.backgroundRotation);
//                                fillMaterial.SetFloatProperty(ShaderKey.BackgroundTiling, renderInfo.backgroundRotation);
//                                fillMaterial.SetFloatProperty(ShaderKey.BackgroundOffset, renderInfo.backgroundRotation);
                            // tiling & offset is a vector4, can probably do this cpu side when we need to for standard shapes (ie rects)
//                            }


                            // if shader is the same
                            ctx.Draw(geometryCache, fillMaterial);

//                            ctx.GetMaterial(unitymaterial | materialName | shader);
//                            ctx.GetSharedMaterial(unitymaterial | materialName | shader, keyword[]); //looks in pool for given material, creates if not there, mark shared
//                            // shared materials are not cloned when passed into draw methods
//                            ctx.GetMaterialInstance(unitymaterial | materialName | shader); // instance materials are cloned when passed to draw methods
//                            
//                            fillMaterial.SetStencil(1, 0, 1, CompareFunction.Always);
//                            fillMaterial.EnableKeyword(keyword);
//                            fillMaterial.SetMainTexture(null);
//
//                            ctx.Draw(geometryCache, 0, fillMaterial);
//                            ctx.Draw(geometryCache, 0, shineMaterial);
//
//                            if (material.isShared) {
////                                use material
//                            }
//                            
//                            fillMaterial.Reset();
//                            fillMaterial.SetShader("shaderName");
//                            fillMaterial.EnableKeyword("");
//                            fillMaterial.EnableOptionalKeyword("");
//                            fillMaterial.SetMainTexture(null);
//
//                            ctx.SetMaterial(material);
//                            ctx.SetStrokeMaterial(material);
//                            fillMaterial.SetMainTexture(null);
//
//                           // geometryGenerator.ComputeTextureCoord0(new Vector2(), new Vector2());
//                            
//                            ctx.Draw(cache, idx, fillMaterial);
//                            
//                           // cache.Clear();
//                            
//                            ctx.SetShader();
//                            ctx.EnableKeyword();
//                            ctx.SetKeywords();
//                            ctx.SetMainTexture();
//                            ctx.SetFloatProperty();
//
//                            ctx.ClearMaterial();

                            ctx.FillRect(100, 100, 200, 200);

                            //ctx.SetUVRect();
//                            ctx.EnableUVTiling();
//                            ctx.DisableUVTiling();

                            geometryCache.GetTextureCoord2();
                        }

                        ctx.FillRect(x, y, width, height, fillMaterial);
                        ctx.SetStrokeWidth(renderInfo.borderSize.x);
                        ctx.StrokeRect(x, y, width, height);
                        break;

                    case RenderMethod.UniformBorderRadius:
                        ctx.FillRoundedRect(0, 0, 100, 100, 0, 0, 0, 0);
                        break;

                    case RenderMethod.MixedBorder:
                        break;

                    case RenderMethod.MixedBorderRadius:
                        ctx.FillRoundedRect(0, 0, 100, 100, 0, 0, 0, 0);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private bool CanUseSharedMaterial(in RenderInfo info) {
            // if info.material != default return false;

            if (info.renderMethod == RenderMethod.Painter) {
                return false;
            }

            if (info.backgroundImage != null) {
                if (info.backgroundRotation != 0 || info.backgroundScale.x != 1 || info.backgroundScale.y != 1) {
                    return false;
                }
            }

            if (info.uvOffset.x != 0 || info.uvOffset.y != 0) {
                return false;
            }

            return true;
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            // todo -- each view can take its own camera
            // each view has its own camera origin
            // each view can attach to a different camera event
        }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) {
            RenderInfo renderInfo = new RenderInfo();
            UIStyleSet style = element.style;
            renderInfo.backgroundColor = style.BackgroundColor;
            renderInfo.backgroundRotation = style.BackgroundImageRotation.value; // todo -- resolve this to a float
            renderInfo.opacity = style.Opacity;
            // todo resolve to float
            renderInfo.backgroundScale = new Vector2(style.BackgroundImageScaleX.value, style.BackgroundImageScaleY.value);
            renderInfo.visibility = style.Visibility;
            renderInfo.uvRect = new Rect(0, 0, 1, 1);
            renderInfo.uvOffset = new Vector2(0, 0);
            renderInfo.uvTiling = new Vector2(1, 1);
            renderInfo.backgroundTint = new Color32(255, 255, 255, 255);
//            renderInfo.borderRadius = new Vector4(element.layoutResult.style.BorderRadiusTopRight, style.BorderRadiusTopLeft, style.BorderRadiusBottomLeft, style.BorderRadiusBottomRight);
            renderInfo.clipRect = Vector4.zero;
            renderInfo.borderSize = element.layoutResult.border;
            renderInfo.textGlowColor = style.TextGlowColor;
            renderInfo.textColor = style.TextColor;
            renderInfo.borderColorTop = Color.black;
            renderInfo.borderColorRight = Color.black;
            renderInfo.borderColorBottom = Color.black;
            renderInfo.borderColorLeft = Color.black;
            renderInfo.requiresRendering = true;
            renderInfos.geometryId = -1;
            renderInfos[element.id] = renderInfo;
        }

        public readonly Action PaintElement;

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        private void OnStylePropertiesWillChange() { }

        private VertigoMaterial sharedMaterial;

        private void OnStylePropertiesDidChanged(UIElement element) {
            RenderInfo info = renderInfos[element.id];

            // recompute render info here
            bool canUseDefaultShared = CanUseSharedMaterial(info);
            if (!canUseDefaultShared) {
                // see if we can use a non shared one w/o creating a new one
                // if not get new material from the pool & set properties on it as needed
            }

            if (info.material != sharedMaterial && CanUseSharedMaterial(info)) {
                // materialPool.Release(info.material);
                info.material = sharedMaterial;
            }
        }

        private void OnStylePropertyChanged() {
            // recompute render type 
            // maybe update geometry
        }

        public void SetCamera(Camera camera) {
            this.camera = camera; // todo -- should be handled by the view
        }

    }

}