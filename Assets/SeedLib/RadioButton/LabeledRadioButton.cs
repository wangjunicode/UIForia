using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace SeedLib {

    [Template("SeedLib/RadioButton/LabeledRadioButton.xml")]
    public class LabeledRadioButton : UIElement, ISelectable {

        public bool isChecked;
        public string _label;

        private UITextElement textElement;
        // set, when part of a group.
        private RadioButtonGroup buttonGroup;

        public override void OnEnable() {
            buttonGroup = FindParent<RadioButtonGroup>();
        }

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {
            isChecked = !isChecked;

            if (buttonGroup == null) {
                evt.StopPropagation();    
            }
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

        public override void OnUpdate() {
            if (buttonGroup == null) {
                return;
            }
            
            isChecked = buttonGroup.IsSelected(siblingIndex);
        }

    }

}