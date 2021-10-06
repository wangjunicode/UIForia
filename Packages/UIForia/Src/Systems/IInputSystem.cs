using System.Collections.Generic;
using UIForia.Elements;
using UIForia.UIInput;

namespace UIForia.Systems {

    public interface IInputSystem : ISystem, IInputProvider {

        void RegisterKeyboardHandler(UIElement element);
        
        /// <summary>
        /// Can contain destroyed elements (released to pool). Check "isDestroyed" flag before processing the element. 
        /// </summary>
        IReadOnlyList<UIElement> AllElementsThisFrame { get; }
        
#if UNITY_EDITOR
        List<UIElement> DebugElementsThisFrame { get; }
        bool DebugMouseUpThisFrame { get; }
#endif
    }

}