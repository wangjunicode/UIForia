using System;
using UnityEngine;

namespace UIForia.Util {

    public class PagedStructList<T> where T : struct {

        private ListPage<T> tail;
        private readonly int pageSize;

        public readonly ListPage<T> head;
        public int size { get; private set; }

        public PagedStructList(int pageSize = 16) {
            this.pageSize = Mathf.Max(8, pageSize);
            this.head = new ListPage<T>(new T[pageSize]);
            this.tail = head;
        }

        public void Add(in T val) {
            if (tail.size >= tail.data.Length) {
                ListPage<T> old = tail;
                tail = new ListPage<T>(new T[pageSize]);
                old.next = tail;
            }

            size++;
            tail.data[tail.size++] = val;
        }

        public void Clear() {
            size = 0;
            ListPage<T> ptr = head;
            while (ptr != null) {
                ptr.size = 0;
                ptr = ptr.next;
            }
        }

        public void ForEach<U>(U closure, Action<U, T[], int> action) {
            ListPage<T> ptr = head;
            while (ptr != null) {
                action(closure, ptr.data, ptr.size);
                ptr = ptr.next;
            }
        }

        public void ForEach<U>(U closure, Func<U, T[], int, bool> action) {
            ListPage<T> ptr = head;
            while (ptr != null) {
                if (!action(closure, ptr.data, ptr.size)) {
                    break;
                }

                ptr = ptr.next;
            }
        }

        public void ForEach(Action<T[], int> action) {
            ListPage<T> ptr = head;
            while (ptr != null) {
                action(ptr.data, ptr.size);
                ptr = ptr.next;
            }
        }

        public void ForEachInRange(RangeInt range, Action<T[], int, int> action) {
            throw new NotImplementedException();
            // ListPage<T> ptr = head;
            // int start = range.start / pageSize;

            // while (ptr != null) {
                // action(ptr.data, ptr.size);
                // ptr = ptr.next;
            // }
        }

        public T[] ToArray() {
            T[] retn = new T[size];
            ListPage<T> ptr = head;
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