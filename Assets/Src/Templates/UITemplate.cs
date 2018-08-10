using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public abstract class UITemplate {

        public ProcessedType processedElementType;
        public List<UITemplate> childTemplates;
        public List<AttributeDefinition> attributes;
        public List<ExpressionEvaluator> generatedBindings;
        
        public abstract bool TypeCheck();

        public abstract UIElement CreateScoped(TemplateScope scope);

//        public abstract UIElement Instantiate(UIView view, UIElement parent, Dictionary<string, List<UIElement>> slotMap);

        public virtual Type ElementType => processedElementType.type;

    }

}