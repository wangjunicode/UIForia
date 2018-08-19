using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Src {

    public interface IHierarchical {

        IHierarchical Element { get; }
        IHierarchical Parent { get; }

    }

    public interface ISkipTreeTraversable : IHierarchical {

        void OnParentChanged(ISkipTreeTraversable newParent);
        void OnBeforeTraverse();
        void OnAfterTraverse();

    }

    public class SkipTree<T> where T : ISkipTreeTraversable {

        private SkipTreeNode<T> root;
        private Dictionary<IHierarchical, SkipTreeNode<T>> nodeMap;

        public SkipTree() {
            root = new SkipTreeNode<T>(default(T));
            nodeMap = new Dictionary<IHierarchical, SkipTreeNode<T>>();
            nodeMap[root.element] = root;
        }

        public int Size => nodeMap.Count;

        public void AddItem(T item) {
            SkipTreeNode<T> node;
            IHierarchical element = item.Element;
            if (nodeMap.TryGetValue(element, out node)) {
                return;
            }
            node = new SkipTreeNode<T>(item);
            nodeMap[element] = node;
            SkipTreeNode<T> parent = FindParent(element);
            Insert(parent == null ? root : parent, node);
        }

        public T GetItem<U>(U key) where U : IHierarchical {
            SkipTreeNode<T> node;
            if (nodeMap.TryGetValue(key, out node)) {
                return node.element;
            }
            return default(T);
        }

        public int GetSiblingIndex(T element) {
            SkipTreeNode<T> node;
            if (!nodeMap.TryGetValue(element, out node)) {
                return -1;
            }
            if (node.parent == null) {
                return -1;
            }
            SkipTreeNode<T> ptr = node.parent.firstChild;
            int index = 0;
            while (ptr != node) {
                index++;
                ptr = ptr.nextSibling;
            }
            return index;
        }

        public void SetSiblingIndex(T element, int index) {
            // todo -- remove and insert
        }

        public int GetChildCount(T element) {
            SkipTreeNode<T> node;
            if (!nodeMap.TryGetValue(element, out node)) {
                return 0;
            }
            return node.childCount;
        }

        public int GetActiveChildCount(T element) {
            SkipTreeNode<T> node;
            if (!nodeMap.TryGetValue(element, out node)) {
                return 0;
            }
            SkipTreeNode<T> ptr = node.parent.firstChild;
            int count = 0;
            while (ptr != null) {
                if (!ptr.isDisabled) {
                    count++;
                }
                ptr = ptr.nextSibling;
            }
            return count;
        }

        public void RemoveItem(T item) {
            SkipTreeNode<T> node;
            IHierarchical element = item.Element;
            if (!nodeMap.TryGetValue(element, out node)) {
                return;
            }

            SkipTreeNode<T> parent = node.parent;
            SkipTreeNode<T> ptr = node.firstChild;
            SkipTreeNode<T> nodeNext = node.nextSibling;
            SkipTreeNode<T> nodePrev = FindPreviousSibling(node);
            SkipTreeNode<T> lastChild = null;

            while (ptr != null) {
                ptr.parent = node.parent;
                node.parent.childCount++;
                ptr.element.OnParentChanged(ptr.parent.element);
                lastChild = ptr;
                ptr = ptr.nextSibling;
            }

            if (parent.firstChild == node) {
                parent.firstChild = node.firstChild;
            }
            else {
                nodePrev.nextSibling = node.firstChild;
                if (lastChild != null) {
                    lastChild.nextSibling = nodeNext;
                }
            }

            node.parent = null;
            node.element = default(T);
            node.nextSibling = null;
            node.firstChild = null;
            nodeMap.Remove(element);
        }

        public void EnableHierarchy(T item) {
            SkipTreeNode<T> node;
            IHierarchical element = item.Element;
            if (!nodeMap.TryGetValue(element, out node)) {
                return;
            }
            node.isDisabled = false;
        }

        public void DisableHierarchy(T item) {
            SkipTreeNode<T> node;
            IHierarchical element = item.Element;
            if (!nodeMap.TryGetValue(element, out node)) {
                return;
            }
            node.isDisabled = true;
        }

        public void RemoveHierarchy(T item) {
            SkipTreeNode<T> node;
            IHierarchical element = item.Element;
            if (!nodeMap.TryGetValue(element, out node)) {
                return;
            }

            SkipTreeNode<T> ptr = node;
            SkipTreeNode<T> nodeNext = node.nextSibling;
            SkipTreeNode<T> nodePrev = FindPreviousSibling(node);

            if (nodePrev != null) {
                nodePrev.nextSibling = nodeNext;
            }

            Stack<SkipTreeNode<T>> stack = new Stack<SkipTreeNode<T>>();

            stack.Push(ptr);

            while (stack.Count > 0) {

                SkipTreeNode<T> current = stack.Pop();

                nodeMap.Remove(current.element);

                ptr = current.firstChild;

                while (ptr != null) {
                    stack.Push(ptr);
                    ptr = ptr.nextSibling;
                }

            }

        }

        private SkipTreeNode<T> FindPreviousSibling(SkipTreeNode<T> node) {
            SkipTreeNode<T> ptr = node.parent.firstChild;
            while (ptr != null) {
                if (ptr.nextSibling == node) {
                    return ptr;
                }
                ptr = ptr.nextSibling;
            }
            return null;
        }

        private void Insert(SkipTreeNode<T> parent, SkipTreeNode<T> inserted) {

            inserted.parent = parent;
            parent.childCount++;
            inserted.element.OnParentChanged(parent.element);

            SkipTreeNode<T> ptr = parent.firstChild;
            ISkipTreeTraversable element = inserted.element;

            // set parent
            // walk through current parent's children
            // if any of those are decsendents of inserted
            // remove from parent
            // attach as first sibling to inserted

            SkipTreeNode<T> insertedLastChild = null;
            SkipTreeNode<T> parentPreviousChild = null;

            while (ptr != null) {
                ISkipTreeTraversable currentElement = ptr.element;
                if (IsDescendentOf(currentElement, element)) {

                    SkipTreeNode<T> next = ptr.nextSibling;

                    if (ptr == parent.firstChild) {
                        parent.firstChild = next;
                    }
                    else if (parentPreviousChild != null) {
                        parentPreviousChild.nextSibling = next;
                    }

                    if (insertedLastChild != null) {
                        insertedLastChild.nextSibling = ptr;
                    }
                    else {
                        inserted.firstChild = ptr;
                    }

                    ptr.parent.childCount--;
                    ptr.parent = inserted;
                    inserted.childCount++;
                    ptr.element.OnParentChanged(inserted.element);
                    ptr.nextSibling = null;
                    insertedLastChild = ptr;
                    ptr = next;
                }
                else {
                    parentPreviousChild = ptr;
                    ptr = ptr.nextSibling;
                }
            }
            inserted.nextSibling = parent.firstChild;
            parent.firstChild = inserted;
        }

        public void TraversePreOrderWithCallback(Action<T> traverseFn) {

            SkipTreeNode<T> ptr = root.firstChild;
            Stack<SkipTreeNode<T>> stack = new Stack<SkipTreeNode<T>>();

            while (ptr != null) {
                if (!ptr.isDisabled) {
                    stack.Push(ptr);
                }
                ptr = ptr.nextSibling;
            }

            while (stack.Count > 0) {

                SkipTreeNode<T> current = stack.Pop();

                traverseFn(current.element);

                ptr = current.firstChild;

                while (ptr != null) {
                    if (!ptr.isDisabled) {
                        stack.Push(ptr);
                    }
                    ptr = ptr.nextSibling;
                }

            }

        }

        public void TraverseRecursePreOrder() {

            SkipTreeNode<T> ptr = root.firstChild;

            while (ptr != null) {
                TraverseRecursePreorderStep(ptr);
                ptr = ptr.nextSibling;
            }

        }

        private void TraverseRecursePreorderStep(SkipTreeNode<T> node) {

            if (node.isDisabled) return;

            node.element.OnBeforeTraverse();

            SkipTreeNode<T> ptr = node.firstChild;
            while (ptr != null) {
                TraverseRecursePreorderStep(ptr);
                ptr = ptr.nextSibling;
            }

            node.element.OnAfterTraverse();

        }

        private SkipTreeNode<T> FindParent(IHierarchical element) {
            IHierarchical ptr = element.Parent;
            while (ptr != null) {
                SkipTreeNode<T> node;
                if (nodeMap.TryGetValue(ptr, out node)) {
                    return node;
                }
                ptr = ptr.Parent;
            }
            return null;
        }

        private static bool IsDescendentOf(IHierarchical child, ISkipTreeTraversable parent) {
            IHierarchical ptr = child;
            while (ptr != null) {
                if (ptr == parent.Element) {
                    return true;
                }
                ptr = ptr.Parent;
            }
            return false;
        }

        [DebuggerDisplay("{element}")]
        private class SkipTreeNode<U> {

            public U element;
            public bool isDisabled;
            public SkipTreeNode<U> parent;
            public SkipTreeNode<U> nextSibling;
            public SkipTreeNode<U> firstChild;
            public int childCount;

            public SkipTreeNode(U element) {
                this.element = element;
                this.isDisabled = false;
            }

        }

    }

}