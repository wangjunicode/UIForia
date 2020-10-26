using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Button/Button.xml")]
    public class Button : UIElement {

        private string _label;
        private string _iconSource;

        public override void OnEnable() {
            string variant = GetAttribute("variant");
            switch (variant) {
                case "primary":
                    break;
                case "secondary":
                    break;
                default:
                    SetAttribute("variant", "primary");
                    variant = "primary";
                    break;
            }

            UIElement text = FindById("text");
            UIElement img = FindById("img");
            text.SetAttribute("variant", variant);
            img.SetAttribute("variant", variant);

            TryGetAttribute("size-variant", out string sizeVariant);
            text.SetAttribute("size-variant", sizeVariant);
            img.SetAttribute("size-variant", sizeVariant);
        }

        private void AdjustSpacing() {
            UITextElement text = FindById<UITextElement>("text");
            UIElement image = FindById("img");

            bool hasLabel = !string.IsNullOrEmpty(_label);
            bool hasIcon = !string.IsNullOrEmpty(_iconSource);

            text.SetEnabled(hasLabel);
            image.SetEnabled(hasIcon);
            
            if (hasLabel && hasIcon) {
                text.SetEnabled(true);
                image.SetEnabled(true);
                SetAttribute("configuration", "label+icon");
            }
            else if (hasIcon) {
                text.SetEnabled(false);
                image.SetEnabled(true);
                SetAttribute("configuration", "icon-only");
                image.SetAttribute("configuration", "icon-only");
            }
            else if (hasLabel) {
                SetAttribute("configuration", "label-only");
            }
            else {
                SetAttribute("configuration", null);
            }
            
        }

        public string label {
            get => _label;
            set {
                if (_label == value) {
                    return;
                }

                _label = value;
                UITextElement text = FindById<UITextElement>("text");
                text.SetText(_label);
                AdjustSpacing();

            }
        }

        public string icon {
            get => _iconSource;
            set {
                if (_iconSource == value) {
                    return;
                }

                _iconSource = value;
                AdjustSpacing();

            }
        }

        protected override void OnSetAttribute(string attrName, string newValue, string oldValue) {
            if (attrName == "variant") {
                FindById("text").SetAttribute("variant", newValue);
                FindById("img").SetAttribute("variant", newValue);
            }
        }

    }

}