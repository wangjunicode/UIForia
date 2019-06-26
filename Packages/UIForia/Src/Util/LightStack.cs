using System;
using UnityEngine;

namespace UIForia.Util {

    public class LightStack<T> {

        private T[] array;
        private int size;
        private bool isPooled;

        public int Count => size;
        public T[] Stack => array;
        
        public LightStack(int capacity = 8) {
            capacity = Mathf.Max(1, capacity);
            this.array = new T[capacity];
            this.size = 0;
            this.isPooled = false;
        }

        public void EnsureCapacity(int capacity) {
            if (array.Length <= capacity) {
                Array.Resize(ref array, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (size + capacity >= array.Length) {
                Array.Resize(ref array, size + capacity);
            }
        }

        public void Push(T item) {
            if (size + 1 >= array.Length) {
                Array.Resize(ref array, (size + 1) * 2);
            }

            array[size++] = item;
        }

        public T PeekAtUnchecked(int idx) {
            return array[idx];
        }
        
        public T Pop() {
            if (size == 0) return default;
            T obj = array[--size];
            array[size] = default;
            return obj;
        }

        public void PushUnchecked(T item) {
            array[size++] = item;
        }

        public T PopUnchecked() {
            T obj = array[--size];
            array[size] = default;
            return obj;
        }

        public void Clear() {
            Array.Clear(array, 0, size);
            size = 0;
        }

        private static readonly LightList<LightStack<T>> s_Pool = new LightList<LightStack<T>>();

        public static LightStack<T> Get() {
            LightStack<T> retn = s_Pool.Count > 0 ? s_Pool.RemoveLast() : new LightStack<T>();
            retn.isPooled = false;
            return retn;
        }

        public static void Release(ref LightStack<T> toPool) {
            Array.Clear(toPool.array, 0, toPool.size);
            toPool.size = 0;
            if (toPool.isPooled) return;
            toPool.isPooled = true;
            s_Pool.Add(toPool);
        }

        public T Peek() {
            if (size > 0) {
                return array[size - 1];
            }

            return default;
        }

        public T PeekUnchecked() {
            return array[size - 1];
        }

    }

}