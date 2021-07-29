using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia {

    public unsafe class SequenceContext {

        internal ElementId rootElementId;
        internal StyleDatabase styleDatabase;
        internal StructList<NamedProperty> properties;
        internal DataList<LongBoolMap>.Shared queryResults;
        internal CheckedArray<TraversalInfo> traversalTable;
        internal int elapsedTime;
        internal int deltaTime;
        internal QueryContext queryContext;
        internal AppInfo* appInfo;
        internal LongBoolMap sharedResultBuffer;

        public void SetBoolProperty(string name, bool value) {
            properties ??= new StructList<NamedProperty>();
            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.Bool) {
                    properties.array[i].property.boolVal = value;
                    return;
                }

            }

            properties.Add(new NamedProperty(name, value));
        }

        public void SetIntProperty(string name, int value) {
            properties ??= new StructList<NamedProperty>();
            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.Int) {
                    properties.array[i].property.intVal = value;
                    return;
                }

            }

            properties.Add(new NamedProperty(name, value));
        }

        public void SetFloatProperty(string name, float value) {
            properties ??= new StructList<NamedProperty>();
            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.Float) {
                    properties.array[i].property.floatVal = value;
                    return;
                }

            }

            properties.Add(new NamedProperty(name, value));
        }

        public void SetStringProperty(string name, string value) {

            properties ??= new StructList<NamedProperty>();

            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.String) {
                    properties.array[i].property.strVal = value;
                    return;
                }

            }

            properties.Add(new NamedProperty(name, value));
        }

        public int GetIntProperty(string name) {

            if (properties == null) return default;

            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.Int) {
                    return properties.array[i].property.intVal;
                }

            }

            return default;
        }

        public float GetFloatProperty(string name) {
            if (properties == null) return default;

            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.Float) {
                    return properties.array[i].property.floatVal;
                }

            }

            return default;
        }

        public string GetStringProperty(string name) {
            if (properties == null) return default;

            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.String) {
                    return properties.array[i].property.strVal;
                }

            }

            return default;
        }

        public bool GetBoolProperty(string name) {

            if (properties == null) return false;

            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == name && properties.array[i].property.propertyType == SequencePropertyType.Bool) {
                    return properties.array[i].property.boolVal;
                }

            }

            return default;
        }

        internal void SetProperty(NamedProperty property) {
            properties ??= new StructList<NamedProperty>();

            for (int i = 0; i < properties.size; i++) {

                if (properties.array[i].name == property.name) {
                    properties.array[i].property = property.property;
                    return;
                }

            }

            properties.Add(property);
        }

        internal LongBoolMap GetQueryResults(QueryId query) {
            return queryResults[query.id];
        }

        public LongBoolMap GetSharedResultBuffer() {
            sharedResultBuffer.Clear();
            return sharedResultBuffer;
        }

        public void SetAttribute(ElementId target, string attr, string value) {
            Debug.Log("Set attribute");
        }

    }

}