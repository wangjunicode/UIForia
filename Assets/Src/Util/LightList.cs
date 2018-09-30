using System;
using UnityEngine;

namespace Src.Util {

    public struct LightList<T> {

        private int size;
        private T[] list;

        public int Count => size;
        public bool IsReadOnly => false;

        public T[] List => list;
        
        public static LightList<T> Create(int size) {
            LightList<T> retn = new LightList<T>();
            retn.list = ArrayPool<T>.GetMinSize(size);
            retn.size = 0;
            return retn;
        }
        
        public void Add(T item) {
            if (size + 1 > list.Length) {
                ArrayPool<T>.Resize(ref list, (size + 1) * 2);
            }

            list[size] = item;
            size++;
        }

        public void Clear() {
            Array.Clear(list, 0, size);
            size = 0;
        }

        public bool Contains(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) return true;
            }

            return false;
        }

        public bool Remove(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) {
                    for (int j = i; j < size; j++) {
                        list[j] = list[j + 1];
                    }

                    list[size - 1] = default(T);
                    size--;
                    return true;
                }
            }

            return false;
        }



        public int IndexOf(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) return i;
            }

            return -1;
        }

        public void Insert(int index, T item) {
            Add(item);
            index = Mathf.Clamp(index, 0, size);
            for (int i = index; i < size; i++) {
                list[i + 1] = list[i];
            }

            list[index] = list[size - 1];
        }

        public void RemoveAt(int index) {
            if ((uint)index >= (uint)size) return;
            for (int j = index; j < size; j++) {
                list[j] = list[j + 1];
            }

            list[size - 1] = default(T);
            size--;
        }

        public T this[int index] {
            get { return list[index]; }
            set { list[index] = value; }
        }

    }

}