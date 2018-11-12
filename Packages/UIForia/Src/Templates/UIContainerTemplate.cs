using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia {

    public class UIContainerTemplate : UITemplate {

        private readonly string typeName;

        public UIContainerTemplate(Type rootType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) {
            elementType = rootType;
        }

        public override Type elementType { get; }
        
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
            else {
                element = (UIContainerElement) Activator.CreateInstance(elementType);
            }
            
            Assert.IsNotNull(element);
            
            element.children = new UIElement[childTemplates.Count];
            
            for (int i = 0; i < childTemplates.Count; i++) {
                element.children[i] = childTemplates[i].CreateScoped(inputScope);
                element.children[i].parent = element;
                element.children[i].templateParent = element;
            }

            element.TemplateContext = inputScope.context;
            element.templateRef = this;
            return element;
        }

    }

    public class UITextElementTemplate : UITemplate {

        public UITextElementTemplate(Type elementType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) {
            this.elementType = elementType;
        }

        public override Type elementType { get; }
        
        public override UIElement CreateScoped(TemplateScope inputScope) {

            UITextElement element = null;
            if (elementType == typeof(UILabelElement)) {
                
            }
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

            element.children = ArrayPool<UIElement>.Empty;
            element.templateRef = this;
            element.TemplateContext = inputScope.context;
            
            return element;

        }

    }

}