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
            // split when encountering {} inside of text
            return element;
        }

        private static void TypeCheck(UIElementTemplate root, List<ContextDefinition> contexts) {
            
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
                ContextDefinition context = GetContextByName(attr.contextName);

                if (attr.IsConstantValue) {
                    FieldInfo fieldInfo = context.type.GetField(attr.propertyName);
                    template.AddLiteralBinding(attr.name, ParseByFieldType(attr.constantValue, fieldInfo));
                }
                else if (attr.IsValueLookup) {
                    FieldInfo fieldInfo = context.type.GetField(attr.propertyName);
                    ExpressionBinding binding = ParseAttributeExpression(fieldInfo, attr.propertyName);
                    ExpressionParser parser = new ExpressionParser(new Tokenizer().Tokenize(""));
                    ExpressionBinding binding = parser.Parse(null);
                    if (binding.isMultipart) {
                        for (int j = 0; j < binding.parts.Length; j++) {
                            ExpressionBindingPart part = binding.parts[j];
                            // types of bindings
                            //  -> constant
                            //  -> expression.once
                            //  -> expression.constant
                            //  -> expression.simple
                            //  -> expression.lookup
                            //  -> expression.complex
                        }
                    }
                    // contextId = FindContext(binding);
                    template.AddExpressionBinding(binding);
//                    template.expressionBindings.Add(binding);
                }
            }
            
            for (int i = 0; i < processedType.propFields.Count; i++) {
                FieldInfo fieldInfo = processedType.propFields[i];
                Type expectedType = fieldInfo.FieldType;
                AttributeDefinition attr = attributes.Find((a) => a.name == fieldInfo.Name);

                if (attr == null) continue;

                if (attr.IsConstantValue) {
                    object constantValue = ParseByFieldType(attr.constantValue, fieldInfo);
                    if (!expectedType.IsInstanceOfType(constantValue)) {
                        throw new InvalidTemplateException(TemplateName, $"Typecheck failure {attr.name}");
                    }
                    template.AddLiteralBinding(fieldInfo.Name, constantValue);
                }
                else if (attr.IsValueLookup) {
                    ContextDefinition contextDefinition = GetContextByName(attr.contextName);
                    contextDefinition.type.GetField(attr.propertyName);
                    ExpressionBinding binding = ParseAttributeExpression(fieldInfo, attr.propertyName);
                    binding.contextName = attr.contextName;
                    template.expressionBindings.Add(binding);
                }
            }
        }

        private static ContextDefinition ReadRepeatAttributes(XmlReader reader) {
            List<AttributeDefinition> attributes = ParseAttributes(reader);
            AttributeDefinition listAttr = attributes.Find((a) => a.name == "list");
            AttributeDefinition alias = attributes.Find((a) => a.name    == "alias");
            AttributeDefinition filter = attributes.Find((a) => a.name   == "filter");

            string name = alias != null ? alias.propertyName : "item";
            ContextDefinition contextDefinition = new ContextDefinition(name, null);
            
            if (listAttr == null) {
                throw new InvalidTemplateException(TemplateName, "<Repeat> is missing a 'list' attribute");
            }

            if (alias != null && !alias.IsConstantValue) {
                throw new InvalidTemplateException(TemplateName, "<Repeat> alias must be a constant string value");
            }
            
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
            for (int attInd = 0;
                attInd < reader.AttributeCount;
                attInd++) {
                reader.MoveToAttribute(attInd);
                string attrName = reader.Name;
                string attrValue = reader.Value.Trim();
                attributeList.Add(ParseAttribute(attrName, attrValue));
            }
            return attributeList;
        }

        private static AttributeDefinition ParseAttribute(string attrName, string attrValue) {
            string originalName = attrName;
            string originalValue = attrValue;
            AttributeDefinition retn = new AttributeDefinition();
            if (attrName.IndexOf('.') != -1) {
                string[] split = attrName.Split('.');
                string[] names = new string[split.Length - 1];
                for (int i = 0; i < names.Length; i++) {
                    names[i] = split[i + 1].Trim();
                }
                retn.modifiers = names;
                retn.name = split[0].Trim();
            }
            else {
                retn.name = attrName.Trim();
            }
            attrValue = attrValue.Trim();
            if (attrValue.Contains("{")) {
                if (attrValue[0] != '{') {
                    throw new InvalidTemplateException(TemplateName, $"Attribute {originalName}={originalValue} must have brace as first character ");
                }
                if (attrValue.IndexOf('}') != attrValue.Length - 1) {
                    throw new InvalidTemplateException(TemplateName, $"Attribute {originalName}={originalValue} must have brace as last character ");
                }
                attrValue = attrValue.Substring(1, attrValue.Length - 2).Trim();
                if (attrValue.IndexOf('.') == -1) {
                    retn.contextName = "this";
                    retn.propertyName = attrValue;
                }
                else {
                    string[] split = attrValue.Split('.');
                    if (split.Length != 2) {
                        throw new InvalidTemplateException(TemplateName, $"Attribute {originalName}={originalValue} does not support multiple . paths");
                    }
                    retn.contextName = split[0];
                    retn.expressionString = split[1];
                }
            }
            else {
                retn.contextName = string.Empty;
                retn.constantValue = attrValue;
            }
            return retn;
        }

        private static ExpressionBinding ParseAttributeExpression(FieldInfo fieldInfo, string toParse) {
            bool inverted = toParse[0] == '!';
            if (inverted) {
                toParse = toParse.Substring(1);
            }
            ExpressionBinding expression = new ExpressionBinding();
            expression.fieldInfo = fieldInfo;
            expression.expressionString = toParse;
            if (inverted) {
                expression.flags |= ExpressionBinding.ExpressionFlag.Inverted;
            }
            return expression;
        }

        private static object ParseByFieldType(string rawValue, FieldInfo fieldInfo) {
            Type fieldType = fieldInfo.FieldType;
            if (fieldType == typeof(string)) return rawValue;
            if (fieldType.IsPrimitive) {
                if (fieldType == typeof(bool)) {
                    return bool.Parse(rawValue);
                }
                else if (fieldType == typeof(int)) {
                    return int.Parse(rawValue);
                }
                else if (fieldType == typeof(float)) {
                    return float.Parse(rawValue, CultureInfo.InvariantCulture);
                }
                else if (fieldType == typeof(double)) {
                    return double.Parse(rawValue, CultureInfo.InvariantCulture);
                }
            }
            return null;
        }

        private class ContextDefinition {

            public readonly string name;
            public readonly List<string> bindings;
            public readonly ProcessedType type;

            public ContextDefinition(string name, ProcessedType type) {
                this.name = name;
                this.bindings = new List<string>();
                this.type = type;
            }

        }

        private class AttributeDefinition {

            public string name;
            public string[] modifiers;
            public string contextName;
            public string propertyName;
            public string constantValue;
            public string expressionString;
            public bool IsComplexExpression => expressionString != null;
            public bool IsValueLookup => propertyName           != null;
            public bool IsConstantValue => constantValue        != null;

        }

    }

}