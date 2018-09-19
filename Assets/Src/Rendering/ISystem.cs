using Src;

namespace Rendering {

    public interface ISystem {

        void OnReset();
        void OnUpdate();
        void OnDestroy();
        void OnReady();
        void OnInitialize();
        
        void OnElementCreated(UIElement element);
        void OnElementMoved(UIElement element, int newIndex, int oldIndex);
        void OnElementEnabled(UIElement element);
        void OnElementDisabled(UIElement element);
        void OnElementDestroyed(UIElement element);
        void OnElementShown(UIElement element);
        void OnElementHidden(UIElement element);

        void OnElementCreatedFromTemplate(MetaData elementData);
        void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent);

    }

}