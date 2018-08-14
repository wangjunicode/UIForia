using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class UIChildrenTemplate : UITemplate {

        public override Type ElementType => typeof(UIChildrenElement);

        public override bool TypeCheck() {
            throw new NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {

            if (scope.inputChildren.Count == 0) return null;
            
            UIChildrenElement retn = new UIChildrenElement();
            retn.children = new List<UIElement>();
            
            for (int i = 0; i < scope.inputChildren.Count; i++) {
                retn.children.Add(scope.inputChildren[i]);
                retn.children[i].parent = retn;
            }

            // todo -- this element doesn't need styling
            ApplyStyles(retn, scope);
            
            return retn;
        }

    }

}