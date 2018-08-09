using System;
using System.Collections.Generic;

namespace Src {

    public interface IHierarchical {

        IHierarchical Parent { get; }

    }

    public class SkipTree<T> {

        private List<SkipTreeNode<T>> roots;
        private Dictionary<IHierarchical, SkipTreeNode<T>> nodeMap;

        public SkipTree() {
            roots = new List<SkipTreeNode<T>>();
            nodeMap = new Dictionary<IHierarchical, SkipTreeNode<T>>();
        }

        public void AddItem(IHierarchical key, T item) {
            SkipTreeNode<T> node;
            if (nodeMap.TryGetValue(key, out node)) {
                node.values.Add(item);
                return;
            }
            node = CreateNode(key);
            node.values.Add(item);
        }

        public void RemoveItem(IHierarchical key, T item) {
            SkipTreeNode<T> node;
            if (!nodeMap.TryGetValue(key, out node)) {
                return;
            }
            node.values.Remove(item);
            if (node.values.Count == 0) {
                RemoveNode(node);
            }
        }

        public void RemoveNode(IHierarchical element) {
            SkipTreeNode<T> node;
            if (!nodeMap.TryGetValue(element, out node)) {
                return;
            }
            if (roots.Contains(node)) {
                roots.AddRange(node.children);
            }
            else {
                SkipTreeNode<T> parent = FindParent(element);
                parent.children.AddRange(node.children);
            }
            nodeMap.Remove(element);
            node.children = null;
            node.values = null;
            node.element = null;
        }

        // todo -- handle orphans, handle pooling 
        public void RemoveNodeTree(IHierarchical element) {
            SkipTreeNode<T> node;
            if (!nodeMap.TryGetValue(element, out node)) {
                return;
            }
            SkipTreeNode<T> parent = FindParent(element);
            if (parent != null) {
                parent.children.Remove(node);
            }
            else {
                roots.Remove(node);
            }
        }

        // maybe have a method to traverse skip tree and return culled sub trees in a list

        public void Traverse(Action<IHierarchical> traverseFn) { }

        public void TraverseRemove(Func<IHierarchical, bool> traverseFn) {
            for (int i = 0; i < roots.Count; i++) { }
        }

        public void TraverseCull(Func<IHierarchical, bool> traverseFn) {
            TraverseCullInternal(roots, traverseFn);
        }

        // todo this has to be a depth first traversal, maybe provide 2 methods, one for depth one for breadth
        private void TraverseCullInternal(List<SkipTreeNode<T>> list, Func<IHierarchical, bool> traverseFn) {
            for (int i = 0; i < list.Count; i++) {
                if (traverseFn(list[i].element)) {
                    // note! not removing from node map yet
                    list.RemoveAt(i--);
                }
            }
            for (int i = 0; i < list.Count; i++) {
                TraverseCullInternal(list[i].children, traverseFn);
            }
        }

        private SkipTreeNode<T> CreateNode(IHierarchical element) {
            SkipTreeNode<T> node = new SkipTreeNode<T>(element);
            nodeMap[element] = node;
            SkipTreeNode<T> parent = FindParent(element);
            return Insert(parent == null ? roots : parent.children, node);
        }

        // if any nodes in the list are actually children of the new node
        // we need to replace the current item with the new node
        // and add the old item as a child of the new node
        private SkipTreeNode<T> Insert(List<SkipTreeNode<T>> list, SkipTreeNode<T> child) {
            // todo I think this is wrong, probably need to shuffle children who might be attached to parent but should now be attached to node
            for (int i = 0; i < list.Count; i++) {
                IHierarchical element = list[i].element;
                if (IsDescendentOf(element, child.element)) {
                    child.children.Add(list[i]);
                    list[i] = child;

                    return child;
                }
            }
            list.Add(child);
            return child;
        }

        private static bool IsDescendentOf(IHierarchical parent, IHierarchical child) {
            IHierarchical ptr = child;
            while (ptr != null) {
                if (ptr.Equals(parent)) return true;
                ptr = ptr.Parent;
            }
            return false;
        }

        private void RemoveNode(SkipTreeNode<T> node) {

            List<SkipTreeNode<T>> children = node.children;
            SkipTreeNode<T> newParent = FindParent(node.element);
            if (newParent != null) { }
            else { }
            nodeMap.Remove(node.element);
        }

        private SkipTreeNode<T> FindParent(IHierarchical element) {
            IHierarchical ptr = element;
            while (ptr != null) {
                SkipTreeNode<T> node;
                if (nodeMap.TryGetValue(ptr, out node)) {
                    return node;
                }
                ptr = ptr.Parent;
            }
            return null;
        }

        private class SkipTreeNode<T> {

            public IHierarchical element;
            public List<T> values;
            public List<SkipTreeNode<T>> children;

            public SkipTreeNode(IHierarchical element) {
                this.element = element;
                this.values = new List<T>();
                this.children = new List<SkipTreeNode<T>>();
            }

        }

    }

}