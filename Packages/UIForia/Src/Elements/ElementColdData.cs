using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

public struct ElementColdData {

    public event Action<ElementAttribute> onAttributeAdded;
    public event Action<ElementAttribute> onAttributeChanged;
    public event Action<ElementAttribute> onAttributeRemoved;

    public UITemplate templateRef;
    public ElementRenderer renderer;
    public UIElement templateParent;
    public UITemplateContext templateContext;
    public LightList<ElementAttribute> attributes;
    public UIChildrenElement transcludedChildren;
    public UIView view;

    private void InitializeAttributes() {
        if (attributes == null && templateRef?.attributes != null) {
            attributes = LightListPool<ElementAttribute>.Get();
            for (int i = 0; i < templateRef.attributes.Count; i++) {
                attributes.AddUnchecked(new ElementAttribute(templateRef.attributes[i].key, templateRef.attributes[i].value));
            }
        }
    }

    public void SetAttribute(string name, string value) {
        InitializeAttributes();
        ElementAttribute attribute = new ElementAttribute(name, value);
        attributes = attributes ?? LightListPool<ElementAttribute>.Get();
        for (int i = 0; i < attributes.Length; i++) {
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
        InitializeAttributes();
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

    public ElementAttribute GetAttribute(string name) {
        InitializeAttributes();
        if (attributes != null) {
            for (int i = 0; i < attributes.Length; i++) {
                if (attributes[i].name == name) {
                    return attributes[i];
                }
            }
        }

        return new ElementAttribute(name, null);
    }

    public List<ElementAttribute> GetAttributes(List<ElementAttribute> retn) {
        InitializeAttributes();
        if (retn == null) {
            retn = ListPool<ElementAttribute>.Get();
        }

        if (attributes != null) {
            for (int i = 0; i < attributes.Length; i++) {
                retn.Add(attributes[i]);
            }
        }

        return retn;
    }

}