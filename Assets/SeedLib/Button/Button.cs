using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Button/Button.xml")]
    public class SecondaryButton : UIElement {

        public override void OnEnable() {
            string variant = "Secondary";
            SetAttribute("variant", variant);
            FindById("text").SetAttribute("variant", variant);
            FindById("img").SetAttribute("variant", variant);
        }

        protected override void OnSetAttribute(string attrName, string newValue, string oldValue) {
            if (attrName == "variant" && newValue != "Secondary") {
                string variant = "Secondary";
                SetAttribute("variant", variant);
                FindById("text").SetAttribute("variant", variant);
                FindById("img").SetAttribute("variant", variant);
            }
        }
        
    }

    [Template("SeedLib/Button/Button.xml")]
    public class Button : UIElement {

        private string _label;
        private string _iconSource;

        public override void OnEnable() {
            string variant = GetAttribute("variant");
            switch (variant) {
                case "Primary":
                    break;
                case "Secondary":
                    break;
                default:
                    SetAttribute("variant", "Primary");
                    variant = "Primary";
                    break;
            }
            
            FindById("text").SetAttribute("variant", variant);
            FindById("img").SetAttribute("variant", variant);
            
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