using System;
using UIForia;
using UIForia.Input;

namespace UI {

    [Template("Klang/Seed/UIForia/KlangCheckBox/KlangCheckBox.xml")]
    public class KlangCheckBox : UIElement {

        public bool isChecked;
        public string label;
        public Action<bool> onValueChanged;

        [OnMouseClick]
        private void OnMouseClick() {
            isChecked = !isChecked;
            onValueChanged?.Invoke(isChecked);
        }
        
    }

}