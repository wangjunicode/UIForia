using System;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public interface IRenderSystem : ISystem {

        event Action<LightList<RenderData>, LightList<RenderData>, Vector3, Camera> DrawDebugOverlay ;
        
    }

}