using System;
using Src.Parsing;

namespace Src {

    public class UITextTemplate : UITemplate {


        private static readonly TextElementParser textParser = new TextElementParser();

        public UITextTemplate(string rawText) : base(null) {
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
            
            return base.Compile(template);
        }

        public override Type elementType => typeof(UITextElement);

        public override InitData CreateScoped(TemplateScope inputScope) {
            return GetCreationData(new UITextElement(), inputScope.context);
        }

    }

   
    
   
    
    
}