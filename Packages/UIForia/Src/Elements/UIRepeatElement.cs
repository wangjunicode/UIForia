using System;
using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

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
        
        public event Action onListPopulated;
        public event Action onListEmptied;

        public UIRepeatElement() {
            children = children ?? new LightList<UIElement>();
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
        
        public override void OnDestroy() {
            onListEmptied = null;
            onListPopulated = null;
        }

    }

    public class UIRepeatElement<T> : UIRepeatElement {

        internal RepeatableList<T> list;
        
        public UIRepeatElement(UITemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
        }
      
        public override string GetDisplayName() {
            return "Repeat";
        }

    }

}