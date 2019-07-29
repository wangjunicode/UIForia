using System;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Application = UIForia.Application;

namespace Src.Systems {

    public class RenderBoxPool {

        // todo -- this doesn't actually pool right now
        public RenderBox GetCustomPainter(string painterId) {
            if (Application.s_CustomPainters.TryGetValue(painterId, out Type boxType)) {
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

        public VertigoRenderSystem(Camera camera, Application application) {
            this.camera = camera;
            this.commandBuffer = new CommandBuffer(); // todo -- per view
            this.commandBuffer.name = "UIForia Main Command Buffer";
            this.renderContext = new RenderContext(application.settings.batchedMaterial);
            this.renderOwners = new LightList<RenderOwner>();

            if (this.camera != null) {
                this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }

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

        public void OnReset() {
            commandBuffer.Clear();
            renderOwners.QuickClear();
        }

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

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

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
            if (this.camera != null) {
                this.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }

            this.camera = camera; // todo -- should be handled by the view
            
            if (this.camera != null) {
                this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }
        }

    }

}