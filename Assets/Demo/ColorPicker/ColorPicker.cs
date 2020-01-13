using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace Demo.ColorPicker {

    [Template("Demo/ColorPicker/ColorPicker.xml")]
    public class ColorPicker : UIElement, IFocusable {

        public RepeatableList<Color> availableColors;

        public Color selectedColor;

        public bool hasFocus;

        [OnMouseClick]
        public void OpenColorPalette() {
            application.InputSystem.RequestFocus(this);
        }

        public void SelectColor(Color color) {
            application.InputSystem.ReleaseFocus(this);
            selectedColor = color;
            style.SetBackgroundColor(color, StyleState.Normal);
        }

        public bool Focus() {
            hasFocus = true;
            return true;
        }

        public void Blur() {
            hasFocus = false;
        }
    }
}
