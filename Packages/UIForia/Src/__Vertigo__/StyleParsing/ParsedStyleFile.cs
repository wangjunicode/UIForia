using System;

namespace UIForia.NewStyleParsing {

    public class ParsedStyleFile {

        public StyleASTNode[] nodeList;
        public StyleNode[] styleNodeList;
        public PropertyDefinition[] propertyDefinitions;
        public StyleDependency[] dependencies;

        public unsafe void Serialize(ref ManagedByteBuffer buffer) {

            buffer.Write(nodeList.Length);

            fixed (StyleASTNode* ptr = nodeList) {
                buffer.Write(ptr, nodeList.Length * sizeof(StyleASTNode));
            }

            buffer.Write(styleNodeList.Length);

            for (int i = 0; i < styleNodeList.Length; i++) {
                styleNodeList[i].Serialize(ref buffer);
            }

            buffer.Write(propertyDefinitions.Length);
            for (int i = 0; i < propertyDefinitions.Length; i++) {
                propertyDefinitions[i].Serialize(ref buffer);
            }

        }

        public static ParsedStyleFile Deserialize(byte[] bytes) {
            ManagedByteBuffer buffer = new ManagedByteBuffer(bytes);
            ParsedStyleFile retn = new ParsedStyleFile();

            buffer.Read(out int nodeListLength);
            retn.nodeList = new StyleASTNode[nodeListLength];
            buffer.Read(retn.nodeList);

            buffer.Read(out int styleNodeListLength);
            retn.styleNodeList = new StyleNode[styleNodeListLength];
            for (int i = 0; i < styleNodeListLength; i++) {
                retn.styleNodeList[i].Deserialize(ref buffer);
            }

            buffer.Read(out int propertyCount);
            retn.propertyDefinitions = new PropertyDefinition[propertyCount];
            for (int i = 0; i < propertyCount; i++) {
                retn.propertyDefinitions[i].Deserialize(ref buffer);
            }

            return retn;
        }

    }

}