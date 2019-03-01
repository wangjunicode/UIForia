using System;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public interface IRenderSystem : ISystem {

        event Action<LightList<RenderData>, LightList<RenderData>, Vector3, Camera> DrawDebugOverlay ;

        RenderData GetRenderData(UIElement element);

        void SetCamera(Camera camera);

    }

}