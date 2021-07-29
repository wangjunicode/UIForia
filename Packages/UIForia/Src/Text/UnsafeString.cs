using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Text {

    [DebuggerTypeProxy(typeof(UnsafeStringDebugView))]
    internal unsafe struct UnsafeString : IEquatable<UnsafeString> {

        public readonly char* str;
        public readonly int length;

        public UnsafeString(char* str, int length) {
            this.str = str;
            this.length = length;
        }

        public bool Equals(UnsafeString other) {
            return length == other.length && UnsafeUtility.MemCmp(str, other.str, sizeof(char) * length) == 0;
        }

        public override bool Equals(object obj) {
            return obj is UnsafeString tag && Equals(tag);
        }

        public override int GetHashCode() {
            return (int) CollectionHelper.Hash(str, length * 2);
        }

        public override string ToString() {
            return new string(str, 0, length);
        }

    }

    internal sealed unsafe class UnsafeStringDebugView {

        public string str;

        public UnsafeStringDebugView(UnsafeString str) {
            this.str = new string(str.str, 0, str.length);
        }

    }

}