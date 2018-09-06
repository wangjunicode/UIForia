using System;

namespace Src.Input {

    public class FocusEvent : InputEvent {

        public FocusEvent() : base(InputEventType.Focus) { }

    }
    
    public class BlurEvent : InputEvent {

        public BlurEvent() : base(InputEventType.Blur) { }

    }

    public interface IFocusable {

        event Action<FocusEvent> onFocus;
        event Action<BlurEvent> onBlur;
        
        bool HasFocus { get; }
        bool HasFocusLocked { get; }

    }

}