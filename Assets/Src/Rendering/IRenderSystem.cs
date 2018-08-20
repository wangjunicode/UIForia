using Src;

namespace Rendering {

    public interface ISystem {

        void OnReset();
        void OnUpdate();
        void OnDestroy();
        void OnElementCreated(UIElementCreationData elementData);
        void OnElementEnabled(UIElement element);
        void OnElementDisabled(UIElement element);
        void OnElementDestroyed(UIElement element);

    }
    
    public interface IRenderSystem : ISystem {

        void OnRender();
        void OnElementStyleChanged(UIElement element);

    }

}