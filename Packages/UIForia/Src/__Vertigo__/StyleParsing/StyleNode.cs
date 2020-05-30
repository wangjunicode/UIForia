using System;

namespace UIForia.NewStyleParsing {

    [Serializable]
    public struct StyleNode {

        public int index;
        public string styleName;
        public string extendName;

        public void Serialize(ref ManagedByteBuffer buffer) {
            buffer.Write(index);
            buffer.Write(styleName);
            buffer.Write(extendName);
        }

        public void Deserialize(ref ManagedByteBuffer buffer) {
            buffer.Read(out index);
            buffer.Read(out styleName);
            buffer.Read(out extendName);
        }

    }

}