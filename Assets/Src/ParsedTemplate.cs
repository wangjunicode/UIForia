using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;

namespace Src {
    public class ParsedTemplate {
        // note -- when parsing templates the element templates know their own bindings
        // when creating a template instance, the bindings should live on a map of the root element
        // mapping propertyName -> real element reference

        private readonly ProcessedType rootType;

        public readonly UIElementTemplate rootElementTemplate;
        private UIElementTemplate currentParent;

        public ParsedTemplate(ProcessedType type) {
            rootType = type;
            rootElementTemplate = new UIElementTemplate(type);
            currentParent = rootElementTemplate;
        }

        public void Ascend() {
            if (currentParent == rootElementTemplate) return;
            currentParent = currentParent.parent;
        }

        public void Descend() {
            currentParent = currentParent.children[currentParent.children.Count - 1];
        }

        public void AddChild(UIElementTemplate elementTemplate) {
            elementTemplate.parent = currentParent;
            currentParent.children.Add(elementTemplate);
        }

        // todo this will need to accept a prop list and probably a parent element
        public UIElement Instantiate() {
            TemplateContext context = new TemplateContext();

            UIElement element = Activator.CreateInstance(rootType.type) as UIElement;
            
            Assert.IsNotNull(element, "element != null");
            element.templateContext = context;

            for (int i = 0; i < rootType.contextProperties.Count; i++) {
                FieldInfo fieldInfo = rootType.contextProperties[i];
                ObservedProperty observedProperty = Activator.CreateInstance(fieldInfo.FieldType) as ObservedProperty;
                if (observedProperty != null) {
                    fieldInfo.SetValue(element, observedProperty);
                }
            }

            element.children = CreateChildrenRecursive(context, rootElementTemplate, element);

            InitializeElementTree(element);
            
            return element;
        }

        // todo -- need to expand child templates if child isn't a primitive
        private UIElement[] CreateChildrenRecursive(TemplateContext context, UIElementTemplate template, UIElement parent) {
            Transform parentTransform = parent.gameObject.transform;
            UIElement[] children = new UIElement[template.children.Count];
            for (int i = 0; i < template.children.Count; i++) {
                UIElementTemplate childTemplate = template.children[i];
                UIElement child = Activator.CreateInstance(childTemplate.type.type) as UIElement;
                Debug.Assert(child != null, nameof(child) + " != null");
                // expand child here if not a primitive
                if (!(child is UIElementPrimitive)) {
                    TemplateParser.GetParsedTemplate(child.GetType().ToString()).Instantiate(/*prop values*/);
                }
                CreateContextBindings(context, childTemplate, child);
                child.gameObject.transform.SetParent(parentTransform);
                children[i] = child;
                child.children = CreateChildrenRecursive(context, childTemplate, child);
            }

            return children;
        }

        private void InitializeElementTree(UIElement element) {
            for (int i = 0; i < element.children.Length; i++) {
                InitializeElementTree(element.children[i]);
            }    
            element.Initialize(null);
        }
        
        private void CreateContextBindings(TemplateContext context, UIElementTemplate template, UIElement element) {
            for (int i = 0; i < template.propBindings.Count; i++) {
                context.CreateBinding(template.propBindings[i], element);    
            }    
        }
        
    }
}