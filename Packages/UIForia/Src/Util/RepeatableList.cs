using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UIForia.Extensions;

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
        public event Action<T, int, int> onItemMoved; // todo
        public event Action onClear;

        private readonly IList<T> backingStore;
        
        public RepeatableList() {
            backingStore =  new List<T>();
        }
        
        public RepeatableList(IList<T> list = null) {
            backingStore = list == null ? new List<T>() : new List<T>(list);
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

        public void RemoveAt(int index) {
            if (index < 0 || index >= backingStore.Count) {
                return;
            }
            onItemRemoved?.Invoke(backingStore[index], index);
            backingStore.RemoveAt(index);
        }

        public T Find(Predicate<T> fn) {
            return ((List<T>) backingStore).Find(fn);
        }

        public T this[int index] {
            get => backingStore[index];
            set => backingStore[index] = value;
        }      

    }

}