using System;
using System.Collections.Generic;
using System.Linq;

namespace UIForia {

    public class UISwitchTemplate : UITemplate {

        // todo -- bring all attribute constants into one file
        public const string CaseKeyAttribute = "when";
        public const string SwitchValueAttribute = "value";

        public UISwitchTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UISwitchTemplate);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
        }

        public override void Compile(ParsedTemplate template) {
            throw new NotImplementedException();
        }        

    }

}