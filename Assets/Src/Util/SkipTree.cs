﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;

namespace Src {

    public interface IHierarchical {

        int UniqueId { get; }
        IHierarchical Element { get; }
        IHierarchical Parent { get; }

    }

    // specify node type? ie extend SkipTreeNode
    // better ToList / ToArray support w/o allocation
    // maintain enabled count?
    // todo allow IHierarchical keys instead of / in addition to T type keys

    // todo -- method to recycle tree node arrays
    public class SkipTree<T> where T : class, IHierarchical {

        public enum TreeChangeType {

            ItemAdded,
            ItemRemoved,
            HierarchyEnabled,
            HierarchyDisabled,
            HierarchyRemoved,
            Cleared

        }

        public delegate void TreeChanged(TreeChangeType changeType);

        public delegate void ParentChanged(T child, T newParent, T oldParent);

        private readonly SkipTreeNode root;
        private readonly Dictionary<int, SkipTreeNode> nodeMap;
        private static readonly List<T> scratchList = new List<T>();
        private static readonly List<SkipTreeNode> scratchNodeList = new List<SkipTreeNode>();

        public event TreeChanged onTreeChanged;
        public event ParentChanged onItemParentChanged;

        public SkipTree() {
            root = new SkipTreeNode(default(T));
            nodeMap = new Dictionary<int, SkipTreeNode>();
        }

        [PublicAPI]
        public int Size => nodeMap.Count;

        [PublicAPI]
        public T[] GetRootItems() {
            T[] retn = new T[root.childCount];
            GetRootItems(ref retn);
            return retn;
        }

        [PublicAPI]
        public void GetRootItems(IList<T> roots) {
            SkipTreeNode ptr = root.firstChild;
            while (ptr != null) {
                roots.Add(ptr.item);
                ptr = ptr.nextSibling;
            }
        }

        [PublicAPI]
        public int GetRootItems(ref T[] roots) {
            if (roots.Length < root.childCount) {
                Array.Resize(ref roots, root.childCount);
            }

            int i = 0;
            SkipTreeNode ptr = root.firstChild;
            while (ptr != null) {
                roots[i] = ptr.item;
                i++;
                ptr = ptr.nextSibling;
            }

            return root.childCount;
        }

        [PublicAPI]
        public void AddItem(T item) {
            SkipTreeNode node;
            IHierarchical element = item.Element;
            if (nodeMap.TryGetValue(element.UniqueId, out node)) {
                return;
            }

            node = new SkipTreeNode(item);
            nodeMap[element.UniqueId] = node;
            SkipTreeNode parent = FindParent(element);
            Insert(parent ?? root, node);
            onTreeChanged?.Invoke(TreeChangeType.ItemAdded);
        }

        [PublicAPI]
        public T GetItem<U>(U key) where U : class, IHierarchical {
            SkipTreeNode node;

            if (nodeMap.TryGetValue(key.UniqueId, out node)) {
                return node.item;
            }

            return default(T);
        }

        [PublicAPI]
        public T GetItem(int key) {
            SkipTreeNode node;

            if (nodeMap.TryGetValue(key, out node)) {
                return node.item;
            }

            return default(T);
        }

        [PublicAPI]
        public int GetSiblingIndex(T element) {
            SkipTreeNode node;
            if (!nodeMap.TryGetValue(element.UniqueId, out node)) {
                return -1;
            }

            if (node.parent == null) {
                return -1;
            }

            SkipTreeNode ptr = node.parent.firstChild;
            int index = 0;
            while (ptr != node) {
                index++;
                ptr = ptr.nextSibling;
            }

            return node.parent.childCount - index - 1;
        }

        [PublicAPI]
        public void SetSiblingIndex(T element, int index) {
            SkipTreeNode node;
            if (!nodeMap.TryGetValue(element.UniqueId, out node)) {
                return;
            }

            if (node == root) return;

            int count = node.parent.childCount;

            if (index > count) index = count;

            // who list is inverted with respect the order in which 
            // items are added, we need to adjust indices to fix this
            index = count - 1 - index;


            if (node.parent.firstChild == node) {
                if (count > 1) {
                    node.parent.firstChild = node.nextSibling;
                }
                else {
                    return;
                }
            }
            else {
                SkipTreeNode prev = FindPreviousSibling(node);

                if (prev != null) {
                    prev.nextSibling = node.nextSibling;
                }
                else {
                    node.nextSibling = null;
                }
            }

            if (index <= 0) {
                node.nextSibling = node.parent.firstChild;
                node.parent.firstChild = node;
                return;
            }

            SkipTreeNode trail = null;
            SkipTreeNode ptr = node.parent.firstChild;
            int i = 0;
            while (ptr != null && i < index) {
                i++;
                trail = ptr;
                ptr = ptr.nextSibling;
            }

            if (i == index) {
                node.nextSibling = ptr;
                // ReSharper disable once PossibleNullReferenceException
                trail.nextSibling = node;
            }
            else {
                Debug.Assert(trail != null, nameof(trail) + " != null");
                trail.nextSibling = node;
            }
        }

