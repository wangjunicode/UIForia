using System;
using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/Checkbox.xml")]
    public class Checkbox : UIElement {

        public bool isChecked;

        [WriteBinding(nameof(isChecked))]
        public event Action<bool> onValueChanged;

        public Color checkedColor = Color.red;
        public Color uncheckedColor = Color.clear;
        
        [OnMouseClick]
        public void Toggle() {
            isChecked = !isChecked;
            onValueChanged?.Invoke(isChecked);
        }
    }

}