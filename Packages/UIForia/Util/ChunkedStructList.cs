using System;
using UnityEngine;

namespace UIForia.Util {

    public class ChunkedStructList<T> where T : struct {

        private StructListChunk tail;
        private readonly int pageSize;

        public readonly StructListChunk head;
        public int size { get; private set; }

        public ChunkedStructList(int pageSize = 16) {
            this.pageSize = Mathf.Max(8, pageSize);
            this.head = new StructListChunk(new T[pageSize]);
            this.tail = head;
        }

        public void Add(in T val) {
            if (tail.size >= tail.data.Length) {
                StructListChunk old = tail;
                tail = new StructListChunk(new T[pageSize]);
                old.next = tail;
            }

            size++;
            tail.data[tail.size++] = val;
        }

        public void Clear() {
            size = 0;
            StructListChunk ptr = head;
            while (ptr != null) {
                ptr.size = 0;
                ptr = ptr.next;
            }
        }

        public void ForEach<U>(U closure, Action<U, T[], int> action) {
            StructListChunk ptr = head;
            while (ptr != null) {
                action(closure, ptr.data, ptr.size);
                ptr = ptr.next;
            }
        }

        public void ForEach<U>(U closure, Func<U, T[], int, bool> action) {
            StructListChunk ptr = head;
            while (ptr != null) {
                if (!action(closure, ptr.data, ptr.size)) {
                    break;
                }

                ptr = ptr.next;
            }
        }

        public void ForEach(Action<T[], int> action) {
            StructListChunk ptr = head;
            while (ptr != null) {
                action(ptr.data, ptr.size);
                ptr = ptr.next;
            }
        }

        public void ForEachInRange(RangeInt range, Action<T[], int, int> action) {
            StructListChunk ptr = head;
            int start = range.start / pageSize;

            while (ptr != null) {
                // action(ptr.data, ptr.size);
                ptr = ptr.next;
            }
        }

        public class StructListChunk {

            public int size;
            public T[] data;
            public StructListChunk next;

            public StructListChunk(T[] data) {
                this.data = data;
                this.size = 0;
            }

        }

        public T[] ToArray() {
            T[] retn = new T[size];
            StructListChunk ptr = head;
            int idx = 0;
            while (ptr != null) {
                Array.Copy(ptr.data, 0, retn, idx, ptr.size);
                idx += ptr.size;
                ptr = ptr.next;
            }

            return retn;
        }

    }

}