using System;
using UIForia.NewStyleParsing;
using UIForia.Style;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public struct PropertyDefinition : IComparable<PropertyDefinition> {

        public PropertyId propertyId;
        public int nodeIndex;
        public string propertyName;
        public string propertyValue;
        public RangeInt valueContentRange;
        public int parserIndex;
        public ushort conditionId;
        public ushort conditionDepth;

        public int CompareTo(PropertyDefinition other) {
            return nodeIndex.CompareTo(other.nodeIndex);
        }

        public void Deserialize(ref ManagedByteBuffer buffer) {
            buffer.Read(out int propertyIdHolder);
            buffer.Read(out nodeIndex);
            buffer.Read(out propertyName);
            buffer.Read(out propertyValue);
            buffer.Read(out valueContentRange.start);
            buffer.Read(out valueContentRange.length);
            buffer.Read(out parserIndex);
            buffer.Read(out conditionId);
            buffer.Read(out conditionDepth);
            propertyId = propertyIdHolder;
        }

        public void Serialize(ref ManagedByteBuffer buffer) {
            buffer.Write(propertyId);
            buffer.Write(nodeIndex);
            buffer.Write(propertyName);
            buffer.Write(propertyValue);
            buffer.Write(valueContentRange.start);
            buffer.Write(valueContentRange.length);
            buffer.Write(parserIndex);
            buffer.Write(conditionId);
            buffer.Write(conditionDepth);
        }

    }

}