        public void Clear() {
            nodeMap.Clear();
            root.childCount = 0;
            root.firstChild = null;
            root.nextSibling = null;
            onTreeChanged?.Invoke(TreeChangeType.Cleared);
        }

        [PublicAPI]
        public int GetChildCount(IHierarchical item) {
            SkipTreeNode node;
            if (!nodeMap.TryGetValue(item.UniqueId, out node)) {
                return 0;
            }

            return node.childCount;
        }

        [PublicAPI]
        public int GetActiveChildCount(IHierarchical element) {
            SkipTreeNode node;
            if (!nodeMap.TryGetValue(element.UniqueId, out node)) {
                return 0;
            }

            SkipTreeNode ptr = node.parent.firstChild;
            int count = 0;
            while (ptr != null) {
                count += !ptr.isDisabled ? 1 : 0;
                ptr = ptr.nextSibling;
            }

            return count;
        }

        [PublicAPI]
        public void RemoveItem(IHierarchical item) {
            SkipTreeNode node;
            IHierarchical element = item.Element;
            if (!nodeMap.TryGetValue(element.UniqueId, out node)) {
                return;
            }

            SkipTreeNode parent = node.parent;
            SkipTreeNode ptr = node.firstChild;
            SkipTreeNode nodeNext = node.nextSibling;
            SkipTreeNode nodePrev = FindPreviousSibling(node);
            SkipTreeNode lastChild = null;

            while (ptr != null) {
                ptr.parent = node.parent;
                node.parent.childCount++;
                onItemParentChanged?.Invoke(ptr.item, ptr.parent.item, node.parent.item);
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
            node.item = default(T);
            node.nextSibling = null;
            node.firstChild = null;
            nodeMap.Remove(element.UniqueId);
            onTreeChanged?.Invoke(TreeChangeType.ItemRemoved);
        }

        [PublicAPI]
        public void EnableHierarchy(IHierarchical item) {
            TraverseNodes(false, item, (isDisabled, n) => n.isDisabled = isDisabled, true);
            onTreeChanged?.Invoke(TreeChangeType.HierarchyEnabled);
        }

        [PublicAPI]
        public void DisableHierarchy(IHierarchical item) {
            TraverseNodes(true, item, (isDisabled, n) => n.isDisabled = isDisabled, false);
            onTreeChanged?.Invoke(TreeChangeType.HierarchyDisabled);
        }

        [PublicAPI]
        public void RemoveHierarchy(IHierarchical item) {
            SkipTreeNode node;
            SkipTreeNode ptr;
            IHierarchical element = item.Element;
            Stack<SkipTreeNode> stack = new Stack<SkipTreeNode>();

            if (!nodeMap.TryGetValue(element.UniqueId, out node)) {
                SkipTreeNode parent = FindParent(item);
                SkipTreeNode trail = null;
                parent = parent ?? root;
                ptr = parent.firstChild;
                while (ptr != null) {
                    if (IsDescendentOf(ptr.item, item)) {
                        if (ptr == parent.firstChild) {
                            parent.firstChild = ptr.nextSibling;
                        }
                        else if (trail != null) {
                            trail.nextSibling = ptr.nextSibling;
                        }

                        stack.Push(ptr);
                    }
                    else {
                        trail = ptr;
                    }

                    ptr = ptr.nextSibling;
                }
            }
            else {
                SkipTreeNode nodeNext = node.nextSibling;
                SkipTreeNode nodePrev = FindPreviousSibling(node);
                if (nodePrev != null) {
                    nodePrev.nextSibling = nodeNext;
                }

                stack.Push(node);
            }

            while (stack.Count > 0) {
                SkipTreeNode current = stack.Pop();
                nodeMap.Remove(current.item.UniqueId);
                AddChildrenToStack(stack, current, true);
            }

            onTreeChanged?.Invoke(TreeChangeType.HierarchyRemoved);
        }

        public void ConditionalTraversePreOrder(Func<T, bool> traverseFn) {
            ConditionalTraversePreOrderStep(root, traverseFn);
        }

        public void ConditionalTraversePreOrder(IHierarchical start, Func<T, bool> traverseFn) {
            SkipTreeNode node;
            if (nodeMap.TryGetValue(start.UniqueId, out node)) {
                ConditionalTraversePreOrderStep(node, traverseFn);
                return;
            }

            SkipTreeNode parent = FindParent(start);
            parent = parent ?? root;
            SkipTreeNode ptr = parent.firstChild;
            while (ptr != null) {
                if (IsDescendentOf(ptr.item, start)) {
                    if (traverseFn(ptr.item)) {
                        ConditionalTraversePreOrderStep(ptr, traverseFn);
                    }
                }

                ptr = ptr.nextSibling;
            }
        }

        public void ConditionalTraversePreOrder<U>(U closureArg, Func<T, U, bool> traverseFn) {
            ConditionalTraversePreOrderStep(root, closureArg, traverseFn);
        }

        public void ConditionalTraversePreOrder<U>(IHierarchical start, U closureArg, Func<T, U, bool> traverseFn) {
            if (start == null) {
                ConditionalTraversePreOrderStep(root, closureArg, traverseFn);
                return;
            }

            SkipTreeNode node;
            if (nodeMap.TryGetValue(start.UniqueId, out node)) {
                ConditionalTraversePreOrderStep(node, closureArg, traverseFn);
                return;
            }

            SkipTreeNode parent = FindParent(start);
            parent = parent ?? root;
            SkipTreeNode ptr = parent.firstChild;
            while (ptr != null) {
                if (IsDescendentOf(ptr.item, start)) {
                    if (traverseFn(ptr.item, closureArg)) {
                        ConditionalTraversePreOrderStep(ptr, closureArg, traverseFn);
                    }
                }

                ptr = ptr.nextSibling;
            }
        }

        private void ConditionalTraversePreOrderStep(SkipTreeNode node, Func<T, bool> traverseFn) {
            Stack<SkipTreeNode> stack = new Stack<SkipTreeNode>();
            AddChildrenToStack(stack, node, true);

            while (stack.Count > 0) {
                SkipTreeNode current = stack.Pop();
                if (traverseFn(current.item)) {
                    AddChildrenToStack(stack, current, true);
                }
            }
        }

        private void ConditionalTraversePreOrderStep<U>(SkipTreeNode node, U closureArg, Func<T, U, bool> traverseFn) {
            Stack<SkipTreeNode> stack = new Stack<SkipTreeNode>();
            AddChildrenToStack(stack, node, true);

            while (stack.Count > 0) {
                SkipTreeNode current = stack.Pop();
                if (traverseFn(current.item, closureArg)) {
                    AddChildrenToStack(stack, current, true);
                }
            }
        }

        public void TraversePreOrder(Action<T> traverseFn, bool includeDisabled = false) {
            TraversePreOrderCallbackStep(root, traverseFn, includeDisabled);
        }

        public void TraversePreOrder<U>(U closureArg, Action<U, T> traverseFn, bool includeDisabled = false) {
            TraversePreOrderCallbackStep(root, closureArg, traverseFn, includeDisabled);
        }

        public void TraversePreOrder(IHierarchical item, Action<T> traverseFn, bool includeDisabled = false) {
            SkipTreeNode node;
            if (nodeMap.TryGetValue(item.UniqueId, out node)) {
                TraversePreOrderCallbackStep(node, traverseFn, includeDisabled);
                return;
            }

            SkipTreeNode parent = FindParent(item);
            parent = parent ?? root;
            SkipTreeNode ptr = parent.firstChild;
            while (ptr != null) {
                if (IsDescendentOf(ptr.item, item)) {
                    TraversePreOrderCallbackStep(ptr, traverseFn, includeDisabled);
                }

                ptr = ptr.nextSibling;
            }
        }

        public void TraversePreOrder<U>(IHierarchical item, U closureArg, Action<U, T> traverseFn, bool includeDisabled = false) {
            SkipTreeNode node;
            if (nodeMap.TryGetValue(item.UniqueId, out node)) {
                TraversePreOrderCallbackStep(node, closureArg, traverseFn, includeDisabled);
                return;
            }

            SkipTreeNode parent = FindParent(item);
            parent = parent ?? root;
            SkipTreeNode ptr = parent.firstChild;
            while (ptr != null) {
                if (IsDescendentOf(ptr.item, item)) {
                    TraversePreOrderCallbackStep(ptr, closureArg, traverseFn, includeDisabled);
                }

                ptr = ptr.nextSibling;
            }
        }

        public void TraverseRecursePreOrder() {
            SkipTreeNode ptr = root.firstChild;

            while (ptr != null) {
                TraverseRecursePreorderStep(ptr);
                ptr = ptr.nextSibling;
            }
        }


        public TreeNode GetTraversableTree(bool includeDisabled = false) {
            return GetChildTree(root, includeDisabled);
        }

        public TreeNode GetTraversableTree(T item, bool includeDisabled = false) {
            SkipTreeNode node;
            IHierarchical element = item.Element;

            if (!nodeMap.TryGetValue(element.UniqueId, out node)) {
                SkipTreeNode parent = FindParent(item);
                parent = parent ?? root;
                SkipTreeNode ptr = parent.firstChild;

                while (ptr != null) {
                    if (!includeDisabled && ptr.isDisabled) {
                        ptr = ptr.nextSibling;
                        continue;
                    }

                    if (IsDescendentOf(ptr.item, item)) {
                        scratchNodeList.Add(ptr);
                    }

                    ptr = ptr.nextSibling;
                }

                TreeNode[] children = scratchNodeList.Count == 0
                    ? TreeNode.EmptyArray
                    : new TreeNode[scratchNodeList.Count];

                for (int i = 0; i < children.Length; i++) {
                    children[i] = GetChildTree(scratchNodeList[i], includeDisabled);
                }

                return new TreeNode(item, false, children);
            }

            return GetChildTree(node, includeDisabled);
        }

        public T[] ToArray(bool includeDisabled = false) {
            // todo accept array argument to avoid allocation
            TraversePreOrder(scratchList, (list, node) => { list.Add(node); }, includeDisabled);
            T[] retn = scratchList.ToArray();
            scratchList.Clear();
            return retn;
        }

        private void TraverseNodes<U>(U closureArg, IHierarchical item, Action<U, SkipTreeNode> traverseFn, bool includeDisabled) {
            SkipTreeNode node;
            if (nodeMap.TryGetValue(item.UniqueId, out node)) {
                TraverseNodesStep(closureArg, node, traverseFn, includeDisabled);
                return;
            }

            SkipTreeNode parent = FindParent(item);
            parent = parent ?? root;
            SkipTreeNode ptr = parent.firstChild;
            while (ptr != null) {
                if (IsDescendentOf(ptr.item, item)) {
                    TraverseNodesStep(closureArg, ptr, traverseFn, includeDisabled);
                }

                ptr = ptr.nextSibling;
            }
        }

        private void TraverseNodesStep<U>(U closureArg, SkipTreeNode startNode, Action<U, SkipTreeNode> traverseFn, bool includeDisabled) {
            if (startNode.isDisabled && !includeDisabled) {
                return;
            }

            traverseFn(closureArg, startNode);

            if (startNode.firstChild == null) return;

            // todo -- pool stacks
            Stack<SkipTreeNode> stack = new Stack<SkipTreeNode>();

            AddChildrenToStack(stack, startNode, includeDisabled);

            while (stack.Count > 0) {
                SkipTreeNode current = stack.Pop();
                traverseFn(closureArg, current);
                AddChildrenToStack(stack, current, includeDisabled);
            }
        }

        private void AddChildrenToStack(Stack<SkipTreeNode> stack, SkipTreeNode parent, bool includeDisabled) {
            SkipTreeNode ptr = parent.firstChild;
            while (ptr != null) {
                if (includeDisabled || !ptr.isDisabled) {
                    stack.Push(ptr);
                }

                ptr = ptr.nextSibling;
            }
        }

        private void TraverseRecursePreorderStep(SkipTreeNode node) {
            if (node.isDisabled) return;

            SkipTreeNode ptr = node.firstChild;
            while (ptr != null) {
                TraverseRecursePreorderStep(ptr);
                ptr = ptr.nextSibling;
            }
        }

        private void TraversePreOrderCallbackStep(SkipTreeNode startNode, Action<T> traverseFn, bool includeDisabled) {
            if (startNode.isDisabled && !includeDisabled) {
                return;
            }

            if (startNode != root) {
                traverseFn(startNode.item);
            }

            if (startNode.firstChild == null) {
                return;
            }

            // todo -- pool stacks
            Stack<SkipTreeNode> stack = new Stack<SkipTreeNode>();
            AddChildrenToStack(stack, startNode, includeDisabled);

            while (stack.Count > 0) {
                SkipTreeNode current = stack.Pop();
                traverseFn(current.item);
                AddChildrenToStack(stack, current, includeDisabled);
            }
        }

        private void TraversePreOrderCallbackStep<U>(SkipTreeNode startNode, U closureArg, Action<U, T> traverseFn, bool includeDisabled) {
            // todo -- pool stacks
            Stack<SkipTreeNode> stack = new Stack<SkipTreeNode>();

            AddChildrenToStack(stack, startNode, includeDisabled);
            while (stack.Count > 0) {
                SkipTreeNode current = stack.Pop();
                traverseFn(closureArg, current.item);
                AddChildrenToStack(stack, current, includeDisabled);
            }
        }

        private SkipTreeNode FindPreviousSibling(SkipTreeNode node) {
            SkipTreeNode ptr = node.parent.firstChild;
            if (ptr == node) return null;

            while (ptr != null) {
                if (ptr.nextSibling == node) {
                    return ptr;
                }

                ptr = ptr.nextSibling;
            }

            return null;
        }

        private void Insert(SkipTreeNode parent, SkipTreeNode inserted) {
            SkipTreeNode ptr = parent.firstChild;
            IHierarchical element = inserted.item;

            // set parent
            // walk through current parent's children
            // if any of those are decsendents of inserted
            // remove from parent
            // attach as first sibling to inserted

            SkipTreeNode insertedLastChild = null;
            SkipTreeNode parentPreviousChild = null;

            while (ptr != null) {
                IHierarchical currentElement = ptr.item;
                if (IsDescendentOf(currentElement, element)) {
                    SkipTreeNode next = ptr.nextSibling;

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
                    onItemParentChanged?.Invoke(ptr.item, inserted.item, ptr.parent.item);
                    ptr.nextSibling = null;
                    insertedLastChild = ptr;
                    ptr = next;
                }
                else {
                    parentPreviousChild = ptr;
                    ptr = ptr.nextSibling;
                }
            }

            parent.childCount++;
            inserted.parent = parent;
            inserted.isDisabled = parent.isDisabled;
            inserted.nextSibling = parent.firstChild;
            parent.firstChild = inserted;
            onItemParentChanged?.Invoke(inserted.item, parent.item, null);
        }

        private SkipTreeNode FindParent(IHierarchical element) {
            IHierarchical ptr = element.Parent;
            while (ptr != null) {
                SkipTreeNode node;
                if (nodeMap.TryGetValue(ptr.UniqueId, out node)) {
                    return node;
                }

                ptr = ptr.Parent;
            }

            return null;
        }

        private static bool IsDescendentOf(IHierarchical child, IHierarchical parent) {
            IHierarchical ptr = child;
            while (ptr != null) {
                if (ptr == parent.Element) {
                    return true;
                }

                ptr = ptr.Parent;
            }

            return false;
        }

        private TreeNode GetChildTree(SkipTreeNode node, bool includeDisabled) {
            SkipTreeNode ptr = node.firstChild;
            int count = 0;
            if (node.childCount == 0) {
                return new TreeNode(node.item, node.isDisabled, TreeNode.EmptyArray);
            }
            else if (!includeDisabled) {
                while (ptr != null) {
                    if (!ptr.isDisabled) {
                        count++;
                    }

                    ptr = ptr.nextSibling;
                }

                if (count == 0) {
                    return new TreeNode(node.item, node.isDisabled, TreeNode.EmptyArray);
                }

                ptr = node.firstChild;
                TreeNode[] children = new TreeNode[count];
                count = 0;
                while (ptr != null) {
                    children[count++] = GetChildTree(ptr, false);
                    ptr = ptr.nextSibling;
                }

                ArrayUtil.ReverseInPlace(children);
                return new TreeNode(node.item, node.isDisabled, children);
            }
            else {
                count = 0;
                TreeNode[] children = new TreeNode[node.childCount];
                while (ptr != null) {
                    children[count++] = GetChildTree(ptr, false);
                    ptr = ptr.nextSibling;
                }

                ArrayUtil.ReverseInPlace(children);
                return new TreeNode(node.item, true, children);
            }
        }


        [DebuggerDisplay("{item} -> disabled: {isDisabled}")]
        private class SkipTreeNode {

            public T item;
            public bool isDisabled;
            public SkipTreeNode parent;
            public SkipTreeNode nextSibling;
            public SkipTreeNode firstChild;
            public int childCount;

            public SkipTreeNode(T item) {
                this.item = item;
                this.isDisabled = false;
            }

        }

#if DEBUG
        [UsedImplicitly]
        private TreeNode DebugTree => GetTraversableTree(true);
#endif

        [DebuggerDisplay("{item} -> disabled: {isDisabled}")]
        public struct TreeNode {

            public readonly T item;
            public readonly bool isDisabled;
            public readonly TreeNode[] children;

            internal TreeNode(T item, bool isDisabled, TreeNode[] children) {
                this.item = item;
                this.isDisabled = isDisabled;
                this.children = children ?? EmptyArray;
            }

            public static TreeNode[] EmptyArray = new TreeNode[0];

        }

    }

}