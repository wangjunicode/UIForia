using Src;

namespace Src.Rendering {

    public interface ISystem {

        void OnReset();
        void OnUpdate();
        void OnDestroy();
        void OnReady();
        void OnInitialize();
        
        void OnElementEnabled(UIElement element);
        void OnElementDisabled(UIElement element);
        void OnElementDestroyed(UIElement element);

        void OnElementCreatedFromTemplate(UIElement element);

    }

}