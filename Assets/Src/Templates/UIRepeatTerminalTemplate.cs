
namespace Src {

    public class UIRepeatTerminalTemplate : UITemplate {

        private readonly Binding[] terminalBindings;

        public UIRepeatTerminalTemplate(Binding[] terminalBindings) : base(null) {
            this.terminalBindings = terminalBindings;
        }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            UIElementCreationData data = GetCreationData(new UIRepeatTerminal(), scope.context);
            data.bindings = terminalBindings;
            return data;
        }

    }

}