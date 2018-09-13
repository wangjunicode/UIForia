
using System;

namespace Src {

    public class UIRepeatTerminalTemplate : UITemplate {

        private readonly Binding[] terminalBindings;

        public UIRepeatTerminalTemplate(Binding[] terminalBindings) : base(null) {
            this.terminalBindings = terminalBindings;
        }

        public override Type elementType => typeof(UIRepeatTerminal);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            MetaData data = GetCreationData(new UIRepeatTerminal(), inputScope.context);
            data.bindings = terminalBindings;
            return data;
        }

    }

}