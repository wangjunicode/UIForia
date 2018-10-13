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
        for (int i = 0; i < element.ownChildren.Length; i++) {
            context.view.DestroyElement(element.ownChildren[i]);
        }

        ArrayPool<UIElement>.Release(element.ownChildren);
        element.ownChildren = new UIElement[0];
        element.templateChildren = element.ownChildren;
    }

    public override void Validate() {
        T list = listExpression.EvaluateTyped(context);

        if (list == null) {
            Reset();
            return;
        }

        if (previousReference == null) {
            previousReference = new T();
            ArrayPool<UIElement>.Release(element.ownChildren);
            UIElement[] ownChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            element.ownChildren = ownChildren;
            for (int i = 0; i < list.Count; i++) {
                previousReference.Add(list[i]);
                MetaData newItem = template.CreateScoped(scope);
                newItem.element.templateParent = element;
                ownChildren[i] = newItem.element;
                context.view.CreateElementFromTemplate(newItem, element);
            }
            
        }
        else if (list.Count > previousReference.Count) {
            UIElement[] oldChildren = element.ownChildren;
            UIElement[] ownChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            element.ownChildren = ownChildren;

            for (int i = 0; i < oldChildren.Length; i++) {
                ownChildren[i] = oldChildren[i];
            }

            int previousCount = previousReference.Count;
            int diff = list.Count - previousCount;
            for (int i = 0; i < diff; i++) {
                previousReference.Add(list[previousCount + i]);
                MetaData newItem = template.CreateScoped(scope);
                newItem.element.templateParent = element;
                ownChildren[previousCount + i] = newItem.element;
                context.view.CreateElementFromTemplate(newItem, element);
            }

            ArrayPool<UIElement>.Release(oldChildren);
        }
        else if (previousReference.Count > list.Count) {
            UIElement[] oldChildren = element.ownChildren;
            UIElement[] ownChildren = ArrayPool<UIElement>.GetExactSize(list.Count);
            element.ownChildren = ownChildren;

            int diff = previousReference.Count - list.Count;
            for (int i = 0; i < diff; i++) {
                int index = previousReference.Count - 1;
                context.RemoveContextValue(oldChildren[index], itemAlias, previousReference[index]);
                context.RemoveContextValue(oldChildren[index], indexAlias, index);
                previousReference.RemoveAt(index);
                context.view.DestroyElement(oldChildren[index]);
            }

            for (int i = 0; i < list.Count; i++) {
                ownChildren[i] = oldChildren[i];
            }

            ArrayPool<UIElement>.Release(oldChildren);
        }

        for (int i = 0; i < element.ownChildren.Length; i++) {
            context.SetContextValue(element.ownChildren[i], itemAlias, list[i]);
            context.SetContextValue(element.ownChildren[i], indexAlias, i);
        }
        
        context.SetContextValue(element, lengthAlias, previousReference.Count);
        element.templateChildren = element.ownChildren;
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