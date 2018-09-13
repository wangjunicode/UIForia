using System;
using System.Collections.Generic;
using Src.Elements;

namespace Src {

    public class UIGraphicTemplate : UITemplate {

        public UIGraphicTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UIGraphicElement);
        
        public override MetaData CreateScoped(TemplateScope inputScope) {
            return GetCreationData(new UIGraphicElement(), inputScope.context);
        }

    }

}