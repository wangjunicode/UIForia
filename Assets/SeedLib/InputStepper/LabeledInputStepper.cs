using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {
    [Template("SeedLib/InputStepper/LabeledInputStepper.xml")]
    public class LabeledInputStepper : BaseInputElement {
        // Sync with InputStepper
        public int min;
        public int max;
        public int step;
        public int value;
        
        private string _label;
        private UITextElement textElement;

        public string label {
            get { return _label; }
            set {
                if (value == _label) {
                    return;
                }

                _label = value;
                _label = value;
                textElement = textElement ?? FindById<UITextElement>("text");
                textElement.SetText(_label);
            }
        }
    }
}