using System;
using SVGX;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Rendering {

    public interface IRenderSystem : ISystem {

        event Action<ImmediateRenderContext> DrawDebugOverlay ;
        event Action<RenderContext> DrawDebugOverlay2;

        void SetCamera(Camera camera);

    }

}