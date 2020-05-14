using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Style;

namespace UIForia {

    [AssertSize(32)]
    [DebuggerTypeProxy(typeof(StaticStyleInfoDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public struct StaticStyleInfo {

        public ModuleId moduleId;
        public ushort activeOffset;
        public ushort activeCount;
        public ushort totalPropertyCount;
        public ushort normalOffset;
        public ushort normalCount;
        public ushort hoverOffset;
        public ushort hoverCount;
        public ushort focusOffset;
        public ushort focusCount;
        public int propertyOffsetInBytes;

        public ushort dataBaseId;

    }

    public struct StaticStyleInfoDebugView {

        public StyleProperty2[] normal;
        public StyleProperty2[] active;
        public StyleProperty2[] hover;
        public StyleProperty2[] focus;

        public unsafe StaticStyleInfoDebugView(StaticStyleInfo info) {

            StyleDatabase db = StyleDatabase.GetDatabase(info.dataBaseId);

            byte* ptr = db.buffer_staticStyleProperties.data;

            active = MakeArray(info.activeCount, info.activeOffset, ptr, info.totalPropertyCount, info.propertyOffsetInBytes);
            focus = MakeArray(info.focusCount, info.focusOffset, ptr, info.totalPropertyCount, info.propertyOffsetInBytes);
            hover = MakeArray(info.hoverCount, info.hoverOffset, ptr, info.totalPropertyCount, info.propertyOffsetInBytes);
            normal = MakeArray(info.normalCount, info.normalOffset, ptr, info.totalPropertyCount, info.propertyOffsetInBytes);

        }

        private static unsafe StyleProperty2[] MakeArray(ushort count, ushort offset, byte* ptr, int totalPropertyCount, int propertyOffsetInBytes) {
            StaticPropertyId* keys = (StaticPropertyId*) (ptr + propertyOffsetInBytes + offset);
            PropertyData* data = (PropertyData*) (keys + totalPropertyCount);
            StyleProperty2[] properties = new StyleProperty2[count];

            for (int i = 0; i < count; i++) {
                properties[i] = new StyleProperty2(keys[i].propertyId, data[i].value);
            }

            return properties;
        }

    }

}