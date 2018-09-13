using System;
using System.Collections.Generic;

namespace Src {

    public class UIRepeatChildTemplate : UITemplate {

        public Binding[] repeatChildBindings;
        
        public UIRepeatChildTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) :
            base(childTemplates, attributes) {
            // clone so parent can clear
            this.childTemplates = new List<UITemplate>(childTemplates);
        }

        // called at runtime! not at creating time like the rest of the templates
        public override Type elementType => typeof(UIRepeatChild);

        public override MetaData CreateScoped(TemplateScope inputScope) {

            UIRepeatChild instance = new UIRepeatChild();
                        
            MetaData data = GetCreationData(instance, inputScope.context);
            data.bindings = repeatChildBindings;
            
            for (int i = 0; i < childTemplates.Count; i++) {
                data.AddChild(childTemplates[i].CreateScoped(inputScope));
            }

            return data;
        }

        public override bool Compile(ParsedTemplate template) {
            return true;
        }

    }

}