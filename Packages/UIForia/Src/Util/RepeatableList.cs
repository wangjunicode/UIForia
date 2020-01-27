using System;
using System.Collections;
using System.Collections.Generic;

namespace UIForia.Util {

    public interface IRepeatableList {}

    public interface IRepeatableList<T> : IList<T>, IRepeatableList {

        event Action<T, int> onItemInserted;
        event Action<T, int> onItemRemoved;
        event Action<T, int, int> onItemMoved;
        event Action onClear;

    }
        
    public class RepeatableList<T> : IList<T>, IRepeatableList {
        
        public event Action<T, int> onItemInserted;
        public event Action<T, int> onItemRemoved;
      //  public event Action<T, int, int> onItemMoved; // todo
        public event Action onClear;

        private readonly IList<T> backingStore;
        
        public RepeatableList() {
            backingStore =  new LightList<T>();
        }
        
        public RepeatableList(IList<T> list = null) {
            backingStore = list == null ? new LightList<T>() : new LightList<T>(list);
        }
        
        public bool IsReadOnly => false;
        public int Count => backingStore.Count;
        
        public IEnumerator<T> GetEnumerator() {
            return backingStore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            backingStore.Add(item);
            onItemInserted?.Invoke(item, backingStore.Count - 1);
        }

        public void Clear() {
            backingStore.Clear();
            onClear?.Invoke();
        }

        public bool Contains(T item) {
            return backingStore.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            backingStore.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            int index = backingStore.IndexOf(item);
            if (index == -1) return false;
            onItemRemoved?.Invoke(item, index);
            backingStore.RemoveAt(index);
            return true;
        }
        
        public int IndexOf(T item) {
            return backingStore.IndexOf(item);
        }

        public void Insert(int index, T item) {
            onItemInserted?.Invoke(item, index);
            backingStore.Insert(index, item);
        }

        public void Upsert(int index, T item) {
            if (index < Count) {
                backingStore[index] = item;
            }
            else {
                Insert(index, item);
            }
        }

        public void ReplaceList(List<T> list) {
            for (int i = 0; i < list.Count; i++) {
                Upsert(i, list[i]);
            }

            if (Count > list.Count) {
                int sizeDifference = Count - list.Count;
                RemoveRange(Count - sizeDifference, sizeDifference);
            }
        }

        public void RemoveAt(int index) {
            if (index < 0 || index >= backingStore.Count) {
                return;
            }

            T item = backingStore[index];
            backingStore.RemoveAt(index);
            onItemRemoved?.Invoke(item, index);
        }

        public T Find(Predicate<T> fn) {
            for (int i = 0; i < backingStore.Count; i++) {
                if (fn.Invoke(backingStore[i])) {
                    return backingStore[i];
                }
            }
            return default;
        }

        public T this[int index] {
            get => backingStore[index];
            set => backingStore[index] = value;
        }

        public void RemoveRange(int index, int count) {
            for (int i = index + count - 1; i >= index; i--) {
                T item = backingStore[i];
                backingStore.RemoveAt(i);
                onItemRemoved?.Invoke(item, i);
            }
        }

        public void Move(int oldIndex, int insertIndex) {
            if (insertIndex == -1) {
                insertIndex = Count - 1;
            }

            if ((uint) oldIndex >= Count) return;
            if ((uint) insertIndex >= Count) return;
            T item = backingStore[oldIndex];
            RemoveAt(oldIndex);
            if (insertIndex >= Count) {
                Add(item);
            } else {
                Insert(insertIndex, item);
            }
        }
    }

}