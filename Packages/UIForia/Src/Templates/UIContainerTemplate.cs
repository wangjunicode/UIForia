using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia.Templates {

    public class UIContainerTemplate : UITemplate {

        private readonly string typeName;

        public UIContainerTemplate(Application app, string elementName, Type rootType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) {
            elementType = rootType;
            this.elementName = elementName;
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

            element.children = LightListPool<UIElement>.Get();
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.OriginTemplate = this;

            CreateChildren(element, childTemplates, inputScope);

            return element;
        }

    }

    public class UITextElementTemplate : UITemplate {

        public UITextElementTemplate(Application app, Type elementType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) {
            this.elementType = elementType;
        }

        protected override Type elementType { get; }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UITextElement element = null;
            if (elementType == typeof(UILabelElement)) { }
            else if (elementType == typeof(UIParagraphElement)) {
                element = new UIParagraphElement();
            }
            else if (elementType == typeof(UIHeading1Element)) {
                element = new UIHeading1Element();
            }
            else if (elementType == typeof(UIHeading2Element)) {
                element = new UIHeading2Element();
            }
            else if (elementType == typeof(UIHeading3Element)) {
                element = new UIHeading3Element();
            }
            else if (elementType == typeof(UIHeading4Element)) {
                element = new UIHeading4Element();
            }
            else if (elementType == typeof(UIHeading5Element)) {
                element = new UIHeading5Element();
            }
            else if (elementType == typeof(UIHeading6Element)) {
                element = new UIHeading6Element();
            }

            Assert.IsNotNull(element);

            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.children = LightListPool<UIElement>.Get();
            element.OriginTemplate = this;

            return element;
        }

    }

}