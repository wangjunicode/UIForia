using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Elements {

    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public abstract class UIElement : IHierarchical {

        public ElementId id;

        public InputHandlerGroup inputHandlers; // todo -- internal with accessor

        public LightList<UIElement> children; // todo -- replace w/ linked list & child count

        private UIElementFlags flags;
        public UIElement parent;

        // todo -- maybe move a lot of this data to an internal representation of UIElement
        internal LayoutBox layoutBox;
        internal RenderBox renderBox;
        public UIStyleSet style; // todo -- make internal with accessor
        public LinqBindingNode bindingNode; // todo -- make internal with accessor

        internal int enableStateChangedFrameId;
        public StructList<ElementAttribute> attributes;
        public TemplateMetaData templateMetaData; // todo - internal / private / whatever

        public UIView View { get; internal set; }
        public Application application { get; internal set; }
        public int hierarchyDepth { get; internal set; }
        private int _siblingIndex;

        public int siblingIndex {
            get => _siblingIndex;
            internal set {
                if (_siblingIndex == value) return;
                _siblingIndex = value;
                // flags |= UIElementFlags.IndexChanged;
            }
        }

//        
        // not actually used since we get elements from the pool as uninitialized
        protected internal UIElement() { }

        public IInputProvider Input => application.InputSystem; // todo -- remove

        public int ChildCount => children?.Count ?? 0;

        public bool __internal_isEnabledAndNeedsUpdate => (flags & UIElementFlags.EnabledFlagSetWithUpdate) == (UIElementFlags.EnabledFlagSetWithUpdate);

        public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;

        public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

        public bool isEnabled {
            get => ((UIElementFlags)application.elementSystem.metaTable[id].flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet);
        }
        
        //!isDestroyed && (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

        public bool isDisabled => (flags & UIElementFlags.EnabledFlagSet) != (UIElementFlags.EnabledFlagSet);

        //isDestroyed || (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

        public bool isDestroyed => (flags & UIElementFlags.Alive) == 0;

        public virtual void OnCreate() { }

        public virtual void OnUpdate() { }

        public virtual void OnBeforePropertyBindings() { }

        public virtual void OnAfterPropertyBindings() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        internal void Destroy() {
            application.DoDestroyElement(this);
        }

        public ref LayoutResult layoutResult {
            get => ref application.elementSystem.layoutTable[id.index];
        }

        public UIElement GetLastChild() {
            ElementSystem elementSystem = application.elementSystem;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;
            UIElement[] instanceTable = elementSystem.instanceTable;

            ref HierarchyInfo tableEntry = ref hierarchyTable[id];

            if (tableEntry.lastChildId.index != 0) {
                return instanceTable[tableEntry.lastChildId.index];
            }

            return null;
        }

        public UIElement GetFirstChild() {

            ElementSystem elementSystem = application.elementSystem;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;
            UIElement[] instanceTable = elementSystem.instanceTable;

            ref HierarchyInfo tableEntry = ref hierarchyTable[id];

            if (tableEntry.firstChildId.index != 0) {
                return instanceTable[tableEntry.firstChildId.index];
            }

            return null;
        }

        public UIElement GetPreviousSibling() {

            ElementSystem elementSystem = application.elementSystem;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;
            UIElement[] instanceTable = elementSystem.instanceTable;

            ref HierarchyInfo tableEntry = ref hierarchyTable[id];

            if (tableEntry.prevSiblingId.index != 0) {
                return instanceTable[tableEntry.prevSiblingId.index];
            }

            return null;
        }

        public UIElement GetNextSibling() {

            ElementSystem elementSystem = application.elementSystem;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;
            UIElement[] instanceTable = elementSystem.instanceTable;

            ref HierarchyInfo tableEntry = ref hierarchyTable[id];

            if (tableEntry.nextSiblingId.index != 0) {
                return instanceTable[tableEntry.nextSiblingId.index];
            }

            return null;
        }

        public UIElement FindChildAt(int index) {

            ElementSystem elementSystem = application.elementSystem;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;
            UIElement[] instanceTable = elementSystem.instanceTable;

            ref HierarchyInfo tableEntry = ref hierarchyTable[id];

            if (tableEntry.childCount == 0 || index >= tableEntry.childCount) {
                return null;
            }

            ElementId ptr = tableEntry.firstChildId;
            int cnt = 0;
            while (ptr.index != 0) {
                if (cnt == index) {
                    return instanceTable[ptr.index];
                }

                cnt++;
                ptr = hierarchyTable[ptr].nextSiblingId;
            }

            return null;
        }

        public int GetChildren(IList<UIElement> retn, bool replaceListContents = true) {
            if (retn == null) return -1;
            if (replaceListContents) {
                retn.Clear();
            }

            ElementSystem elementSystem = application.elementSystem;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;
            UIElement[] instanceTable = elementSystem.instanceTable;

            ref HierarchyInfo tableEntry = ref hierarchyTable[id];

            if (tableEntry.childCount == 0) return 0;

            ElementId ptr = tableEntry.firstChildId;
            while (ptr.index != 0) {
                retn.Add(instanceTable[ptr.index]);
                ptr = hierarchyTable[ptr].nextSiblingId;
            }

            return tableEntry.childCount;
        }

        public bool internal__dontcallmeplease_SetEnabledIfBinding(bool enabled) {
            if (enabled && isSelfDisabled) {
                application.DoEnableElement(this);
            }
            else if (!enabled && isSelfEnabled) {
                application.DoDisableElement(this);
            }

            return false;
        }

        public void SetEnabled(bool enabled) {
            if (enabled && isSelfDisabled) {
                application.DoEnableElement(this);
            }
            else if (!enabled && isSelfEnabled) {
                application.DoDisableElement(this);
            }
        }

        public UIElement GetChild(int index) {
            if (children == null || (uint) index >= (uint) children.Count) {
                return null;
            }

            return children[index];
        }

        public UIElement FindById(string elementId) {
            return FindById<UIElement>(elementId);
        }

        [PublicAPI]
        public T FindById<T>(string elementId) where T : UIElement {
            LightStack<UIElement> elementStack = LightStack<UIElement>.Get();
            elementStack.Push(this);
            while (elementStack.size > 0) {
                UIElement element = elementStack.array[--elementStack.size];

                if (element.children == null) {
                    continue;
                }

                elementStack.EnsureAdditionalCapacity(element.children.size);

                for (int i = element.children.size - 1; i >= 0; i--) {
                    UIElement child = element.children.array[i];

                    // todo -- need to figure out if we should descend into children. probably want to scrap this whole method and do something better with selectors
                    // if (child.templateMetaData == element.templateMetaData) {
                    if (child is T castChild && child.GetAttribute("id") == elementId) {
                        LightStack<UIElement>.Release(ref elementStack);
                        return castChild;
                    }
                    // }

                    elementStack.array[elementStack.size++] = child;
                }
            }

            LightStack<UIElement>.Release(ref elementStack);

            return null;
        }

        [PublicAPI]
        public T FindFirstByType<T>() where T : UIElement {
            if (children == null) {
                return null;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i] is T) {
                    return (T) children[i];
                }

                if (children[i] is UIChildrenElement) {
                    continue;
                }

                UIElement childResult = children[i].FindFirstByType<T>();
                if (childResult != null) {
                    return (T) childResult;
                }
            }

            return null;
        }

        public List<T> FindByType<T>(List<T> retn = null) where T : UIElement {
            retn = retn ?? new List<T>();
            if (children == null) {
                return retn;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i] is T) {
                    retn.Add((T) children[i]);
                }

                if (children[i] is UIChildrenElement) {
                    continue;
                }

                children[i].FindByType(retn);
            }

            return retn;
        }

        public override string ToString() {
            if (style != null) {
                string idText = GetAttribute("id");
                string styleNames = style.GetStyleNames();
                return $"<{GetDisplayName()}[{id}]{(idText != null ? ":" + idText : "")}> {styleNames}";
            }
            else {
                return "<" + GetDisplayName() + " " + id + ">";
            }
        }

        public virtual string GetDisplayName() {
            return GetType().Name;
        }

        public List<ElementAttribute> GetAttributes(List<ElementAttribute> retn = null) {
            retn = retn ?? new List<ElementAttribute>();
            if (attributes == null || attributes.size == 0) {
                return retn;
            }

            for (int i = 0; i < attributes.size; i++) {
                retn.Add(attributes.array[i]);
            }

            return retn;
        }

        public void SetAttribute(string name, string value) {
            if (attributes == null) {
                attributes = StructList<ElementAttribute>.Get();
            }

            ElementAttribute[] attrs = attributes.array;
            int attrCount = attributes.size;

            for (int i = 0; i < attrCount; i++) {
                if (attrs[i].name == name) {
                    if (attrs[i].value == value) {
                        return;
                    }
                    else {
                        string oldValue = attrs[i].value;
                        attrs[i] = new ElementAttribute(name, value);
                        application.OnAttributeSet(this, name, value, oldValue);
                        return;
                    }
                }
            }

            attributes.Add(new ElementAttribute(name, value));
            application.OnAttributeSet(this, name, value, null);
        }

        public bool TryGetAttribute(string key, out string value) {
            if (attributes == null) {
                value = null;
                return false;
            }

            ElementAttribute[] attrs = attributes.array;
            int attrCount = attributes.size;

            for (int i = 0; i < attrCount; i++) {
                if (attrs[i].name == key) {
                    value = attrs[i].value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public string GetAttribute(string attr) {
            if (attributes == null) {
                return null;
            }

            ElementAttribute[] attrs = attributes.array;
            int attrCount = attributes.size;

            for (int i = 0; i < attrCount; i++) {
                if (attrs[i].name == attr) {
                    return attrs[i].value;
                }
            }

            return null;
        }

        public bool HasAttribute(string name) {
            return GetAttribute(name) != null;
        }

        public int UniqueId => id.id;
        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

        // todo -- remove
        public List<UIElement> GetChildren(List<UIElement> retn = null) {
            retn = retn ?? ListPool<UIElement>.Get();

            if (children == null) {
                return retn;
            }

            UIElement[] childArray = children.Array;
            for (int i = 0; i < children.Count; i++) {
                retn.Add(childArray[i]);
            }

            return retn;
        }

        internal void InternalDestroy() {
            LightList<UIElement>.Release(ref children);
            parent = null;
        }

        public MaterialInterface Material {
            get => new MaterialInterface(this, application);
        }

        public bool IsAncestorOf(UIElement potentialParent) {
            if (potentialParent == this || potentialParent == null) {
                return false;
            }

            UIElement ptr = this;
            while (ptr != null) {
                if (ptr.parent == potentialParent) {
                    return true;
                }

                ptr = ptr.parent;
            }

            return false;
        }

        public UIElement this[int i] {
            get { return GetChild(i); }
        }

        public UIElement this[string id] {
            get { return FindById(id); }
        }

        public ElementAnimator Animator => new ElementAnimator(application.animationSystem, this);

        public IInputSystem InputSystem => application.inputSystem;

        // element.animator.Stop();

        public void ScrollIntoView() {
            UIElement ptr = parent;

            float crawlPositionX = layoutResult.localPosition.x;
            float crawlPositionY = layoutResult.localPosition.y;

            while (ptr != null) {

                if (ptr is ScrollView scrollView) {
                    scrollView.ScrollElementIntoView(this, crawlPositionX, crawlPositionY);
                    return;
                }

                crawlPositionX += ptr.layoutResult.localPosition.x;
                crawlPositionY += ptr.layoutResult.localPosition.y;

                ptr = ptr.parent;
            }
        }

        public T FindParent<T>() where T : UIElement {
            UIElement ptr = parent;
            while (ptr != null) {
                if (ptr is T retn) {
                    return retn;
                }

                ptr = ptr.parent;
            }

            return null;
        }

    }

}