﻿
namespace Src {

    public class UIRepeatTerminalTemplate : UITemplate {

        private readonly Binding[] terminalBindings;

        public UIRepeatTerminalTemplate(Binding[] terminalBindings) : base(null) {
            this.terminalBindings = terminalBindings;
        }

        public override InitData CreateScoped(TemplateScope inputScope) {
            InitData data = GetCreationData(new UIRepeatTerminal(), inputScope.context);
            data.bindings = terminalBindings;
            return data;
        }

    }

}