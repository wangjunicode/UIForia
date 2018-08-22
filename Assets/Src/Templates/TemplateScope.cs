using System.Collections.Generic;
using UnityEngine;

namespace Src {

    public class TemplateScope {

        public UIView view;
        public UITemplateContext context;
        public List<UIElementCreationData> inputChildren;
        
        public readonly List<UIElementCreationData> outputList;

        public TemplateScope(List<UIElementCreationData> outputList) {
            this.outputList = outputList;
        }
        
        public void SetParent(UIElementCreationData uiElementCreationData, UIElementCreationData instance) {
            if (instance.element == null) {
                uiElementCreationData.element.parent = null;
            }
            else {
                uiElementCreationData.element.parent = instance.element;
            }
            outputList.Add(uiElementCreationData);
        }

        public void RegisterAll() {
            Debug.Log("Registering " + outputList.Count + " elements");
            for (int i = 0; i < outputList.Count; i++) {
                view.Register(outputList[i]);
            }
            
        }

    }

}