using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Systems;
using UIForia.Util;
using UnityEditorInternal.Profiling.Memory.Experimental;

namespace UIForia.Elements {

    public struct RepeatItemKey {

        public readonly string keyString;
        public readonly long keyLong;

        public RepeatItemKey(long keyLong) {
            this.keyString = null;
            this.keyLong = keyLong;
        }

        public RepeatItemKey(int keyInt) {
            this.keyString = null;
            this.keyLong = keyInt;
        }

        public RepeatItemKey(string keyString) {
            this.keyLong = 0;
            this.keyString = keyString;
        }

        public static bool operator ==(RepeatItemKey a, RepeatItemKey b) {
            return a.keyLong == b.keyLong && string.Equals(a.keyString, b.keyString, StringComparison.Ordinal);
        }

        public static bool operator !=(RepeatItemKey a, RepeatItemKey b) {
            return a.keyLong != b.keyLong || !string.Equals(a.keyString, b.keyString, StringComparison.Ordinal);
        }

        public bool Equals(RepeatItemKey other) {
            return keyString == other.keyString && keyLong == other.keyLong;
        }

        public override bool Equals(object obj) {
            return obj is RepeatItemKey other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return ((keyString != null ? keyString.GetHashCode() : 0) * 397) ^ keyLong.GetHashCode();
            }
        }

    }

    public abstract class UIRepeatElement : UIElement {

        public int templateSpawnId;
        public int indexVarId;
        public UIElement templateContextRoot;
        public TemplateScope scope;
        public int itemVarId;

        protected void CreateFromRange(int start, int end) {
            for (int i = start; i < end; i++) {
                UIElement child = application.CreateTemplate(templateSpawnId, templateContextRoot, this, scope);
                application.InsertChild(this, child, (uint) i);
                ContextVariable<int> indexVariable = new ContextVariable<int>(indexVarId, "index", i);
                children[i].bindingNode.CreateLocalContextVariable(indexVariable);
            }
        }

        protected void DestroyAll() {
            while (children.size > 0) {
                children.Last.Destroy();
            }
        }

    }

    public sealed class UIRepeatCountElement : UIRepeatElement {

        public int count;

        [OnPropertyChanged(nameof(count))]
        public void OnCountChanged(int oldCount) {
            if (count > oldCount) {
                CreateFromRange(oldCount, count);
            }
            else {
                int diff = oldCount - count;
                for (int i = 0; i < diff; i++) {
                    children.Last.Destroy();
                }
            }
        }

    }

    [GenericElementTypeResolvedBy(nameof(list))]
    public sealed class UIRepeatElement<T> : UIRepeatElement {

        public IList<T> list;
        private IList<T> previousList;
        private int previousSize;
        public Func<T, RepeatItemKey> keyFn;
        private bool skipUpdate;

        private T[] listClone;


        [OnPropertyChanged(nameof(list))]
        public void OnListChanged(IList<T> oldList) {
            skipUpdate = true;
            // can skip update when list changed (I think this is safe, confirm!)
            if (oldList == null) {
                CreateFromRange(0, list.Count);
                for (int i = 0; i < children.size; i++) {
                    ContextVariable<T> variable = new ContextVariable<T>(8, "item", list[i]);
                    children[i].bindingNode.CreateLocalContextVariable(variable);
                }
            }
            else if (list == null) {
                DestroyAll();
            }
            else { }
        }

        public override void OnUpdate() {
            if (skipUpdate) {
                skipUpdate = false;
                // CloneList();
                return;
            }

            if (keyFn == null) {
                UpdateWithoutKeyFunc();
            }
            else {
                UpdateWithKeyFunc();
            }

            // CloneList();
        }

        private RepeatIndex[] keys;

        private struct RepeatIndex {

            public UIElement element;
            public RepeatItemKey key;

        }
        
        private void UpdateWithKeyFunc() {
            for (int i = 0; i < children.size; i++) {
                RepeatItemKey key = keyFn(list[i]);
                // for each item in the list, compute a key for it
                // key should be unique but maybe we don't enforce this

                // for each key we compute, if we had that key before we need to get the element and insert it at that index

                // children[i] = GetElementForKey(key);
                
                if (keys[i].key != key) {
                    keys[i].key = key;
                    keys[i].element = children[i];
                }        
                
            }

            for (int i = 0; i < children.size; i++) {
                children.array[i].bindingNode.SetRepeatItem(8, "item", list[i]);
            }
        }

        private void UpdateWithoutKeyFunc() {
            for (int i = 0; i < children.size; i++) {
                children.array[i].bindingNode.SetRepeatItem(8, "item", list[i]);
            }
        }

        private void CloneList() {
            if (listClone == null) {
                listClone = new T[list.Count];
            }

            if (listClone.Length < list.Count) {
                listClone = new T[list.Count];
            }

            switch (list) {
                case T[] array:
                    Array.Copy(array, 0, listClone, 0, array.Length);
                    break;
                case List<T> actualList: {
                    T[] a = actualList.GetArray();
                    Array.Copy(a, 0, listClone, 0, actualList.Count);
                    break;
                }
                default: {
                    for (int i = 0; i < list.Count; i++) {
                        listClone[i] = list[i];
                    }

                    break;
                }
            }
        }

    }

}