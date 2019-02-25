using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class UIElement : IHierarchical {

        public readonly int id;
        public readonly UIStyleSet style;

        internal LightList<UIElement> children;

        public ExpressionContext templateContext;

        internal UIElementFlags flags;
        internal UIElement parent;

        public LayoutResult layoutResult { get; internal set; }
        private ElementRenderer renderer = ElementRenderer.DefaultInstanced; // cold data?

        internal static IntMap<ElementColdData> s_ColdDataMap = new IntMap<ElementColdData>();
        
        protected internal UIElement() {
            this.id = Application.NextElementId;
            this.style = new UIStyleSet(this);
            this.flags = UIElementFlags.Enabled;
        }

        public Application Application {
            get { return s_ColdDataMap.GetOrDefault(id).view.Application; }
        }
        
        public UIView view {
            get { return s_ColdDataMap.GetOrDefault(id).view; }
            internal set {
                ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
                coldData.view = value;
                s_ColdDataMap[id] = coldData;
            }
        }

        public UIChildrenElement TranscludedChildren {
            get { return s_ColdDataMap.GetOrDefault(id).transcludedChildren; }
            internal set {
                ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
                coldData.transcludedChildren = value;
                s_ColdDataMap[id] = coldData;
            }
        }

        public UITemplate OriginTemplate {
            get { return s_ColdDataMap.GetOrDefault(id).templateRef; }
            internal set {
                ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
                coldData.templateRef = value;
                coldData.InitializeAttributes();
                s_ColdDataMap[id] = coldData;
            }
        }

        public ElementRenderer Renderer {
            get { return renderer; }
            set {
                if (value == null) {
                    value = ElementRenderer.DefaultInstanced;
                }
                else {
                    renderer = value;
                }
            }
        }

        public Vector2 scrollOffset { get; internal set; }

        public int depth { get; internal set; }
        public int siblingIndex { get; internal set; }

        public IInputProvider Input => view.Application.InputSystem;

        public int ChildCount => children?.Count ?? 0;

        public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;

        public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

        public bool isEnabled => (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

        public bool isDisabled => (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

        public bool hasDisabledAncestor => (flags & UIElementFlags.AncestorEnabled) == 0;

        public bool isDestroyed => (flags & UIElementFlags.Destroyed) != 0;

        internal bool isPrimitive => (flags & UIElementFlags.Primitive) != 0;

        public virtual void OnCreate() { }

        public virtual void OnReady() { }

        public virtual void OnUpdate() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        public UIElement CreateChild(Type type) {
            // todo -- ensure we can accept children

            if (!typeof(UIElement).IsAssignableFrom(type)) {
                throw new Exception("Can't create child from non UIElement type");
            }

            ParsedTemplate template = Application.templateParser.GetParsedTemplate(type);
            if (template == null) {
                throw new Exception("failed creating child");
            }

            UIElement child = template.Create();
            child.parent = this;
            child.templateContext.rootObject = templateContext.rootObject;
            children.Add(child);
            view.Application.RegisterElement(child);
            return child;
        }

        public T CreateChild<T>() where T : UIElement {
            ParsedTemplate template = Application.templateParser.GetParsedTemplate(typeof(T));
            if (template == null) {
                throw new Exception("failed creating child");
            }

            UIElement child = template.Create();
            child.parent = this;
            child.templateContext.rootObject = templateContext.rootObject;
            children.Add(child);
            view.Application.RegisterElement(child);
            return child as T;
        }

        public void SetEnabled(bool active) {
            if (active && isSelfDisabled) {
                view.Application.DoEnableElement(this);
            }
            else if (!active && isSelfEnabled) {
                view.Application.DoDisableElement(this);
            }
        }

        public UIElement GetChild(int index) {
            if (children == null || (uint) index >= (uint) children.Count) {
                return null;
            }

            return children[index];
        }

        public UIElement FindById(string id) {
            return FindById<UIElement>(id);
        }

        [PublicAPI]
        public T FindById<T>(string id) where T : UIElement {
            if (isPrimitive || children == null) {
                return null;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i].GetAttribute("id") == id) {
                    return children[i] as T;
                }

                if (children[i] is UIChildrenElement) {
                    continue;
                }

                if (children[i]?.OriginTemplate is UIElementTemplate) {
                    continue;
                }

                UIElement childResult = children[i].FindById(id);

                if (childResult != null) {
                    return childResult as T;
                }
            }

            return null;
        }

        [PublicAPI]
        public T FindFirstByType<T>() where T : UIElement {
            if (isPrimitive || children == null) {
                return null;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i] is T) {
                    return (T) children[i];
                }

                if (children[i] is UIChildrenElement) {
                    continue;
                }

                if (children[i]?.OriginTemplate is UIElementTemplate) {
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
            if (isPrimitive || children == null) {
                return retn;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i] is T) {
                    retn.Add((T) children[i]);
                }


                if (children[i] is UIChildrenElement) {
                    continue;
                }

                if (children[i]?.OriginTemplate is UIElementTemplate) {
                    continue;
                }

                children[i].FindByType<T>(retn);
            }

            return retn;
        }

        public override string ToString() {
            if (HasAttribute("id")) {
                return "<" + GetDisplayName() + ":" + GetAttribute("id") + " " + id + ">";
            }

            if (style != null && style.HasBaseStyles) {
                return "<" + GetDisplayName() + "> " + style.BaseStyleNames;
            }
            else {
                return "<" + GetDisplayName() + " " + id + ">";
            }
        }

        public virtual string GetDisplayName() {
            return GetType().Name;
        }

        public List<ElementAttribute> GetAttributes(List<ElementAttribute> retn = null) {
            return s_ColdDataMap.GetOrDefault(id).GetAttributes(retn);
        }

        public void SetAttribute(string name, string value) {
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            Application.OnAttributeSet(this, name, value);
            coldData.SetAttribute(name, value);
            s_ColdDataMap[id] = coldData;
        }

        public string GetAttribute(string attr) {
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            if (coldData.TryGetAttribute(attr, out ElementAttribute retn)) {
                return retn.value;
            }

            return null;
        }

        public void OnAttributeAdded(Action<ElementAttribute> handler) {
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            coldData.onAttributeAdded += handler;
            s_ColdDataMap[id] = coldData;
        }

        public void OnAttributeChanged(Action<ElementAttribute> handler) {
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            coldData.onAttributeChanged += handler;
            s_ColdDataMap[id] = coldData;
        }

        public void OnAttributeRemoved(Action<ElementAttribute> handler) {
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            coldData.onAttributeRemoved += handler;
            s_ColdDataMap[id] = coldData;
        }

        public bool HasAttribute(string name) {
            return s_ColdDataMap.GetOrDefault(id).GetAttribute(name).value != null;
        }

        public IRouterElement GetNearestRouter() {
            UIElement ptr = this;
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            if (coldData.nearestRouter != null) {
                return coldData.nearestRouter;
            }

            while (ptr != null) {
                if (ptr is IRouterElement routeElement) {
                    coldData.nearestRouter = routeElement;
                    s_ColdDataMap[id] = coldData;
                    return routeElement;
                }

                ptr = ptr.parent;
            }

            return null;
        }

        // todo improve this
        public RouteParameter GetRouteParameter(string name) {
            UIElement ptr = this;

            while (ptr != null) {
                if (ptr is RouteElement routeElement) {
                    return routeElement.CurrentMatch.GetParameter(name);
                }

                ptr = ptr.parent;
            }

            return default;
        }

        public int UniqueId => id;
        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

        // todo this needs to be optimized, we want to remove the recursion and if possible the style lookup
        public class RenderLayerComparerAscending : IComparer<UIElement> {

            private static RenderLayer GetRenderLayer(UIElement e) {
                while (true) {
                    if (e == null) return RenderLayer.Default;
                    if (e.style.IsDefined(StylePropertyId.RenderLayer)) {
                        return e.style.RenderLayer;
                    }

                    e = e.parent;
                }
            }

            private static int GetZIndex(UIElement e) {
                while (true) {
                    if (e == null) return 0;
                    if (e.style.IsDefined(StylePropertyId.ZIndex)) {
                        return e.style.ZIndex;
                    }

                    e = e.parent;
                }
            }

            public int Compare(UIElement a, UIElement b) {
                // todo do we need null checks? seems like elements should never be null
                if (a == null) {
                    if (b == null) return 0;
                    return 1;
                }

                if (b == null) {
                    return -1;
                }

                RenderLayer renderLayerA = GetRenderLayer(a);
                RenderLayer renderLayerB = GetRenderLayer(b);

                if (renderLayerA > renderLayerB) {
                    return -1;
                }

                if (renderLayerB > renderLayerA) {
                    return 1;
                }

                int zIndexA = GetZIndex(a);
                int zIndexB = GetZIndex(b);

                if (zIndexA > zIndexB) {
                    return -1;
                }

                if (zIndexB > zIndexA) {
                    return 1;
                }

                if (a.depth != b.depth) {
                    return a.depth > b.depth ? -1 : 1;
                }

                if (a.parent == b.parent) {
                    return a.siblingIndex > b.siblingIndex ? -1 : 1;
                }

                if (a.parent == null) return -1;
                if (b.parent == null) return 1;

                return a.parent.siblingIndex > b.parent.siblingIndex ? -1 : 1;
            }

        }

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
            ElementColdData coldData = s_ColdDataMap.GetOrDefault(id);
            coldData.Destroy();
            s_ColdDataMap.Remove(id);
            LightListPool<UIElement>.Release(ref children);
            parent = null;
        }

    }

}