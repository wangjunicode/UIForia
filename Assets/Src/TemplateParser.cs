using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

namespace Src {
    public static class TemplateParser {
        private static readonly Dictionary<string, ParsedTemplate> parsedTemplates = new Dictionary<string, ParsedTemplate>();

        private static List<string> templatesToParse = new List<string>();
        private static ParsedTemplate currentParsedTemplate;

        // todo -- take in a template path or id instead of the full thing
        public static ParsedTemplate ParseTemplate(string template) {
            if (parsedTemplates.ContainsKey(template)) {
                return parsedTemplates[template];
            }

            XmlReader xReader = XmlReader.Create(new StringReader(template));
            if (!xReader.ReadToDescendant("RootType")) { }

            string typeName = xReader.ReadString();
            currentParsedTemplate = InitParsedTemplate(typeName);

            if (!xReader.ReadToNextSibling("Contents")) { }

            while (xReader.Read()) {
                switch (xReader.NodeType) {
                    
                    case XmlNodeType.Element:
                        bool shouldDescend = !xReader.IsEmptyElement;
                        ParseElement(xReader);
                        if(shouldDescend) currentParsedTemplate.Descend();
                        break;
                    case XmlNodeType.EndElement:
                        currentParsedTemplate.Ascend();
                        break;
                    
                    case XmlNodeType.Text:
                        ParseTextNode(xReader);
                        break;
                }
               
            }

            parsedTemplates[template] = currentParsedTemplate;
            return currentParsedTemplate;
        }

        private static void ParseTextNode(XmlReader reader) {
            ProcessedType processedType = TypeProcessor.ProcessType(typeof(UITextElement));
            UIElementTemplate element = new UIElementTemplate(processedType);
            element.text = reader.Value;
            currentParsedTemplate.AddChild(element);
        }

        private static void ParseElement(XmlReader reader) {
            // todo -- if template not parsed, add to list needing to parsed (if not primitive)
            ProcessedType childProcessedType = TypeProcessor.ProcessType(reader.Name);
            
            UIElementTemplate element = new UIElementTemplate(childProcessedType);
            
            for (int attInd = 0; attInd < reader.AttributeCount; attInd++) {
                reader.MoveToAttribute(attInd);

                string attrName = reader.Name;
                string attrValue = reader.Value;

                // here we know what the inputs are from the context and can construct bindings
                if (!childProcessedType.HasProp(attrName)) continue;
                // todo accept literals
                if (attrValue[0] == '$') {
                    attrValue = attrValue.Substring(1);
                }
                element.propBindings.Add(new PropertyBindPair(attrName, attrValue));
            }
            
            currentParsedTemplate.AddChild(element);
        }

        private static ParsedTemplate InitParsedTemplate(string typeName) {
            Type type = Type.GetType(typeName);
            if (type == null) throw new Exception("Cannot find type: " + typeName);
            return new ParsedTemplate(TypeProcessor.ProcessType(type));
        }
       
        public static ParsedTemplate GetParsedTemplate(string typeName) {
            throw new NotImplementedException();
        }
    }
}