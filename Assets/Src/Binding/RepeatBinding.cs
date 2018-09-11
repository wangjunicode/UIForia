using System.Collections;
using System.Collections.Generic;

namespace Src {

    public abstract class RepeatBinding : Binding {

        private int currentIndex;

        private readonly string lengthAliasName;
        private readonly string arrayIndexAliasName;

        // todo use const for list name
        protected RepeatBinding(string arrayIndexAliasName, string lengthAliasName) : base("list") {
            this.arrayIndexAliasName = arrayIndexAliasName;
            this.lengthAliasName = lengthAliasName;
        }
        
        protected void Reset(UITemplateContext context, IList activeList, int listLength) {
            currentIndex = 0;
            context.SetIntAlias(lengthAliasName, listLength);
            context.activeList = activeList;
        }

        // todo -- use const for item alias
        public void Next(UITemplateContext context) {
            context.SetIntAlias(arrayIndexAliasName, currentIndex);
            context.SetObjectAlias("$item", context.activeList[currentIndex]);
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

            if (previousList == null) {
                repeat.previousListRef = new T();
                for (int i = 0; i < list.Count; i++) {
                    repeat.previousListRef.Add(list[i]);
                    InitData newItem = repeat.template.CreateScoped(repeat.scope);
                    context.view.CreateElement(newItem, repeat);
                    repeat.children.Add(newItem.element);
                }
                previousList = repeat.previousListRef;
            }

            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (list != previousList) { }

            if (list.Count > previousList.Count) {
                int previousCount = previousList.Count;
                int diff = list.Count - previousCount;
                for (int i = 0; i < diff; i++) {
                    previousList.Add(list[previousCount + i]);
                    InitData newItem = repeat.template.CreateScoped(repeat.scope);
                    context.view.CreateElement(newItem, repeat);
                    repeat.children.Add(newItem.element);
                }
            }
            else if (previousList.Count > list.Count) {
                int diff = previousList.Count - list.Count;
                for (int i = 0; i < diff; i++) {
                    int index = previousList.Count - 1;
                    context.view.DestroyElement(repeat.children[index]);
                    previousList.RemoveAt(index);
                }

            }
            
            Reset(context, (IList)previousList, previousList.Count);

        }

        public override bool IsConstant() {
            return false;
        }


    }

}