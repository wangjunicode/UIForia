using System;
using System.Collections.Generic;

namespace Src {

    public class UIChildrenTemplate : UITemplate {

        public UIChildrenTemplate(List<UITemplate> childTemplates = null, List<AttributeDefinition> attributes = null) 
            : base(childTemplates, attributes) { }
        
        // This class is generally just a marker, it shouldn't do anything on it's own
        public override Type elementType => typeof(UIChildrenTemplate);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
        }


    }

}