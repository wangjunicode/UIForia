using System;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public class OnTextInputAttribute : Attribute {

        public readonly EventPhase phase;

        public OnTextInputAttribute() : this(default) { }

        public OnTextInputAttribute(EventPhase phase) {
            this.phase = phase;
        }

    }
}