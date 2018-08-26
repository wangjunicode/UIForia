using System.Collections;
using System.Collections.Generic;

namespace Src {

    public abstract class RepeatBinding : Binding {

        private int currentIndex;

        private readonly string lengthAliasName;
        private readonly string arrayIndexAliasName;

        protected RepeatBinding(string arrayIndexAliasName, string lengthAliasName) {
            this.arrayIndexAliasName = arrayIndexAliasName;
            this.lengthAliasName = lengthAliasName;
        }
        
        protected void Reset(UITemplateContext context, IList activeList, int listLength) {
            currentIndex = 0;
            context.SetIntAlias(lengthAliasName, listLength);
            context.activelist = activeList;
        }

        public void Next(UITemplateContext context) {
            context.SetIntAlias(arrayIndexAliasName, currentIndex);
            context.SetObjectAlias("$item", context.activelist[currentIndex]);
            currentIndex++;
        }

        public void Complete(UITemplateContext context) {
            context.RemoveIntAlias(arrayIndexAliasName);
            context.RemoveIntAlias(lengthAliasName);
        }

    }
    
    // todo make a another one for U ref types that can be compared using == 
    public class RepeatBinding<T, U> : RepeatBinding where T : class, IList<U>, new() {

        private readonly Expression<T> expression;

        public RepeatBinding(Expression<T> expression, string indexAliasName, string lengthAliasName)
            : base(indexAliasName, lengthAliasName) {
            this.expression = expression;
        }
        // element passed to execute is the repeat element itself
        // store per-instance data there to keep the binding re-usable

        public override void Execute(UIElement element, UITemplateContext context) {

            UIRepeatElement<T> repeat = (UIRepeatElement<T>) element;

            T list = expression.EvaluateTyped(context);
            T previousList = repeat.previousListRef;

            if (previousList == null && list == null) {
                Reset(context, null, 0);
                return;
            }

            // todo remove scope & use context.view instead 
            if (previousList == null) {
                repeat.previousListRef = new T();
                for (int i = 0; i < list.Count; i++) {
                    repeat.previousListRef.Add(list[i]);
                    UIElementCreationData newItem = repeat.template.CreateScoped(repeat.scope);
                    newItem.element.parent = repeat;
                    context.view.Register(newItem);
                    repeat.scope.RegisterAll();
                    repeat.children.Add(newItem.element);
                }
                previousList = repeat.previousListRef;
            }

            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (list != previousList) { }

            if (list.Count > previousList.Count) {
                int diff = list.Count - previousList.Count;
                for (int i = 0; i < diff; i++) {
                    previousList.Add(list[i]);
                    UIElementCreationData newItem = repeat.template.CreateScoped(repeat.scope);
                    newItem.element.parent = repeat;
                    context.view.Register(newItem);
                    repeat.scope.RegisterAll();
                    repeat.children.Add(newItem.element);
                }
            }
            else if (previousList.Count > list.Count) {
                int diff = previousList.Count - list.Count;
                for (int i = 0; i < diff; i++) {
                    int index = previousList.Count - 1;
                    previousList.RemoveAt(index);
                    repeat.scope.view.DestroyElement(repeat.children[index]);
                }

            }
            
            Reset(context, (IList)repeat.previousListRef, repeat.previousListRef.Count);

        }

        public override bool IsConstant() {
            return false;
        }


    }

}