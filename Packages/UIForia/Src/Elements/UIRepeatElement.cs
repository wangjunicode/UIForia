using System;
using System.Collections.Generic;

namespace UIForia {

    public interface IRequireBindingSetup {

    }

    public abstract class UIRepeatElement : UIElement {
        
        internal bool listBecamePopulated;
        internal bool listBecameEmpty;
        internal Expression listExpression;

        internal Type listType;
        internal Type itemType;
        internal string itemAlias;
        internal string indexAlias;
        internal string lengthAlias;
        internal UITemplate template;
        internal TemplateScope scope;
        internal int currentIndex;
        
        public event Action onListPopulated;
        public event Action onListEmptied;

        public override void OnUpdate() {
            if (listBecamePopulated) {
                listBecamePopulated = false;
                onListPopulated?.Invoke();
            }

            if (listBecameEmpty) {
                listBecameEmpty = false;
                onListEmptied?.Invoke();
            }
        }

        public override void OnDestroy() {
            onListEmptied = null;
            onListPopulated = null;
        }

        public abstract void Next();
        public abstract void Reset();

    }

    public class UIRepeatElement<T> : UIRepeatElement, IRequireBindingSetup {

        internal T currentItem;
        internal IList<T> list;
        
        public UIRepeatElement(UITemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
            this.currentIndex = -1;
            flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresRendering);
        }
        
        public override void Next() {
            currentItem = list[++currentIndex];
        }

        public override void Reset() {
            currentItem = default;
            currentIndex = -1;
        }

        public override string GetDisplayName() {
            return "Repeat";
        }



    }

}