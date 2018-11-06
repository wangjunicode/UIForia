using System;

namespace Src {

    public class UIRepeatElement : UIElement {
        
        public event Action onListPopulated;
        public event Action onListEmptied;

        internal readonly UITemplate template;
        internal readonly TemplateScope scope;

        internal Expression listExpression;

        internal Type listType;
        internal Type itemType;
        internal string itemAlias;
        internal string indexAlias;
        internal string lengthAlias;

        internal bool listBecamePopulated;
        internal bool listBecameEmpty;
        

        public UIRepeatElement(UITemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
            flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresRendering);
        }


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

        public override string GetDisplayName() {
            return "Repeat";
        }

        public override void OnDestroy() {
            onListEmptied = null;
            onListPopulated = null;
        }

    }

//    public class ObservableList<T> {
//
//        private int size;
//        private T[] list;
//
//        public int Count => size;
//        
//        public event Action<T> onItemAdded;
//        public event Action<T> onItemMoved;
//        public event Action<T> onItemRemoved;
//
//        public void Insert(int index, T item) {
//            
//        }
//
//        public T this[int index] {
//            get { return list[index]; }
//            set {
//                list[index] = value;
//            }
//        }
//        
//    }
}