
using UnityEngine;

namespace Src {

    public class SwitchCaseBinding : Binding {

        private readonly int index;
        private readonly int switchId;

        public SwitchCaseBinding(int switchId, int index) {
            this.switchId = switchId;
            this.index = index;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (index == context.GetSwitchValue(switchId)) {
                if ((element.flags & UIElementFlags.Enabled) == 0) {
                    Debug.Log("Enabling case: " + index);
                    context.view.EnableElement(element);
                }
            }
            else {
                if ((element.flags & UIElementFlags.Enabled) != 0) {
                    Debug.Log("Disabling case: " + index);
                    context.view.DisableElement(element);
                }
            }
        }

        public override bool IsConstant() {
            return false;
        }

    }

}