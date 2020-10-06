using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace SeedLib {

    [Template("SeedLib/RadioButton/LabeledRadioButton.xml")]
    public class LabeledRadioButton : UIElement {

        public bool isChecked;
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