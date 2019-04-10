using System;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Bindings {

    public class RepeatBindingNode : BindingNode {

        public UITemplate template;

    }

    public class RepeatBindingNode<T, U> : RepeatBindingNode where T : RepeatableList<U>, new() {

        private readonly UIRepeatElement<U> repeat;
        private readonly Expression<T> listExpression;
        private T previousReference;

        public RepeatBindingNode(UIRepeatElement<U> repeat, Expression<T> listExpression) {
            this.repeat = repeat;
            this.listExpression = listExpression;
        }

        private void OnItemInserted(U item, int index) {
            UIElement newItem = template.CreateScoped(repeat.scope);
            newItem.parent = element;
            // root object isn't being assigned. make it assigned 
            newItem.templateContext.rootObject = element.templateContext.rootObject;
            element.children.Insert(index, newItem);
            element.View.Application.RegisterElement(newItem);
        }

        private void OnItemRemoved(U item, int index) {
            Application.DestroyElement(element.children[index]);
        }

        private void OnItemMoved(U item, int index, int newIndex) {
            throw new NotImplementedException();
        }

        private void OnClear() {
            element.View.Application.DestroyChildren(element);
        }

        public void CreateOrDestroyChildren() {
            T list = listExpression.Evaluate(context);
            repeat.list = list;

            // if we get a new list entirely -> nuke the world & rebuild
            // if get get null -> nuke the world
            // if we get the same list -> process changes

            if (list == null || list.Count == 0) {
                if (previousReference == null) {
                    return;
                }

                repeat.listBecameEmpty = previousReference.Count > 0;
                previousReference = null;
                element.View.Application.DestroyChildren(element);
                return;
            }

            if (list == previousReference) {
                return;
            }

            if (previousReference != null) {
                previousReference.onItemInserted -= OnItemInserted;
               // previousReference.onItemMoved -= OnItemMoved;
                previousReference.onItemRemoved -= OnItemRemoved;
                previousReference.onClear -= OnClear;
                element.View.Application.DestroyChildren(element);
            }

            previousReference = list;

            list.onItemInserted += OnItemInserted;
          //  list.onItemMoved += OnItemMoved;
            list.onItemRemoved += OnItemRemoved;
            list.onClear += OnClear;

            for (int i = 0; i < list.Count; i++) {
                UIElement newItem = template.CreateScoped(repeat.scope);
                newItem.parent = element;
                // root object isn't being assigned. make it assigned 
                newItem.templateContext.rootObject = element.templateContext.rootObject;
                element.children.Insert(i, newItem);
                element.View.Application.RegisterElement(newItem);
            }

            repeat.listBecamePopulated = list.Count > 0;
        }

        public override void OnUpdate() {
            CreateOrDestroyChildren();

            if (!element.isEnabled || element.children == null || previousReference == null) {
                return;
            }

            for (int i = 0; i < bindings.Length; i++) {
                bindings[i].Execute(element, context);
            }
        }

    }
}