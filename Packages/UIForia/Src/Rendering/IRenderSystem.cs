using System;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Rendering {

    public interface IRenderSystem {

        event Action<RenderContext> DrawDebugOverlay2;

        void SetCamera(Camera camera);

    }

}