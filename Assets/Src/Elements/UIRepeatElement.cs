using System;
using System.Collections;
using System.Collections.Generic;

namespace Src {

    public class UIRepeatElement<T> : UIElement {

        public readonly UIRepeatChildTemplate template;
        public readonly TemplateScope scope;

        public List<T> previousListRef;
        public List<T> filteredList;

        public UIRepeatElement(UIRepeatChildTemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
        }

    }

    // todo make a another one for U ref types that can be compared using == 
    public class RepeatBinding<T, U> : Binding where T : class, IList<U> {

        private readonly Expression<T> expression;

        public RepeatBinding(Expression<T> expression) {
            this.expression = expression;
        }
        // element passed to execute is the repeat element itself
        // store per-instance data there to keep the binding re-usable

        public override void Execute(UIElement element, UITemplateContext context) {
            UIRepeatElement<U> repeat = (UIRepeatElement<U>) element;

            T list = expression.EvaluateTyped(context);
            List<U> previousList = repeat.previousListRef;

            if (previousList == null && list == null) {
                return;
            }

            if (previousList == null) {
                repeat.previousListRef = new List<U>(list);
                for (int i = 0; i < list.Count; i++) {
                    UIElementCreationData newItem = repeat.template.CreateScoped(repeat.scope);
                    newItem.element.parent = repeat;
                    repeat.scope.view.Register(newItem);
                    repeat.scope.Clear();
                }
            }

            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (list != repeat.previousListRef) { }
            if (list.Count > repeat.previousListRef.Count) {
                int diff = list.Count - repeat.previousListRef.Count;
                for (int i = 0; i < diff; i++) {
                    repeat.previousListRef.Add(list[i]);
                    UIElementCreationData newItem = repeat.template.CreateScoped(repeat.scope);
                    newItem.element.parent = repeat;
                    repeat.scope.view.Register(newItem);
                }
            }
            else if (repeat.previousListRef.Count > list.Count) {
                int diff = repeat.previousListRef.Count - list.Count;
                for (int i = 0; i < diff; i++) {
                    int index = repeat.previousListRef.Count - 1;
                    repeat.previousListRef.RemoveAt(index);
                    //view.Destroy(repeat.children[index]);
                }

            }

        }

        public override bool IsConstant() {
            return false;
        }

    }

}