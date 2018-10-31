using System;

namespace Src {

    public class UIRepeatElement : UIElement {

        public readonly UITemplate template;
        public readonly TemplateScope scope;

        public Expression listExpression;

        public Type listType;
        public Type itemType;
        public string itemAlias;
        public string indexAlias;
        public string lengthAlias;

        public UIRepeatElement(UITemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
            flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresRendering);
        }

        protected override string GetDisplayName() {
            return "Repeat";
        }

    }

}