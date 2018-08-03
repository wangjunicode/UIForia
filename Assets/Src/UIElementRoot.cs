using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Xml;
using Src;

[ExecuteInEditMode]
public class UIElementRoot : MonoBehaviour {
    private Dictionary<string, Type> elementTypeMap = new Dictionary<string, Type>();
    private Dictionary<Type, ElementMetaData> elementMetaMap = new Dictionary<Type, ElementMetaData>();
    private Stack<TemplateContext> contexts;

    public void SetState(string key, object value) {
        //foreach output value
    }

    public void ParseTemplate(string template) {
//        contexts = new Stack<TemplateContext>();
////        contexts.Push(new TemplateContext());
//
//        XmlReader xReader = XmlReader.Create(new StringReader(template));
//        while (xReader.Read()) {
//            switch (xReader.NodeType) {
//                case XmlNodeType.Element:
//                    // get available props from type
//                    // get assigned props from template parsing
//                    // augment context if needed
//                    ParseElement(xReader);
//                    break;
//                case XmlNodeType.Text:
//                    break;
//                case XmlNodeType.EndElement:
//                    break;
//                case XmlNodeType.Comment:
//                    break;
//            }
//        }
    }

    public ElementMetaData ProcessElementType(string elementName) {
        Type type;
        if (!elementTypeMap.TryGetValue(elementName, out type)) {
            type = Type.GetType(elementName);
            if (type != null) {
                elementTypeMap[elementName] = type;
            }
        }

        if (type == null) return null;

        ElementMetaData meta;
        if (elementMetaMap.TryGetValue(type, out meta)) {
            return null;
        }

        meta = new ElementMetaData();
        meta.elementType = type;
        elementMetaMap[type] = meta;

        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++) {
            FieldInfo fieldInfo = fields[i];
            object[] attrs = fieldInfo.GetCustomAttributes(typeof(PropAttribute), true);
            if (attrs.Length > 0) {
                meta.AddPropField(fieldInfo.Name, fieldInfo.FieldType);
                continue; // props are not compatible with observed properties
            }

            if (fieldInfo.FieldType.IsAssignableFrom(typeof(ObservedProperty<>))) {
                meta.observedProperties.Add(fieldInfo);
            }
        }

        return meta;
    }

    private void ParseElement(XmlReader reader) {
        ElementMetaData meta = ProcessElementType(reader.Name);

        UIElement element = Activator.CreateInstance(meta.elementType) as UIElement;

        for (int i = 0; i < meta.observedProperties.Count; i++) {
            FieldInfo fieldInfo = meta.observedProperties[i];
            ObservedProperty observedProperty = Activator.CreateInstance(fieldInfo.FieldType) as ObservedProperty;
            if (observedProperty != null) {
                fieldInfo.SetValue(element, observedProperty);
            }
        }

        int attrCount = reader.AttributeCount;
        if (attrCount == 0) return;

        TemplateContext context = contexts.Peek();
        for (int attInd = 0; attInd < reader.AttributeCount; attInd++) {
            reader.MoveToAttribute(attInd);

            string attrName = reader.Name;
            string attrValue = reader.Value;
            // here we know what the inputs are from the context and can construct bindings
            if (!meta.HasProp(attrName)) continue;
            PropertyBinding binding = ParsePropAttr(element, contexts.Peek(), attrName, attrValue);
            //context.CreateBinding(attrName, binding);
        }

    }

    private PropertyBinding ParsePropAttr(UIElement element, TemplateContext context, string attrName,
        string attrValue) {
        PropertyBinding binding = new PropertyBinding(attrName, BindingType.Prop);
        attrValue = attrValue.Trim();
        // for now assume we only have context variables from the template root type
        // we need to change this for events and lists eventually
        if (attrValue[0] == '$') {
            // assume no dot properties yet
            attrValue = attrValue.Substring(1);
            // todo -- typecheck 
           // context.CreateBinding(attrName, attrValue);
        }
        else {
            //primitive
        }

        return binding;
    }

    public static bool IsProp(string prop) {
        return true;
    }

    public static bool IsObservableField(string prop) {
        return true;
    }
}