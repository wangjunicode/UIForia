using System;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Systems {

    public class RenderBoxPool {

        private ResourceManager resourceManager;

        public RenderBoxPool(ResourceManager resourceManager) {
            this.resourceManager = resourceManager;
        }

        // todo -- this doesn't actually pool right now
        public RenderBox GetCustomPainter(string painterId) {

            if (painterId == "self") {
                return new SelfPaintedRenderBox();
            }

            if (Application.s_CustomPainters.TryGetValue(painterId, out Type boxType)) {
                return (RenderBox) Activator.CreateInstance(boxType);
            }

            if (resourceManager.TryGetStylePainter(painterId, out StylePainterDefinition painter)) {

                StylePainterRenderBox stylePainterRenderBox = new StylePainterRenderBox();
                stylePainterRenderBox.painterDefinition = painter;
                return stylePainterRenderBox;

            }

            return null;
        }

    }

    public class RenderSystem : IRenderSystem {

        private Camera camera;
        private CommandBuffer commandBuffer;
        private RenderContext renderContext;
        internal LightList<RenderOwner> renderOwners;

        private ElementSystem elementSystem;

        public RenderSystem(Camera camera, Application application, ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.camera = camera;
            this.commandBuffer = new CommandBuffer(); // todo -- per view
            this.commandBuffer.name = "UIForia Main Command Buffer";
            this.renderContext = new RenderContext(application.settings);
            this.renderOwners = new LightList<RenderOwner>();

            if (this.camera != null) {
                this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }

            application.onViewsSorted += uiViews => { renderOwners.Sort((o1, o2) => o1.view.Depth.CompareTo(o2.view.Depth)); };
        }

        private void ReplaceRenderBox(UIElement element, string painterId) {
            renderOwners[0].CreateRenderBox(element);
        }

        public event Action<RenderContext> DrawDebugOverlay2;

        public void OnReset() {
            commandBuffer.Clear();
            for (int i = 0; i < renderOwners.size; i++) {
                renderOwners[i].Destroy();
            }

            renderOwners.QuickClear();
            renderContext.clipContext.Destroy();
            renderContext.clipContext = new ClipContext(Application.Settings);
        }

        public virtual void OnUpdate() {
            renderContext.Clear();
            // todo
            // views can have their own cameras.
            // if they do they are not batchable with other views. 
            // for now we can make batching not cross view boundaries, eventually that would be cool though

            camera.orthographicSize = Screen.height * 0.5f;
            for (int i = 0; i < renderOwners.size; i++) {
                renderOwners.array[i].Render(renderContext);
            }

            DrawDebugOverlay2?.Invoke(renderContext);
            renderContext.Render(camera, commandBuffer);
        }

        public void OnDestroy() {
            commandBuffer.Clear();
            renderContext.clipContext.Destroy();
            renderContext.Destroy();
        }

        public void OnViewAdded(UIView view) {
            renderOwners.Add(new RenderOwner(view, elementSystem));
        }

        public void OnViewRemoved(UIView view) {
            for (int i = 0; i < renderOwners.size; i++) {
                if (renderOwners.array[i].view == view) {
                    renderOwners.RemoveAt(i);
                    return;
                }
            }
        }

        public void SetCamera(Camera camera) {
            if (this.camera != null) {
                this.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }

            this.camera = camera; // todo -- should be handled by the view

            if (this.camera != null) {
                this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            }
        }

        public void HandleStylePropertyUpdates(UIElement element, StyleProperty[] propertyList, int propertyCount) {
            if (element.renderBox == null) return;

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.Painter:
                        ReplaceRenderBox(element, property.AsString);
                        break;
                }
            }

            element.renderBox.OnStylePropertyChanged(propertyList, propertyCount);
        }

    }

}