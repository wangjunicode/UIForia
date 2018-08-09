using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public abstract class UITemplate {

        public UITemplate parent;
        public ProcessedType processedElementType;
        public List<UITemplate> childTemplates;
        public List<AttributeDefinition> attributes;
        public List<ExpressionBinding> generatedBindings;
        
        public abstract bool TypeCheck();

        public abstract UIElement CreateScoped(TemplateScope scope);

//        public abstract UIElement Instantiate(UIView view, UIElement parent, Dictionary<string, List<UIElement>> slotMap);

    }

}