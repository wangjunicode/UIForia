using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Util;

namespace BuiltInElements {

    public class SlotElement : UIContainerElement {

    }

    [Template("BuiltInElements/Repeat.xml")]
    [GenericElementTypeResolvedBy(nameof(list))]
    public class Repeat<T> : UIElement {

        public List<T> list;

        public void SlotCreated() { }

        public void SlotDestroyed() { }

        // Stateful Repeat -> shuffles elements according to list position, requires a key property, or a reference type (disallows duplicates)
        // Stateless Repeat -> pure data, element references don't shuffle
        // EnumerableRepeat -> use IEnumerable instead of list
        
        // <Repeat keyFn="(item) => item.value"/>
        private StructList<KeyItem> keyList = new StructList<KeyItem>();
        
        private Func<T, int> keyFn;

        private struct KeyItem {

            public int key;
            public int lastIndex;
            public UIElement element;

        }

        private bool HasKey(int key, out UIElement element) {
            for (int i = 0; i < keyList.size; i++) {
                if (keyList[i].key == key) {
                    element = keyList[i].element;
                    return true;
                }
            }

            element = null;
            return false;
        }
        
        public override void OnUpdate() {
            
            if (keyFn == null) return;
            
            for (int i = 0; i < list.Count; i++) {
                
                int key = keyFn(list[i]);

                if (HasKey(key, out UIElement element)) {
                    // SetElementIndex(this, element, i);
                }
                else {
                    // CreateElementAtIndex(this, element, i);
                }
                
                keyList.array[i] = new KeyItem() {
                    element = children.array[i],
                    lastIndex = i,
                    key = key
                };
                
            }

        }

    }
    
}