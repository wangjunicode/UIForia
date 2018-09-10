using System;
using System.Collections.Generic;
using Src.Elements;

namespace Src {

    public class UIShapeTemplate : UITemplate {

        public UIShapeTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UIGraphicElement);
        
        public override InitData CreateScoped(TemplateScope inputScope) {
            return GetCreationData(new UIGraphicElement(), inputScope.context);
        }

    }

}