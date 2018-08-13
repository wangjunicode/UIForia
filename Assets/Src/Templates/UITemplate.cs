using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src {

    public abstract class UITemplate {

        public ProcessedType processedElementType;
        public List<UITemplate> childTemplates;
        public List<AttributeDefinition> attributes;
        public List<ExpressionEvaluator> generatedBindings;

        public abstract bool TypeCheck();

        public abstract UIElement CreateScoped(TemplateScope scope);

        public virtual Type ElementType => processedElementType.type;

        public bool HasAttribute(string attributeName) {
            if (attributes == null) return false;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == attributeName) return true;
            }

            return false;
        }

        public AttributeDefinition GetAttribute(string attributeName) {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == attributeName) return attributes[i];
            }

            return null;
        }

        public void ApplyStyles(UIElement element, TemplateScope scope) {
            if (!HasAttribute("style")) return;
            
            AttributeDefinition styleAttr = GetAttribute("style");
            StyleTemplate styleTemplate = scope.GetStyleTemplate(styleAttr.value);
            if (styleTemplate == null) {
                Debug.LogWarning("Unable to find style definition for: " + styleAttr.name);
                return;
            }

            UIStyle style = element.style;
            style.paint.backgroundColor = styleTemplate.backgroundColor;
            style.paint.backgroundImage = styleTemplate.backgroundImage;
            
            style.contentBox.margin = styleTemplate.margin;
            style.contentBox.padding = styleTemplate.padding;
            style.contentBox.border = styleTemplate.border;
            
            style.textStyle.alignment = styleTemplate.textAnchor;
            style.textStyle.color = styleTemplate.fontColor;
            style.textStyle.fontSize = styleTemplate.fontSize;
            style.textStyle.font = Resources.Load<Font>(styleTemplate.fontAssetName);
            style.textStyle.fontStyle = styleTemplate.fontStyle;

        }
    }

}