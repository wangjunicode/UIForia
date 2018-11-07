using System;
using UnityEngine;

namespace UIForia.Input {

    public abstract class DragEvent {

        public readonly Type type;
        internal EventPropagator source;
        public readonly UIElement origin;
        
        protected DragEvent(UIElement origin) {
            this.type = GetType();
            this.origin = origin;
        }

        public Vector2 MousePosition { get; internal set; }
        public Vector2 DragStartPosition { get; internal set; }
        public KeyboardModifiers Modifiers { get; internal set; }
        public InputEventType CurrentEventType { get; internal set; }

        public float StartTime { get; internal set; }
        public bool IsCanceled { get; protected set; }
        public bool IsDropped { get; }

        public virtual void Update() { }

        public bool IsConsumed => source.isConsumed;

        public void StopPropagation() {
            if (source != null) {
                source.shouldStopPropagation = true;
            }
        }

        public void Consume() {
            if (source != null) {
                source.isConsumed = true;
            }
        }

        public virtual void Drop(bool success) { }

        public virtual void Cancel() { }

    }

    public class CallbackDragEvent : DragEvent {

        public event Action<DragEvent> onUpdate;
        
        public CallbackDragEvent(UIElement origin) : base(origin) { }

        public override void Update() {
            onUpdate?.Invoke(this);
        }

        public override void Drop(bool success) {
            onUpdate = null;
        }

        public override void Cancel() {
            IsCanceled = true;
            onUpdate = null;
        }


    }

}