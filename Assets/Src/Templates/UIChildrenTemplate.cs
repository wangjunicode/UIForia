using System;

namespace Src {

    public class UIChildrenTemplate : UITemplate {

        // This class is generally just a marker, it shouldn't do anything on it's own
        public override RegistrationData CreateScoped(TemplateScope scope) {
            throw new NotImplementedException();
        }

    }

}