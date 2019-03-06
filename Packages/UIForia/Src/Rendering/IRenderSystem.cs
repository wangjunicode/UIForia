using System;
using SVGX;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public interface IRenderSystem : ISystem {

        event Action<ImmediateRenderContext> DrawDebugOverlay ;

        RenderData GetRenderData(UIElement element);

        void SetCamera(Camera camera);

    }

}