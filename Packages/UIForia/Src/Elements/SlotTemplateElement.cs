using System;
using UIForia.Attributes;
using UnityEngine;

namespace UIForia.Elements {

    public class SlotTemplateElement : UIElement {

        public int count;
        public int templateId;
        
        public event Action onSlotCreated;
        public event Action onSlotDestroyed;

        [OnPropertyChanged(nameof(count))]
        public void OnCountChanged() {
            
            if (templateId < 0) {
                return;
            }
            
            int oldCount = ChildCount;

            if (count > oldCount) {
                Application.AddTemplateChildren(this, templateId, count - oldCount);
                if (onSlotCreated != null) {
                    for (int i = oldCount; i < count; i++) {
                        onSlotCreated.Invoke(); // todo -- argument
                    }
                }
            }
            else {
                
                Debug.Log("Destroying children");
                if (onSlotDestroyed != null) {
                    // todo -- wrong
                    for (int i = count; i < oldCount; i++) {
                        onSlotDestroyed.Invoke(); // todo -- argument
                    }
                }
                // Application.AddTemplateChildren(this, templateId, count - ChildCount);
            }
        }
    }

}