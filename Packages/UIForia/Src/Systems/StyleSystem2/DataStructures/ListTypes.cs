using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Style;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    // Because C# won't let me have generic pointers, this is the next best thing. 
    // For each type that we need a list of (there shouldn't be that many) we create
    // a new struct definition that aligns exactly with the ListInterface fields
    // the array type should be typed in the list struct and struct.listInterface 
    // should return an exact field alignment.

    public unsafe struct ListDebugView<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T[] array;

        public ListDebugView(ListInterface list) {
            this.size = list.size;
            this.capacity = list.capacity;
            this.array = DebugUtil.PointerListToArray((T*) list.array, list.size);
        }

        public ListDebugView(List_ElementId target) : this(target.GetListInterface()) { }

        public ListDebugView(List_AttributeInfo target) : this(target.GetListInterface()) { }

        public ListDebugView(List_SelectorTypeIndex target) : this(target.GetListInterface()) { }

    }

    public interface IListInterface {

        ref ListInterface GetListInterface();

        int ItemSize { get; }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ListInterface {

        [FieldOffset(0)] public void* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(ListDebugView<ElementId>))]
    public unsafe struct List_ElementId : IListInterface {

        [FieldOffset(0)] public ElementId* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

        public ref ListInterface GetListInterface() {
            void* x = UnsafeUtility.AddressOf(ref this);
            return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
        }

        public int ItemSize {
            get => sizeof(ElementId);
        }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(ListDebugView<InstanceStyleProperty>))]
    public unsafe struct List_InstanceStyleProperty : IListInterface {

        [FieldOffset(0)] public InstanceStyleProperty* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

        public ref ListInterface GetListInterface() {
            void* x = UnsafeUtility.AddressOf(ref this);
            return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
        }

        public int ItemSize {
            get => sizeof(InstanceStyleProperty);
        }

    }
    
    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(ListDebugView<StyleIndexUpdate>))]
    public unsafe struct List_StyleIndexUpdate : IListInterface {

        [FieldOffset(0)] public StyleIndexUpdate* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

        public ref ListInterface GetListInterface() {
            void* x = UnsafeUtility.AddressOf(ref this);
            return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
        }

        public int ItemSize {
            get => sizeof(StyleIndexUpdate);
        }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(ListDebugView<AttributeInfo>))]
    public unsafe struct List_AttributeInfo : IListInterface {

        [FieldOffset(0)] public AttributeInfo* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

        public ref ListInterface GetListInterface() {
            void* x = UnsafeUtility.AddressOf(ref this);
            return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
        }

        public int ItemSize {
            get => sizeof(AttributeInfo);
        }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(ListDebugView<StyleId>))]
    public unsafe struct List_StyleId : IListInterface {

        [FieldOffset(0)] public StyleId* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

        public ref ListInterface GetListInterface() {
            void* x = UnsafeUtility.AddressOf(ref this);
            return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
        }

        public int ItemSize {
            get => sizeof(StyleId);
        }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(ListDebugView<SelectorTypeIndex>))]
    public unsafe struct List_SelectorTypeIndex : IListInterface {

        [FieldOffset(0)] public SelectorTypeIndex* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

        public ref ListInterface GetListInterface() {
            void* x = UnsafeUtility.AddressOf(ref this);
            return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
        }

        public int ItemSize {
            get => sizeof(SelectorTypeIndex);
        }

    }

}