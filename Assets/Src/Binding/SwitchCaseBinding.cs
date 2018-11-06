

namespace Src {

    public class SwitchCaseBinding : Binding {

        private readonly int index;
        private readonly int switchId;

        public SwitchCaseBinding(int switchId, int index) : base($"{switchId}:{index}") {
            this.switchId = switchId;
            this.index = index;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
//            if (index == context.GetSwitchValue(switchId)) {
//                if ((element.flags & UIElementFlags.Enabled) == 0) {
//                    context.view.EnableElement(element);
//                }
//            }
//            else {
//                if ((element.flags & UIElementFlags.Enabled) != 0) {
//                    context.view.DisableElement(element);
//                }
//            }
        }

        public override bool IsConstant() {
            return false;
        }

    }

}