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

        private readonly Action<U, int> onInserted;
        private readonly Action<U, int> onRemoved;
        private readonly Action onClear;
        
        public RepeatBindingNode(UIRepeatElement<U> repeat, Expression<T> listExpression) {
            this.repeat = repeat;
            this.listExpression = listExpression;
            this.onInserted = OnItemInserted;
            this.onRemoved = OnItemRemoved;
            this.onClear = OnClear;
        }

        private void OnItemInserted(U item, int index) {
            UIElement newItem = template.CreateScoped(repeat.scope);
            // root object isn't being assigned. make it assigned 
            newItem.templateContext.rootObject = element.templateContext.rootObject;
            element.InsertChild((uint)index, newItem);
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
                
                if (previousReference != null) {
                    previousReference.onItemInserted -= onInserted;
                    // previousReference.onItemMoved -= OnItemMoved;
                    previousReference.onItemRemoved -= onRemoved;
                    previousReference.onClear -= onClear;
                    element.View.Application.DestroyChildren(element);
                    previousReference = null;
                }
                
                element.View.Application.DestroyChildren(element);
                return;
            }

            if (list == previousReference) {
                return;
            }

            if (previousReference != null) {
                previousReference.onItemInserted -= onInserted;
               // previousReference.onItemMoved -= OnItemMoved;
                previousReference.onItemRemoved -= onRemoved;
                previousReference.onClear -= onClear;
                element.View.Application.DestroyChildren(element);
            }

            previousReference = list;

            list.onItemInserted += onInserted;
          //  list.onItemMoved += OnItemMoved;
            list.onItemRemoved += onRemoved;
            list.onClear += onClear;

            for (int i = 0; i < list.Count; i++) {
                OnItemInserted(list[i], i);
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

        public override void OnReset() {

            if (previousReference != null) {
                previousReference.onItemInserted -= onInserted;
                previousReference.onItemRemoved -= onRemoved;
                previousReference.onClear -= onClear;
                previousReference = null;
            }
        }
    }
}