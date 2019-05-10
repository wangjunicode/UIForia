using System;
using SVGX;
using UIForia;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
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
            public Painter painter;
            public bool isSelfPainting;
            public bool hasUniformBorder;
            public bool requiresRendering;
            public RenderMethod renderMethod;

        }

        public enum RenderMethod {

            None,
            Text,
            Painter,
            SelfPainter,
            UniformBorder,
            UniformBorderRadius,
            MixedBorder,
            MixedBorderRadius

        }
        
        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void OnReset() {
            
        }

        public void OnUpdate() {
            ctx.Clear();
            
            for (int i = 0; i < views.Count; i++) {
                RenderView(views[i]);
            }
            
            ctx.Flush(camera, commandBuffer);
        }

        private GeometryCache geometryCache;
        
        private void RenderView(UIView view) {
            UIElement[] visibleElements = view.visibleElements.Array;
            int count = view.visibleElements.Count;
            
            if(count == 0) return;

            for (int i = 0; i < count; i++) {
                UIElement element = visibleElements[i];
                renderInfos.TryGetValue(element.id, out RenderInfo renderInfo);
                switch (renderInfo.renderMethod) {
                    case RenderMethod.None:
                        break;
                    case RenderMethod.Text:
                        break;
                    case RenderMethod.Painter:
                        break;
                    case RenderMethod.SelfPainter:
                        break;
                    case RenderMethod.UniformBorder:
                        break;
                    case RenderMethod.UniformBorderRadius:
                        break;
                    case RenderMethod.MixedBorder:
                        break;
                    case RenderMethod.MixedBorderRadius:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }                                                
            }

        }

        public void OnDestroy() {
            
        }

        public void OnViewAdded(UIView view) {
            // todo -- each view can take its own camera
            // each view has its own camera origin
            // each view can attach to a different camera event
        }

        public void OnViewRemoved(UIView view) {
        }

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
            renderInfos[element.id] = renderInfo;
        }

        public readonly Action PaintElement;
        
        public void OnElementDisabled(UIElement element) {
            
        }

        public void OnElementDestroyed(UIElement element) {
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
        }

        public void OnElementCreated(UIElement element) {
        }

        private void OnStylePropertiesWillChange() { }

        private void OnStylePropertiesDidChanged() { }

        private void OnStylePropertyChanged() {
            
        }

        public void SetCamera(Camera camera) {
            this.camera = camera; // todo -- should be handled by the view
        }

    }

}