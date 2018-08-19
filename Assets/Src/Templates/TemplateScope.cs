using System.Collections.Generic;

namespace Src {

    public class TemplateScope {

        public UIView view;
        public UITemplateContext context;
        public List<RegistrationData> inputChildren;
        
        public readonly List<RegistrationData> outputList;

        public TemplateScope(List<RegistrationData> outputList) {
            this.outputList = outputList;
        }
        
        public void SetParent(RegistrationData registrationData, RegistrationData instance) {
            if (instance.element == null) {
                registrationData.element.parent = null;
            }
            else {
                registrationData.element.parent = instance.element;
            }
            outputList.Add(registrationData);
        }

        public void RegisterAll() {
            
            for (int i = 0; i < outputList.Count; i++) {
                view.Register(outputList[i]);
            }
            
        }

    }

}