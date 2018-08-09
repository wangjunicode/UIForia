using System.Collections.Generic;
using Rendering;

namespace Src {

    public class UIChildrenTemplate : UITemplate {

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            NonRenderedElement retn = new NonRenderedElement();
            
            for (int i = 0; i < scope.inputChildren.Count; i++) {
                retn.children.Add(scope.inputChildren[i]);
            }

            return retn;
        }

    }

}