using System;
using UIForia.Elements;
using UIForia.Selectors;

namespace UIForia {

    public struct VertigoSelectorQuery {

        public FromTarget targetGroup;
        public Action<UIElement> filter;

    }

}