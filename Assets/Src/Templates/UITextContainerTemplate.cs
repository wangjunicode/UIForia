using System;
using System.Collections.Generic;
using System.Linq;

namespace Src {

    public class UITextContainerTemplate : UITemplate {

        public UITextContainerTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UITextContainerElement);

        public override InitData CreateScoped(TemplateScope inputScope) {
            UIElement instance = new UITextContainerElement();
            InitData initData = GetCreationData(instance, inputScope.context);

            if (childTemplates.Count == 0) {
                initData.AddChild(new UITextTemplate(string.Empty).CreateScoped(inputScope));
            }
            else {
                if (childTemplates.Count > 1) {
                    throw new Exception("TextContainers can have only 1 child");
                }

                if (!(childTemplates[0] is UITextTemplate)) {
                    throw new Exception("TextContainers must have children of type text");
                }
                
                for (int i = 0; i < childTemplates.Count; i++) {
                    initData.AddChild(childTemplates[i].CreateScoped(inputScope));
                }
            }

            instance.ownChildren = initData.children.Select(c => c.element).ToArray();
            
            return initData;
        }

    }

}