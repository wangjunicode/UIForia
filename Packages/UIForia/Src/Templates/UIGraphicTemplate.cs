using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia {

    public class UIGraphicTemplate : UITemplate {

        public UIGraphicTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UIGraphicElement);
        
        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIGraphicElement graphic = new UIGraphicElement();
            graphic.children =  LightListPool<UIElement>.Get();
            graphic.OriginTemplate = this;
            return graphic;
        }

    }

}