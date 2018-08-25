using Rendering;
using UnityEngine;

namespace Src.Systems {

    public interface ILayoutSystem : ISystem {

        int RectCount { get; }
        LayoutResult[] LayoutResults { get; }
        void SetViewportRect(Rect viewportRect);

        int QueryPoint(Vector2 point, ref LayoutResult[] output);
        Rect GetRectForElement(int elementId);

    }

}