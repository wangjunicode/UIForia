using UIForia.Attributes;

namespace UIForia.Elements {
    public abstract class BaseInputElement : UIElement {

        public bool disabled;
        protected UITextElement textElement;

        public override void OnCreate() {
            if (disabled) {
                SetAttribute("disabled", "true");
            }
        }

        public override void OnEnable() {
            textElement = FindById<UITextElement>("input-element-text");
        }

        [OnPropertyChanged(nameof(disabled))]
        public void OnDisabledChanged() {
            if (disabled) {
                SetAttribute("disabled", "true");
            }
            else {
                SetAttribute("disabled", null);
            }
        }

    }
}
