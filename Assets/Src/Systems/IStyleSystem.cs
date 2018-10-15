using System;
using Rendering;
using Src.Animation;

namespace Src.Systems {

    public interface IStyleSystem {

        event Action<UIElement, string> onTextContentChanged;
        event Action<UIElement, StyleProperty> onStylePropertyChanged;

        void SetStyleProperty(UIElement element, StyleProperty propertyValue);

        void PlayAnimation(UIStyleSet styleSet, StyleAnimation animation, AnimationOptions options);

    }

}