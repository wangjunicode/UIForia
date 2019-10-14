using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia.Templates {

    public class UIContainerTemplate : UITemplate {

        private readonly string typeName;

        public UIContainerTemplate(Application app, string elementName, Type rootType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) {
            elementType = rootType;
        }
        
        public UIContainerTemplate(Application app, Type rootType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) {
            elementType = rootType;
        }

        protected override Type elementType { get; }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIContainerElement element = null;

            if (elementType == typeof(UIGroupElement)) {
                element = new UIGroupElement();
            }
            else if (elementType == typeof(UIPanelElement)) {
                element = new UIPanelElement();
            }
            else if (elementType == typeof(UISectionElement)) {
                element = new UISectionElement();
            }
            else if (elementType == typeof(UIDivElement)) {
                element = new UIDivElement();
            }
            else if (elementType == typeof(UIHeaderElement)) {
                element = new UIHeaderElement();
            }
            else if (elementType == typeof(UIFooterElement)) {
                element = new UIFooterElement();
            }
            else if (elementType == typeof(UIRouterLinkElement)) {
                element = new UIRouterLinkElement();
            }
            else {
                element = (UIContainerElement) Activator.CreateInstance(elementType);
            }

            Assert.IsNotNull(element);

            element.children = LightList<UIElement>.Get();
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.OriginTemplate = this;

            CreateChildren(element, childTemplates, inputScope);

            return element;
        }

    }
}