using System.Diagnostics;
using Unity.Mathematics;

namespace UIForia.Text {

    [DebuggerTypeProxy(typeof(StringTagInfoDebugView))]
    public unsafe struct StringTagInfo {

        public int lastTouched;
        public int length;
        public int capacity;
        public char* str;

        public StringTagInfo(int frameId, char* str, int length, int capacity) {
            this.lastTouched = frameId;
            this.str = str;
            this.length = length;
            this.capacity = capacity;
        }

        public int NextFreeIndex {
            get => length;
            set { length = value; }
        }

        public bool IsFree => str == null;

    }

    internal sealed unsafe class StringTagInfoDebugView {

        public string str;
        public int lastTouchedFrame;
        public bool isFree;

        public StringTagInfoDebugView(StringTagInfo str) {
            this.str = new string(str.str, 0, math.max(0, str.length));
            this.lastTouchedFrame = str.lastTouched;
            this.isFree = str.IsFree;
        }

    }

}