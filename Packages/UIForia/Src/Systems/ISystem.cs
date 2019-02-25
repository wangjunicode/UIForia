﻿namespace UIForia.Rendering {

    public interface ISystem {

        void OnReset();
        void OnUpdate();
        void OnDestroy();

        void OnViewAdded(UIView view);
        void OnViewRemoved(UIView view);
        void OnElementEnabled(UIElement element);
        void OnElementDisabled(UIElement element);
        void OnElementDestroyed(UIElement element);

        void OnAttributeSet(UIElement element, string attributeName, string previousValue);

        void OnElementCreated(UIElement element);

    }

}