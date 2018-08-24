using Src;
using UnityEngine;

namespace Rendering {

    public interface IRenderSystem : ISystem {

        void OnRender();
        void SetViewportRect(Rect viewport);

    }

}