using System;
using System.Runtime.InteropServices;
using UIForia.Selectors;
using UIForia.Rendering;
using UIForia.Systems;
using Unity.Collections;
using UnityEngine;

namespace UIForia {

    // todo -- these need a free call on destroy
    [StructLayout(LayoutKind.Explicit)]
    public struct StyleProperty2 {

        // important! do not change these fields outside of constructors
        [FieldOffset(0)] public readonly StylePropertyId propertyId;
        [FieldOffset(2)] internal StylePropertyFlags flags;
        [FieldOffset(4)] internal int int0;
        [FieldOffset(4)] internal float float0;
        [FieldOffset(8)] internal RawIntBuffer intPtr;
        [FieldOffset(8)] internal int int1;
        [FieldOffset(8)] internal float float1;

        [Flags]
        internal enum StylePropertyFlags : ushort {

            IntBufferPtr = 1 << 0,
            HasValue = 1 << 1,

        }

        internal StyleProperty2(StylePropertyId propertyId, string data) {
            int id = StyleSystem2.GetStringId(data);
            this.propertyId = propertyId;
            this.flags = 0;
            this.float0 = 0;
            this.intPtr = default;
            this.int1 = 0;
            this.float1 = 0;
            if (data != null) {
                this.flags = StylePropertyFlags.IntBufferPtr | StylePropertyFlags.HasValue;
                this.int0 = id;
            }
            else {
                this.int0 = -1;
            }

        }

        internal StyleProperty2(StylePropertyId propertyId, in Color color) {
            this.propertyId = propertyId;
            this.float0 = default;
            this.float1 = default;
            this.intPtr = default;
            this.int1 = default;
            this.int0 = new StyleColor(color).rgba;
            this.flags = StylePropertyFlags.HasValue;
        }

        internal StyleProperty2(StylePropertyId propertyId, in Color? color) {
            this.propertyId = propertyId;
            this.float0 = default;
            this.float1 = default;
            this.intPtr = default;
            this.int1 = default;
            if (color.HasValue) {
                this.int0 = new StyleColor(color.Value).rgba;
                this.flags = StylePropertyFlags.HasValue;
            }
            else {
                this.int0 = default;
                this.flags = default;
            }
        }

        public static StyleProperty2 BackgroundColor(in Color color) {
            return new StyleProperty2(StylePropertyId.BackgroundColor, color);
        }

    }

    public struct RunCommand {

        public RunCommandType type;
        public RunCommandState state;
        public RunCommandAction action;
        public NativeArray<char> cmd;

    }

    public enum RunCommandAction {

        Start,
        Pause,
        Stop,
        Resume

    }

    public enum RunCommandState {

        Enter,
        Exit

    }

    public enum RunCommandType {

        Sound,
        Event,
        Animation,
        Transition,
        Selector

    }

}