using System;
using Rendering;
using Src.Elements;
using UnityEngine;

namespace Src.Systems {

    public interface ILayoutSystem : ISystem {

        event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        event Action<VirtualScrollbar> onDestroyVirtualScrollbar;

        int RectCount { get; }
        LayoutResult[] LayoutResults { get; }
        void SetViewportRect(Rect viewportRect);

        int QueryPoint(Vector2 point, ref LayoutResult[] output);
        bool GetRectForElement(int elementId, out Rect rect);

    }

}