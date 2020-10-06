using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/ToggleSwitch/ToggleSwitchSmall.xml")]
    public class ToggleSwitchSmall : UIElement {

        private bool _value;
        private UIElement knob;

        public bool value {
            get => _value;
            set {
                if (value == this._value) {
                    return;
                }

                _value = value;
                knob?.SetAttribute("status", _value ? "on" : "off");
                SetAttribute("status", _value ? "on" : "off");
            }
        }

        public override void OnEnable() {
            knob = knob ?? FindById("knob");
            knob.SetAttribute("status", _value ? "on" : "off");
            SetAttribute("status", _value ? "on" : "off");
        }

        [OnMouseClick]
        public void Toggle() {
            value = !_value;
            knob.SetAttribute("status", _value ? "animate_on" : "animate_off");
            SetAttribute("status", _value ? "animate_on" : "animate_off");
        }

    }

    [Template("SeedLib/ToggleSwitch/ToggleSwitch.xml")]
    public class ToggleSwitch : UIElement {

        private bool _value;
        private UIElement knob;

        public bool value {
            get => _value;
            set {
                if (value == this._value) {
                    return;
                }

                _value = value;
                knob?.SetAttribute("status", _value ? "on" : "off");
                SetAttribute("status", _value ? "on" : "off");
            }
        }

        public override void OnEnable() {
            knob = knob ?? FindById("knob");
            knob.SetAttribute("status", _value ? "on" : "off");
            SetAttribute("status", _value ? "on" : "off");
        }

        [OnMouseClick]
        public void Toggle() {
            value = !_value;
            knob.SetAttribute("status", _value ? "animate_on" : "animate_off");
            SetAttribute("status", _value ? "animate_on" : "animate_off");
        }

    }

}