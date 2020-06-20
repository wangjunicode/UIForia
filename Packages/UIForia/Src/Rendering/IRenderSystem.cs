using System;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Rendering {

    public interface IRenderSystem {

        event Action<RenderContext> DrawDebugOverlay;

        void SetCamera(Camera camera);

    }

}