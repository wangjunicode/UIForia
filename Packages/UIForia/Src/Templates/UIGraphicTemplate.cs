using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia {

    public class UIGraphicTemplate : UITemplate {

        public UIGraphicTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) 
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UIGraphicElement);
        
        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIGraphicElement graphic = new UIGraphicElement();
            graphic.children =  LightListPool<UIElement>.Get();
            graphic.OriginTemplate = this;
            return graphic;
        }

    }

}