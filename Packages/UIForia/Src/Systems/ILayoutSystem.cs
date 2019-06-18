using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public interface ILayoutSystem : ISystem {

        IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn);

        OffsetRect GetPaddingRect(UIElement element);
        OffsetRect GetMarginRect(UIElement element);
        OffsetRect GetBorderRect(UIElement element);

        LayoutBox GetBoxForElement(UIElement itemElement);

        LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null);

    }

}