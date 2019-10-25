using System;
using UIForia.Attributes;

namespace UIForia.Elements {
    public abstract class BaseInputElement : UIElement {

        [WriteBinding(nameof(disabled))]
        public event Action<bool> onDisabledChanged;
        
        public bool disabled;

        public override void OnCreate() {
            if (disabled) {
                SetAttribute("disabled", "true");
            }
        }

        [OnPropertyChanged(nameof(disabled))]
        protected void OnDisabledChanged(string name) {
            if (disabled) {
                SetAttribute("disabled", "true");
            }
            else {
                SetAttribute("disabled", null);
            }
        }

    }
}
