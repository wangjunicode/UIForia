using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Selectors {

    public abstract class SelectorIndex {

        public int size;

        public abstract void Gather(UIElement origin, int templateId, LightList<UIElement> resultSet);

        public abstract void Filter(UIElement origin, int templateId, LightList<UIElement> resultSet);

    }

}