using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public interface ILayoutSystem : ISystem {

        IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn);

        LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null);

        AwesomeLayoutRunner GetLayoutRunner(UIElement viewRoot);

    }

}