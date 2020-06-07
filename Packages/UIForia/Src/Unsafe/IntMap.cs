using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct IntMap<TValue> : IDisposable where TValue : unmanaged {

        [NativeDisableUnsafePtrRestriction] private readonly UntypedIntMap* mapState;

        public IntMap(UntypedIntMap* mapState) {
            this.mapState = mapState->GetItemSize() == sizeof(TValue)
                ? mapState
                : default;
        }

        public IntMap(int initialCapacity, Allocator allocator, float fillFactor = 0.75f) {
            this.mapState = UntypedIntMap.Create<TValue>(initialCapacity, fillFactor, allocator);
        }

        public void Dispose() {
            if (mapState != null) {
                mapState->Dispose();
            }

            this = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int key, in TValue value) {
            mapState->Add<TValue>(key, value, out TValue* _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue* GetOrCreate(int key) {
            return mapState->GetOrCreate<TValue>(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreateReference(int key) {
            TValue* ptr = mapState->GetOrCreate<TValue>(key);
            return ref UnsafeUtilityEx.AsRef<TValue>(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out TValue value) {
            return mapState->TryGetValue<TValue>(key, out value);
        }

        // ReSharper disable once RedundantAssignment
        public bool TryGetReference(int key, ref TValue value) {
            if (mapState->TryGetPointer(key, out TValue* ptr)) {
                value = UnsafeUtilityEx.AsRef<TValue>(ptr);
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetPointer(int key, out TValue* value) {
            return mapState->TryGetPointer<TValue>(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key) {
            return mapState->Remove<TValue>(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(int key, out TValue oldValue) {
            return mapState->TryRemove<TValue>(key, out oldValue);
        }

        public int size {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mapState->GetSize();
        }

        public TValue GetOrDefault(int key) {
            if (mapState->TryGetValue<TValue>(key, out TValue val)) {
                return val;
            }

            return default;
        }

        public bool ContainsKey(int key) {
            return mapState->TryGetValue<TValue>(key, out _);
        }

        public UntypedIntMap* GetState() {
            return mapState;
        }

    }

}