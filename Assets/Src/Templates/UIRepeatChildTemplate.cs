using System.Collections;

namespace Src {

    public class UIRepeatChildTemplate : UITemplate {

        public IList list;
        public int index;
        
        public override bool TypeCheck() {
            return true;
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }
        
    }

}