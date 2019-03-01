using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Parsing.Expression;

namespace UIForia.Templates {

    public class UISwitchTemplate : UITemplate {

        // todo -- bring all attribute constants into one file
        public const string CaseKeyAttribute = "when";
        public const string SwitchValueAttribute = "value";

        public UISwitchTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UISwitchTemplate);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
        }

        public override void Compile(ParsedTemplate template) {
            throw new NotImplementedException();
        }        

    }

}