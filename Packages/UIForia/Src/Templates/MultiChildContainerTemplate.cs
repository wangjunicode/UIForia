using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Templates {

    public class MultiChildContainerTemplate : UITemplate {

        public MultiChildContainerTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) 
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(RepeatMultiChildContainerElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            RepeatMultiChildContainerElement element = new RepeatMultiChildContainerElement();
            element.children = new LightList<UIElement>(childTemplates.Count);
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.OriginTemplate = this;
            CreateChildren(element, childTemplates, inputScope);
            return element;
        }

    }

}