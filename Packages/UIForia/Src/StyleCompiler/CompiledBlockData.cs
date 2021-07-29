using System.Runtime.InteropServices;
using UIForia.Style;

namespace UIForia.Compilers {

    [StructLayout(LayoutKind.Explicit)]
    internal struct CompiledBlockData {

        [FieldOffset(0)] public StateData stateData;
        [FieldOffset(0)] public AttributeData attrData;

        public struct StateData {

            public StyleState state;

        }

        public struct AttributeData {

            public int keyId;
            public int valueId;
            public AttributeOperator op;

        }

    }

}