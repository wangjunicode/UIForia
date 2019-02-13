using System;
using System.Collections.Generic;
using UIForia.Util;

namespace UIForia {

    internal struct ElementColdData {

        public event Action<ElementAttribute> onAttributeAdded;
        public event Action<ElementAttribute> onAttributeChanged;
        public event Action<ElementAttribute> onAttributeRemoved;

        public UIView view;
        public UITemplate templateRef;
        public LightList<ElementAttribute> attributes;
        public UIChildrenElement transcludedChildren;
        public IRouterElement nearestRouter;

        public void InitializeAttributes() {
            if (attributes == null && templateRef?.templateAttributes != null) {
                attributes = LightListPool<ElementAttribute>.Get();
                for (int i = 0; i < templateRef.templateAttributes.Count; i++) {
                    attributes.AddUnchecked(templateRef.templateAttributes[i]);
                }
            }
        }

        public void SetAttribute(string name, string value) {
            ElementAttribute attribute = new ElementAttribute(name, value);
            attributes = attributes ?? LightListPool<ElementAttribute>.Get();
            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == name) {
                    if (string.IsNullOrEmpty(value)) {
                        attributes.RemoveAt(i);
                        onAttributeRemoved?.Invoke(attribute);
                    }
                    else {
                        attributes[i] = attribute;
                        onAttributeChanged?.Invoke(attributes[i]);
                    }

                    return;
                }
            }

            attributes.Add(attribute);
            onAttributeAdded?.Invoke(attribute);
        }

        public void RemoveAttribute(string name) {
            if (attributes == null) {
                return;
            }

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == name) {
                    onAttributeRemoved?.Invoke(attributes[i]);
                    attributes.RemoveAt(i);
                    return;
                }
            }
        }

        public bool TryGetAttribute(string name, out ElementAttribute attr) {
            if (attributes == null) {
                attr = default;
                return false;
            }

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == name) {
                    attr = attributes[i];
                    return true;
                }
            }

            attr = default;
            return false;
        }

        public ElementAttribute GetAttribute(string name) {
            if (attributes != null) {
                for (int i = 0; i < attributes.Count; i++) {
                    if (attributes[i].name == name) {
                        return attributes[i];
                    }
                }
            }

            return new ElementAttribute(name, null);
        }

        public List<ElementAttribute> GetAttributes(List<ElementAttribute> retn) {
            if (retn == null) {
                retn = ListPool<ElementAttribute>.Get();
            }

            if (attributes != null) {
                for (int i = 0; i < attributes.Count; i++) {
                    retn.Add(attributes[i]);
                }
            }

            return retn;
        }

        public void Destroy() {
            LightListPool<ElementAttribute>.Release(ref attributes);
            onAttributeAdded = null;
            onAttributeRemoved = null;
            onAttributeChanged = null;
            transcludedChildren = null;
            templateRef = null;
            view = null;
        }

    }

}