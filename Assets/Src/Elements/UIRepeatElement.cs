using System;
using System.Collections.Generic;

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

        // todo --  have an inline version of that has no layout / render presense
        public UIRepeatElement(UITemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
        }

    }

}