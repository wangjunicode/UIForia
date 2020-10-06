using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace SeedLib {

    [Template("SeedLib/Checkbox/LabeledCheckbox.xml")]
    public class LabeledCheckbox : UIElement {

        public bool isChecked;
        public bool isMixed;
        public string _label;

        private UITextElement textElement;

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {
            isChecked = !isChecked;
            evt.StopPropagation();
        }
        
        public string label {
            get => _label;
            set {
                if (value == _label) {
                    return;
                }

                _label = value;
                textElement = textElement ?? FindById<UITextElement>("text");
                textElement.SetText(_label);
                
            }
        }

    }

}