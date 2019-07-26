using System;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
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
        public OffsetRect borderSize;

        // todo -- border style

        public Color32 textOutlineColor;
        public Color32 textGlowColor;
        public Vector4 clipRect;
        public Visibility visibility;
        public ISVGXPaintable painter;
        public ISVGXElementPainter selfPainter;
        public RenderMethod renderMethod;
        public Texture backgroundImage;
        public VertigoMaterial material;
        public bool isText;

    }
    

    public class RenderBoxPool {
        
        // todo -- this doesn't actually pool right now
        public RenderBox GetCustomPainter(string painterId) {
            
            if (UIForia.Application.s_CustomPainters.TryGetValue(painterId, out Type boxType)) {
                return (RenderBox) Activator.CreateInstance(boxType);
            }

            return null;
            
        }

    }

    public class VertigoRenderSystem : IRenderSystem {

        private Camera camera;
        private CommandBuffer commandBuffer;
        private RenderContext renderContext;
        private LightList<RenderOwner> renderOwners;

        public VertigoRenderSystem(Camera camera, UIForia.Application application) {
            this.camera = camera;
            this.commandBuffer = new CommandBuffer(); // todo -- per view
            this.renderContext = new RenderContext(application.settings.batchedMaterial);
            this.renderOwners = new LightList<RenderOwner>();
            this.camera?.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            application.StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> propertyList) {
            int count = propertyList.size;
            StyleProperty[] properties = propertyList.array;

            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.Painter:
                        ReplaceRenderBox(element, property.AsString);
                        break;
                }
            }

            element.renderBox.OnStylePropertyChanged(propertyList);
        }

        private void ReplaceRenderBox(UIElement element, string painterId) {
            throw new NotImplementedException();
        }

        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void OnReset() { }

        public void OnUpdate() {

            renderContext.Clear();
            
            // todo
            // views can have their own cameras.
            // if they do they are not batchable with other views. 
            // for now we can make batching not cross view boundaries, eventually that would be cool though

            camera.orthographicSize = Screen.height * 0.5f;

            for (int i = 0; i < renderOwners.size; i++) {
                renderOwners.array[i].GatherBoxData();
                renderOwners.array[i].BuildClipGroups();
                renderOwners.array[i].Render(renderContext);
            }
            
            renderContext.Render(camera, commandBuffer);
            
        }
        
        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            renderOwners.Add(new RenderOwner(view, camera));
        }

        public void OnViewRemoved(UIView view) {
            for (int i = 0; i < renderOwners.size; i++) {
                if (renderOwners.array[i].view == view) {
                    renderOwners.RemoveAt(i);
                    return;
                }
            }
        }

        public void OnElementEnabled(UIElement element) {

        }

        public void OnElementDisabled(UIElement element) {
            
        }

        public void OnElementDestroyed(UIElement element) {
            // re-pool the render boxes for the hierarchy
            UIView view = element.View;
            for (int i = 0; i < renderOwners.size; i++) {
                if (renderOwners.array[i].view == view) {
                    renderOwners.array[i].OnElementDestroyed(element);
                    return;
                }
            }
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