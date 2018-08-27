using System.Collections.Generic;

namespace Src {

    public class TemplateScope {

        public UITemplateContext context;
        public List<InitData> inputChildren;


        public TemplateScope() {}
        
        public void Clear() {
            inputChildren.Clear();
        }

    }

}