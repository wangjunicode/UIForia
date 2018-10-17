using System;
using Rendering;
using Src;
using Src.Systems;

namespace Tests.Mocks {

    public class MockView : UIView {

        
        public MockView(Type elementType, string template = null) : base(elementType, template) {
            layoutSystem = new MockLayoutSystem(styleSystem);
            inputSystem = new MockInputSystem(layoutSystem, styleSystem);
            renderSystem = new MockRenderSystem();
            systems.Add(layoutSystem);
            systems.Add(inputSystem);
        }
        
        public MockInputSystem InputSystem => (MockInputSystem)inputSystem;
        public ILayoutSystem LayoutSystem => layoutSystem;

        
    }

    public class MockRenderSystem : IRenderSystem {

        public void OnReset() {
            
        }

        public void OnUpdate() {
            
        }

        public void OnDestroy() {
            
        }

        public void OnReady() {
            
        }

        public void OnInitialize() {
            
        }

        public void OnElementCreated(UIElement element) {
            
        }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) {
            
        }

        public void OnElementEnabled(UIElement element) {
            
        }

        public void OnElementDisabled(UIElement element) {
            
        }

        public void OnElementDestroyed(UIElement element) {
            
        }

        public void OnElementShown(UIElement element) {
            
        }

        public void OnElementHidden(UIElement element) {
            
        }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            
        }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) {
            
        }

        public void OnRender() {
            
        }

        public void MarkGeometryDirty(IDrawable element) {
            
        }

        public void MarkMaterialDirty(IDrawable element) {
            
        }

    }


}