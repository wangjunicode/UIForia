using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ElementMetaData {
    public List<KeyValuePair<string, Type>> propList = new List<KeyValuePair<string, Type>>();
    public Type elementType;
    public List<FieldInfo> observedProperties;

    public void AddPropField(string key, Type type) {
        propList.Add(new KeyValuePair<string, Type>(key, type));
    }

    public bool HasProp(string attrName) {
        for (int i = 0; i < propList.Count; i++) {
            if (propList[i].Key == attrName) return true;
        }

        return false;
    }
}