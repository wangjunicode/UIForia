using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Src {

    // type check the template attributes
    // process attributes 
    // handle importing
    // handle style nodes
    // ensure valid contexts
    // warn about non-existent props
    // ensure required things are required
    // build and validate contexts
    // ensure terminal nodes are terminal
    // reference constant values not in the context (for things like a route map)

    // todo slot & children nodes cannot appear inside repeats

    public static class TemplateParser {

        private static string TemplateName = string.Empty;

        private static readonly List<ContextDefinition> contexts = new List<ContextDefinition>();
        private static readonly Dictionary<Type, UIElementTemplate> parsedTemplates = new Dictionary<Type, UIElementTemplate>();

        public static UIElementTemplate GetParsedTemplate(Type elementType, bool forceReload = false) {
            if (!forceReload && parsedTemplates.ContainsKey(elementType)) {
                return parsedTemplates[elementType];
            }
            ProcessedType type = TypeProcessor.GetProcessedType(elementType);
            string template = File.ReadAllText(Application.dataPath + type.GetTemplatePath());
            UIElementTemplate parsedTemplate = ParseTemplate(type, template);
            parsedTemplates[elementType] = parsedTemplate;
            return parsedTemplate;
        }

        public static UIElementTemplate GetParsedTemplate<T>(bool forceReload = false) where T : UIElement {
            return GetParsedTemplate(typeof(T), forceReload);
        }

        private static UIElementTemplate ParseTemplate(ProcessedType type, string template) {
            TemplateName = type.GetTemplatePath();
            contexts.Clear();

            XmlReader xReader = XmlReader.Create(new StringReader(template));
            if (!xReader.ReadToDescendant("Contents")) {
                throw new InvalidTemplateException(type.type.Name, "Template is missing a Contents Section");
            }

            UIElementTemplate root = new UIElementTemplate(UIElementTemplateType.Template, type);
            Stack<UIElementTemplate> stack = new Stack<UIElementTemplate>();
            stack.Push(root);

            AddContext(new ContextDefinition("this", type));

            bool foundSlot = false;
            while (xReader.Read()) {
                switch (xReader.NodeType) {

                    case XmlNodeType.Element:
                        bool shouldDescend = !xReader.IsEmptyElement;

                        UIElementTemplate parsedElement = ParseElement(xReader);
                        if (parsedElement.IsSlot && foundSlot) {
                            throw new MultipleChildSlotException(type.type.Name);
                        }
                        if (parsedElement.IsSlot) {
                            foundSlot = true;
                            if (shouldDescend) {
                                throw new ChildrenSlotWithChildrenException(type.type.Name);
                            }
                        }
                        stack.Peek().children.Add(parsedElement);
                        if (shouldDescend) {
                            stack.Push(parsedElement);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (stack.Count > 0) stack.Pop();
                        break;

                    case XmlNodeType.Text:
                        stack.Peek().children.Add(ParseTextNode(xReader));
                        break;
                    case XmlNodeType.Whitespace:
                        break;
                }

            }
            if (stack.Count > 0) stack.Pop();
            xReader.Dispose();
            TemplateName = string.Empty;
            return root;
        }

        private static UIElementTemplate ParseTextNode(XmlReader reader) {
            ProcessedType processedType = TypeProcessor.GetProcessedType(typeof(UITextElement));
            UIElementTemplate element = new UIElementTemplate(UIElementTemplateType.Text, processedType);
            element.text = reader.Value;
            // todo -- split when encountering {} inside of text
            return element;
        }

        private static UIElementTemplate ParseElement(XmlReader reader) {
            UIElementTemplateType templateType = UIElementTemplateType.Template;

            if (reader.Name == "Children") {
                return new UIElementTemplate(UIElementTemplateType.Slot, null);
            }

            if (reader.Name == "Repeat") {
                AddContext(ReadRepeatAttributes(reader));
                return new UIElementTemplate(UIElementTemplateType.Repeat, null);
            }

            ProcessedType childProcessedType = TypeProcessor.GetProcessedType(reader.Name);

            if (childProcessedType.isPrimitive) {
                templateType = UIElementTemplateType.Primitive;
            }

            UIElementTemplate element = new UIElementTemplate(templateType, childProcessedType);

            List<AttributeDefinition> attributes = ParseAttributes(reader);
            CreateBindings(element, childProcessedType, attributes);

            return element;
        }

        private static void CreateBindings(UIElementTemplate template, ProcessedType processedType, List<AttributeDefinition> attributes) {

            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];
               // ContextDefinition context = GetContextByName(attr.contextName);
            }

        }

        private static ContextDefinition ReadRepeatAttributes(XmlReader reader) {
            List<AttributeDefinition> attributes = ParseAttributes(reader);
//            AttributeDefinition listAttr = attributes.Find((a) => a.name == "list");
            AttributeDefinition alias = attributes.Find((a) => a.name == "alias");
//            AttributeDefinition filter = attributes.Find((a) => a.name   == "filter");

            string name = alias != null ? alias.key : "item";
            ContextDefinition contextDefinition = new ContextDefinition(name, null);

//            if (listAttr == null) {
//                throw new InvalidTemplateException(TemplateName, "<Repeat> is missing a 'list' attribute");
//            }
//
//            if (alias != null && !alias.IsConstantValue) {
//                throw new InvalidTemplateException(TemplateName, "<Repeat> alias must be a constant string value");
//            }

            return contextDefinition;

        }

        private static void AddContext(ContextDefinition context) {
            ContextDefinition existing = GetContextByName(context.name);
            if (existing != null) {
                throw new InvalidTemplateException(TemplateName, $"<Repeat> is using an alias '{context.name}' but this is already taken.");
            }
        }

        private static ContextDefinition GetContextByName(string contextName) {
            return contexts.Find((c) => c.name == contextName);
        }

        private static List<AttributeDefinition> ParseAttributes(XmlReader reader) {
            List<AttributeDefinition> attributeList = new List<AttributeDefinition>();
            for (int i = 0; i < reader.AttributeCount; i++) {
                reader.MoveToAttribute(i);
                attributeList.Add(new AttributeDefinition(reader.Name, reader.Value));
            }
            return attributeList;
        }

    }

}