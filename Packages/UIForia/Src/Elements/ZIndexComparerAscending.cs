using System.Collections.Generic;

namespace UIForia.Elements {

    public class ZIndexComparerAscending : IComparer<UIElement> {

        public int Compare(UIElement a, UIElement b) {
            
            if (a.View.id == b.View.id) {
                return a.layoutResult.zIndex < b.layoutResult.zIndex ? -1 : 1;
            }

            return a.View.Depth > b.View.Depth ? -1 : 1;
        }

    }

}
