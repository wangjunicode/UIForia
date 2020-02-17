using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/DocumentationElements/NumberStepper.xml")]
    public class NumberStepper : BaseInputElement {

        public int min = Int32.MinValue;

        public int max = Int32.MaxValue;

        public int step = 1;

        public int value;

        public string placeholder;

        private UIInputElement inputElement;
        private float keyLockTimestamp;
        private float holdDebounce = 0.05f;
        private float timestamp;

        public override void OnEnable() {
            inputElement = FindById<UIInputElement>("input-element");
        }

        public void OnKeyPressed(KeyboardInputEvent evt) {
            
            if (inputElement.HasDisabledAttr()) {
                return;
            }
            
            evt.StopPropagation();
            
            switch (evt.keyCode) {
                case KeyCode.UpArrow: Increment();
                    break;
                case KeyCode.DownArrow: Decrement(); 
                    break;
                default: return;
            }
            
            keyLockTimestamp = Time.unscaledTime;
        }

        public void OnKeyHeldDownWithFocus(KeyboardInputEvent evt) {
            if (inputElement.HasDisabledAttr()) {
                return;
            }
            
            evt.StopPropagation();
            
            switch (evt.keyCode) {
                case KeyCode.UpArrow: IncrementContinuously();
                    break;
                case KeyCode.DownArrow: DecrementContinuously();
                    break;
                default: return;
            }
        }

        public void IncrementContinuously(MouseInputEvent evt) {
            evt.StopPropagation();
            IncrementContinuously();
        }

        public void DecrementContinuously(MouseInputEvent evt) {
            evt.StopPropagation();
            DecrementContinuously();
        }

        public void Increment(MouseInputEvent evt) {
            evt.StopPropagation();
            keyLockTimestamp = Time.unscaledTime;
            if (inputElement.HasDisabledAttr()) {
                return;
            }

            Increment();
        }

        public void Decrement(MouseInputEvent evt) {
            evt.StopPropagation();
            keyLockTimestamp = Time.unscaledTime;
            if (inputElement.HasDisabledAttr()) {
                return;
            }

            Decrement();
        }

        private void IncrementContinuously() {
            if (!CanTriggerHeldKey()) {
                return;
            }

            Increment();
            timestamp = Time.unscaledTime;
        }

        private void DecrementContinuously() {
            if (!CanTriggerHeldKey()) {
                return;
            }

            Decrement();
            timestamp = Time.unscaledTime;
        }

        private void Increment() {
            value += step;

            if (value < min) {
                value = min;
            } else if (value > max) {
                value = max;
            }
        }

        private void Decrement() {
            value -= step;

            if (value < min) {
                value = min;
            } else if (value > max) {
                value = max;
            }
        }

        public void InputValueChanged() {
            if (value < min) {
                value = min;
            } else if (value > max) {
                value = max;
            }
        }

        private bool CanTriggerHeldKey() {
            if (GetAttribute("disabled") != null) return false;

            if (Time.unscaledTime - keyLockTimestamp < 0.5f) {
                return false;
            }

            if (Time.unscaledTime - timestamp < holdDebounce) {
                return false;
            }

            return true;
        }
    }
}
