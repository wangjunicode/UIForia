using System;
using System.Collections.Generic;
using Src;
using Src.Util;

public class RepeatBindingNode : BindingNode {

    public string itemAlias;
    public string indexAlias;
    public string lengthAlias;
    public UITemplate template;
    public TemplateScope scope;

}

public class RepeatBindingNode<T, U> : RepeatBindingNode where T : class, IList<U>, new() {

    private readonly Expression<T> listExpression;
    private T previousReference;

    public RepeatBindingNode(Expression<T> listExpression, string itemAlias, string indexAlias, string lengthAlias) {
        this.listExpression = listExpression;
        this.itemAlias = itemAlias;
        this.indexAlias = indexAlias;
        this.lengthAlias = lengthAlias;
    }

    private void Reset() {
        if (previousReference == null) {
            return;
        }

        previousReference.Clear();
        previousReference = null;
        
        context.view.DestroyChildren(element);

    }

    public override void Validate() {
        T list = listExpression.EvaluateTyped(context);

        if (list == null) {
            Reset();
            return;
        }

        if (previousReference == null) {
            previousReference = new T();
            element.ownChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            element.templateChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            
            for (int i = 0; i < list.Count; i++) {
                previousReference.Add(list[i]);
                MetaData newItem = template.CreateScoped(scope);
                
                newItem.element.parent = element;
                newItem.element.templateParent = element;
                
                element.ownChildren[i] = newItem.element;
                element.templateChildren[i] = newItem.element;
                
                context.view.CreateElementFromTemplate(newItem, element);
            }

        }
        else if (list.Count > previousReference.Count) {
            UIElement[] oldChildren = element.ownChildren;
            UIElement[] oldTemplateChildren = element.templateChildren;
            
            UIElement[] ownChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            UIElement[] templateChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            
            element.ownChildren = ownChildren;
            element.templateChildren = templateChildren;
            
            for (int i = 0; i < oldChildren.Length; i++) {
                ownChildren[i] = oldChildren[i];
                templateChildren[i] = oldChildren[i];
            }

            int previousCount = previousReference.Count;
            int diff = list.Count - previousCount;
            
            for (int i = 0; i < diff; i++) {
                previousReference.Add(list[previousCount + i]);
                MetaData newItem = template.CreateScoped(scope);
                
                newItem.element.parent = element;
                newItem.element.templateParent = element;
                
                ownChildren[previousCount + i] = newItem.element;
                templateChildren[previousCount + i] = newItem.element;
                context.view.CreateElementFromTemplate(newItem, element);
            }
            
            ArrayPool<UIElement>.Release(ref oldChildren);
            ArrayPool<UIElement>.Release(ref oldTemplateChildren);
        }
        else if (previousReference.Count > list.Count) {

            // todo -- this is potentially way faster w/ a DestroyChildren(start, end) method
            
            int diff = previousReference.Count - list.Count;
            for (int i = 0; i < diff; i++) {
                int index = previousReference.Count - 1;
                context.RemoveContextValue(element.ownChildren[index], itemAlias, previousReference[index]);
                context.RemoveContextValue(element.ownChildren[index], indexAlias, index);
                previousReference.RemoveAt(index);
                context.view.DestroyElement(element.ownChildren[index]);
            }
        }

        for (int i = 0; i < element.ownChildren.Length; i++) {
            context.SetContextValue(element.ownChildren[i], itemAlias, list[i]);
            context.SetContextValue(element.ownChildren[i], indexAlias, i);
        }
        
        context.SetContextValue(element, lengthAlias, previousReference.Count);
    }

    public override void OnUpdate(SkipTree<BindingNode>.TreeNode[] children) {
        context.current = element;

        for (int i = 0; i < bindings.Length; i++) {
            if (bindings[i].isEnabled) {
                bindings[i].Execute(element, context);
            }
        }

        if (!element.isEnabled || children == null || previousReference == null) {
            return;
        }

        for (int i = 0; i < children.Length; i++) {
            children[i].item.OnUpdate(children[i].children);
        }

    }

}