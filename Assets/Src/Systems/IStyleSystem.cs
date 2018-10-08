using System;
using Rendering;

namespace Src.Systems {

    public interface IStyleSystem {

        event Action<UIElement, string> onTextContentChanged;
        event Action<UIElement, StyleProperty> onStylePropertyChanged;

        void SetStyleProperty(UIElement element, StyleProperty propertyValue);

    }

}