using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Checkbox/Checkbox.xml")]
    public class Checkbox : UIElement {

        private bool _isChecked;
        private bool _isMixed;

        public bool isChecked {
            get => _isChecked;
            set {
                if (value == _isChecked) {
                    return;
                }

                _isChecked = value;
                if (_isChecked) {
                    SetAttribute("checked", _isMixed ? "mixed" : "checked");
                }
                else {
                    SetAttribute("checked", null);
                }
            }
        }

        public bool isMixed {
            get => _isMixed;
            set {
                if (value == _isMixed) {
                    return;
                }

                _isMixed = value;
                if (_isChecked) {
                    SetAttribute("checked", _isMixed ? "mixed" : "checked");
                }

            }
        }

        [OnMouseClick]
        public void OnClick() {
            isChecked = !_isChecked;
        }

    }

}