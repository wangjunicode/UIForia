using System.Collections.Generic;
using UIForia.Elements;
using UnityEngine;

namespace UIForia.Systems {

    public interface ILayoutSystem  {

        IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn);

    }

}