using System;
using UnityEngine;

namespace Src.Input {

    public abstract class DragEvent {

        public readonly Type type;
        internal EventPropagator source;
        
        public event Action<DragEvent> onUpdate;
        
        protected DragEvent() {
            this.type = GetType();
        }
        
        public Vector2 MousePosition { get; internal set; }
        public Vector2 DragStartPosition { get; internal set; }
        public KeyboardModifiers Modifiers { get; internal set; }
        public InputEventType CurrentEventType { get; internal set; }
        
        public float StartTime { get; internal set; }
        public bool IsCanceled { get; private set; }
        public bool IsDropped { get; }

        internal void Update() {
            onUpdate?.Invoke(this);
        }
        
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

        public void Drop(bool success) {
            onUpdate = null;
        }

        public void Cancel() {
            IsCanceled = true;
            onUpdate = null;
        }
        
    }

}