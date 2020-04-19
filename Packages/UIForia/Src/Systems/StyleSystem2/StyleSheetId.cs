using System.Runtime.InteropServices;

namespace UIForia {

    [StructLayout(LayoutKind.Explicit)]
    public struct StyleSheetId {

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort moduleId;
        [FieldOffset(2)] public readonly ushort index;

        public StyleSheetId(ushort moduleId, ushort index) {
            this.id = 0;
            this.moduleId = moduleId;
            this.index = index;
        }

    }

}