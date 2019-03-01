using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;

namespace UIForia.Templates {

    public class UIImageTemplate : UITemplate {

        public UIImageTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UIImageElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIImageElement element = new UIImageElement();
            element.OriginTemplate = this;
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            return element;
        }

    }

}