using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

   
    public unsafe struct IntListMap<TValue> : IDisposable where TValue : unmanaged {

        [NativeDisableUnsafePtrRestriction] private readonly UntypedIntMap* mapState;

        public IntListMap(UntypedIntMap* mapState) {
            this.mapState = mapState->GetItemSize() == sizeof(ListHandle)
                ? mapState
                : default;
        }

        public IntListMap(int initialCapacity, Allocator allocator, float fillFactor = 0.8f) {
            this.mapState = UntypedIntMap.Create<TValue>(initialCapacity, fillFactor, allocator);
        }

        public void Dispose() {
            if (mapState != null) {
                mapState->Dispose();
            }

            this = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int key, in TypedListHandle<TValue> value) {
            mapState->Add<ListHandle>(key, *value.listHandle, out ListHandle* _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int key, in ListHandle value) {
            mapState->Add<ListHandle>(key, value, out ListHandle* _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypedListHandle<TValue> GetOrCreate(int key) {
            return new TypedListHandle<TValue>(mapState->GetOrCreate<ListHandle>(key));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out TypedListHandle<TValue> value) {
            bool result = mapState->TryGetPointer<ListHandle>(key, out ListHandle* ptr);
            value = new TypedListHandle<TValue>(ptr);
            return result;
        }

        // public bool TryGetReference(int key, out TValue* value) {
        //     return mapState->TryGetPointer<TValue>(key, out value);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key) {
            return mapState->Remove<ListHandle>(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(int key, out ListHandle oldValue) {
            if (mapState->TryRemove<ListHandle>(key, out oldValue)) {
                return true;
            }

            oldValue = default;
            return false;

        }

        public int size {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mapState->GetSize();
        }

    }

}