using System;
using UIForia.Elements;

namespace UIForia {

    public struct EventHook {

        public long sourceId;
        public int sourceType;
        public StyleEvent eventType;
        public Action<UIElement> action;

    }

}