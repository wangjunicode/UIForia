using System;
using System.Collections.Generic;

namespace Src {

    public class UIPrefabTemplate : UITemplate {

        public UIPrefabTemplate(List<AttributeDefinition> attributes)
            : base(null, attributes) { }

        public override Type elementType => null;

        public override MetaData CreateScoped(TemplateScope inputScope) {
            throw new System.NotImplementedException();
        }

    }

}