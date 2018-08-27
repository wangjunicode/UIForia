using Src;

namespace Rendering {

    public interface ISystem {

        void OnReset();
        void OnUpdate();
        void OnDestroy();
        void OnReady();
        void OnInitialize();
        
        void OnElementCreated(InitData elementData);
        void OnElementEnabled(UIElement element);
        void OnElementDisabled(UIElement element);
        void OnElementDestroyed(UIElement element);
        void OnElementShown(UIElement element);
        void OnElementHidden(UIElement element);

    }

}