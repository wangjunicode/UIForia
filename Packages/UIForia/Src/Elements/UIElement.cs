using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Elements.Routing;
using UIForia.Expressions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {

    public class InputHandlerGroup {

        public struct HandlerData {

            public InputEventType eventType;
            public KeyboardModifiers modifiers;
            public KeyCode keyCode;
            public char character;
            public bool requireFocus;
            public EventPhase eventPhase;
            public Action<GenericInputEvent> handler;

        }
        
        public InputEventType handledEvents;
        public LightList<HandlerData> eventHandlers;

        public InputHandlerGroup() {
            eventHandlers = new LightList<HandlerData>(2);
        }

        public void AddMouseEvent(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, Action<GenericInputEvent> handler) {
            handledEvents |= eventType;
            eventHandlers.Add(new HandlerData() {
                eventType = eventType,
                eventPhase = phase,
                keyCode = 0,
                character = '\0',
                requireFocus = requiresFocus,
                modifiers = modifiers,
                handler = handler
            });
        }
        
        public void AddKeyboardEvent(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, KeyCode keyCode, char character, Action<GenericInputEvent> handler) {
            handledEvents |= eventType;
             eventHandlers.Add(new HandlerData() {
                eventType = eventType,
                eventPhase = phase,
                keyCode = keyCode,
                character = character,
                requireFocus = requiresFocus,
                modifiers = modifiers,
                handler = handler
            });
        }

    }

    public struct UIElementRef {

        private readonly int id;
        private UIElement element;

        public UIElementRef(UIElement element) {
            this.id = element?.id ?? -1;
            this.element = element;
        }

        public UIElement Element {
            get {
                if (id != element.id) {
                    element = null;
                    return null;
                }

                return element;
            }
        }

        public static implicit operator UIElement(UIElementRef elementRef) {
            return elementRef.Element;
        }

    }

    public struct UIElementRef<T> where T : UIElement {

        private readonly int id;
        private T element;

        public UIElementRef(T element) {
            this.id = element?.id ?? -1;
            this.element = element;
        }

        public T Element {
            get {
                if (id != element.id) {
                    element = null;
                    return null;
                }

                return element;
            }
        }

        public static implicit operator UIElementRef(UIElementRef<T> elementRef) {
            return new UIElementRef(elementRef.Element);
        }

        public static implicit operator UIElement(UIElementRef<T> elementRef) {
            return elementRef.Element;
        }

        public static implicit operator T(UIElementRef<T> elementRef) {
            return elementRef.Element;
        }

    }

    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class UIElement : IHierarchical {

        public int id; // todo -- internal with accessor

        public InputHandlerGroup inputHandlers; // todo -- internal with accessor

        public LightList<UIElement> children; // todo -- replace w/ linked list & child count

        public ExpressionContext templateContext; // todo -- can probably be moved to binding system

        internal UIElementFlags flags;
        internal UIElement parent;

        public LayoutResult layoutResult;
        
        internal AwesomeLayoutBox layoutBox;
        internal RenderBox renderBox;
        public UIStyleSet style; // todo -- make internal with accessor
        public LinqBindingNode bindingNode; // todo -- make internal with accessor

        internal int enableStateChangedFrameId;
        public StructList<ElementAttribute> attributes;

        public UIView View { get; internal set; }

//        protected internal UIElement() {
//            this.id = Application.NextElementId;
//            this.style = new UIStyleSet(this);
//            this.layoutResult = new LayoutResult(this);
//            this.flags = UIElementFlags.Enabled | UIElementFlags.Alive | UIElementFlags.DefaultLayoutDirty;
//            this.children = LightList<UIElement>.Get();
//        }
//        
        // not actually used since we get elements from the pool as uninitialized
        protected internal UIElement() { }

        public Application Application => View.application;
        

        public int depth { get; internal set; }
        public int siblingIndex { get; internal set; }

        public TemplateMetaData templateMetaData; // todo - internal / private / whatever

        public IInputProvider Input => View.application.InputSystem; // todo -- remove

        public int ChildCount => children?.Count ?? 0;

        public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;

        public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

        public bool isEnabled => (flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet);
        //!isDestroyed && (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

        public bool isDisabled => (flags & UIElementFlags.EnabledFlagSet) != (UIElementFlags.EnabledFlagSet);

        //isDestroyed || (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

        public bool isDestroyed => (flags & UIElementFlags.Alive) == 0;

        public virtual void OnCreate() { }

        public virtual void OnUpdate() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        public virtual void HandleUIEvent(UIEvent evt) { }

        public void Destroy() {
            View.application.DoDestroyElement(this);
        }

        public UIElement InsertChild(uint idx, UIElement element) {
            throw new NotImplementedException();
            // if (element == null || element == this || element.isDestroyed) {
            //     return null;
            // }
            //
            // if (View == null) {
            //     element.parent = this;
            //     element.View = null;
            //     element.siblingIndex = children.Count;
            //     element.depth = depth + 1;
            //     children.Insert((int) idx, element);
            // }
            // else {
            //     Application.InsertChild(this, element, (uint) children.Count);
            // }
            //
            // return element;
        }

        public UIElement AddChild(UIElement element) {
            // todo -- if <Children/> is defined in the template, attach child to that element instead
            if (element == null || element == this || element.isDestroyed) {
                return null;
            }

            if (View == null) {
                element.parent = this;
                element.View = null;
                element.siblingIndex = children.Count;
                element.depth = depth + 1;
                children.Add(element);
            }
            else {
                Application.InsertChild(this, element, (uint) children.Count);
            }

            return element;
        }

        public void TriggerEvent(UIEvent evt) {
            evt.origin = this;
            UIElement ptr = this;
            while (evt.IsPropagating() && ptr != null) {
                ptr.HandleUIEvent(evt);
                ptr = ptr.parent;
            }
        }

        public void SetEnabled(bool active) {
            if (View == null) {
                flags &= ~UIElementFlags.Enabled;
                return;
            }

            if (active && isSelfDisabled) {
                View.application.DoEnableElement(this);
            }
            else if (!active && isSelfEnabled) {
                View.application.DoDisableElement(this);
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
            Stack<UIElement> elementStack = StackPool<UIElement>.Get();
            elementStack.Push(this);
            while (elementStack.Count > 0) {
                UIElement element = elementStack.Pop();
                if (element.children == null) {
                    continue;
                }

                for (int i = 0; i < element.children.Count; i++) {
                    if (element.children[i].GetAttribute("id") == elementId) {
                        bool isSameElement = element.children[i].templateContext.rootObject == this;
                        // special case for slots: if the child belongs to the parent and is also a slot definition we can assume the slot originated also from this element
                        bool isSlotDefinitionId = element.children[i].templateContext.currentObject is UISlotDefinition slotDefinition && slotDefinition.templateContext.rootObject == parent;
                        if (isSlotDefinitionId || isSameElement) {
                            return element.children[i] as T;
                        }
                    }

                    elementStack.Push(element.children[i]);
                }
            }

            StackPool<UIElement>.Release(elementStack);

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

                children[i].FindByType<T>(retn);
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
                        Application.OnAttributeSet(this, name, value, oldValue);
                        return;
                    }
                }
            }

            attributes.Add(new ElementAttribute(name, value));
            Application.OnAttributeSet(this, name, value, null);
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

        public int UniqueId => id;
        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

        public List<UIElement> GetChildren(List<UIElement> retn = null) {
            retn = ListPool<UIElement>.Get();

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

        internal UIElementTypeData GetTypeData() {
            UIElementTypeData typeData = default;
            Type elementType = GetType();
            if (s_TypeDataMap.TryGetValue(elementType, out typeData)) {
                return typeData;
            }
            else {
                typeData.requiresUpdate = ReflectionUtil.IsOverride(elementType.GetMethod(nameof(OnUpdate)));
                //typeData.attributes = elementType.GetCustomAttributes();
                s_TypeDataMap[elementType] = typeData;
                return typeData;
            }
        }

        private static readonly Dictionary<Type, UIElementTypeData> s_TypeDataMap = new Dictionary<Type, UIElementTypeData>();

        public UIElement this[int i] {
            get { return GetChild(i); }
        }

        public UIElement this[string id] {
            get { return FindById(id); }
        }

        internal struct UIElementTypeData {

            public bool requiresUpdate;
            // public Attribute[] attributes;

        }

    }

}