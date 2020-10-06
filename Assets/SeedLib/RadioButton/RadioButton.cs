using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace SeedLib {

    [Template("SeedLib/RadioButton/RadioButton.xml")]
    public class RadioButton : UIElement {

        private bool _isChecked;
        private UIElement selected;
        
        public bool isChecked {
            get => _isChecked;
            set {
                if (_isChecked == value) {
                    return;
                }

                _isChecked = value;
                selected?.SetEnabled(value);
            }
        }

        public override void OnEnable() {
            selected = selected ?? FindById("selected");
            selected.SetEnabled(_isChecked);
        }

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {
            isChecked = !_isChecked;
            evt.StopPropagation();
        }

    }

}