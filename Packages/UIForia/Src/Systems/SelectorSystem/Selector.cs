using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Selectors {

    public struct SelectorEffect {

        public int selectorId;
        public ElementReference elementReference;

    }

    public struct SelectorUsage {

        public int id;
        public Selector selector;
        public LightList<StyleSet2> resultSet;

    }

    public class Selector {

        public int id;
        public SelectorQuery rootQuery;
        public UIStyle matchStyle;
        public StyleState state;
        public LightList<StyleSet2> resultSet;

        public void Run(UIElement origin) {
            LightList<UIElement> result = LightList<UIElement>.Get();

            rootQuery.Gather(origin, result);

            if (result.size == 0) {
                result.Release();
                return;
            }

            if (rootQuery.next == null) {
                // match!    
                for (int i = 0; i < result.size; i++) {
                    result.array[i].style.SetSelectorStyle(matchStyle);
                }

                result.Release();
                return;
            }

            for (int i = 0; i < result.size; i++) {
                if (rootQuery.next.Run(origin, result.array[i])) {
                    result.array[i].style.SetSelectorStyle(matchStyle);
                }
            }

            result.Release();
        }

    }

}