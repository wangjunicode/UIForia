using System;
using System.Collections.Generic;

namespace UIForia {

    public class UIImageTemplate : UITemplate {

        public UIImageTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UIImageElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIImageElement element = new UIImageElement();
            element.OriginTemplate = this;
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            return element;
        }

    }

}