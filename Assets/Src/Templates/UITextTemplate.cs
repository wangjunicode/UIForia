using System;
using System.Collections.Generic;
using Src.Parsing;

namespace Src {

    public class UITextTemplate : UITemplate {

        private static readonly TextElementParser textParser = new TextElementParser();
        private readonly Type textElementType;
        
        public UITextTemplate(Type textElementType, string rawText, List<AttributeDefinition> attributes = null) : base(null, attributes) {
            this.textElementType = textElementType;
            this.RawText = rawText;
        }
        
        public UITextTemplate(string rawText, List<AttributeDefinition> attributes = null) : base(null, attributes) {
            this.textElementType = null;
            this.RawText = rawText;
        }
        
        public string RawText { get; }

        public override bool Compile(ParsedTemplate template) {

            // todo -- if part is constant and value is empty string, remove it
            string[] expressionParts = textParser.Parse(RawText);

            if (expressionParts.Length == 1) {
                Expression<string> exp = template.compiler.Compile<string>(expressionParts[0]);
                bindingList.Add(new TextBinding_Single(exp));    
            }
            else {
                
                Expression<string>[] expressions = new Expression<string>[expressionParts.Length];
                
                for (int i = 0; i < expressionParts.Length; i++) {
                    expressions[i] = template.compiler.Compile<string>(expressionParts[i]);
                }
                
                // todo -- if constant, try to merge bindings. This probably happens in the binding system
                // todo    because we need to have access to the element and an expression context
                bindingList.Add(new TextBinding_Multiple(expressions));
            }

            // todo -- might ask template root for the style for element and default to DefaultIntrinsicStyles if not provided
            // or merge both if template provides one
            
            if (textElementType == typeof(UILabelElement)) {
                baseStyles.Add(DefaultIntrinsicStyles.LabelStyle);
            }
            else if (textElementType == typeof(UIParagraphElement)) {
                baseStyles.Add(DefaultIntrinsicStyles.ParagraphStyle);
            }
            else if (textElementType == typeof(UIHeading1Element)) {
                baseStyles.Add(DefaultIntrinsicStyles.Heading1Style);
            }
            else if (textElementType == typeof(UIHeading2Element)) {
                baseStyles.Add(DefaultIntrinsicStyles.Heading2Style);
            }
            else if (textElementType == typeof(UIHeading3Element)) {
                baseStyles.Add(DefaultIntrinsicStyles.Heading3Style);
            }
            else if (textElementType == typeof(UIHeading4Element)) {
                baseStyles.Add(DefaultIntrinsicStyles.Heading4Style);
            }
            else if (textElementType == typeof(UIHeading5Element)) {
                baseStyles.Add(DefaultIntrinsicStyles.Heading5Style);
            }
            else if (textElementType == typeof(UIHeading6Element)) {
                baseStyles.Add(DefaultIntrinsicStyles.Heading6Style);
            }

            return base.Compile(template);
        }

        public override Type elementType => typeof(UITextElement);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            return GetCreationData(new UITextElement(), inputScope.context);
        }

    }

   
    
   
    
    
}