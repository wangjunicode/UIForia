using Src;
using UnityEngine;

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
        void SetViewportRect(Rect viewport);
        void OnElementStyleChanged(UIElement element);

    }

}