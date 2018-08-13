using System.Collections;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class TemplateContext {

        public UIView view;
        public int currentIndex;
        public IList currentList;
        public UIElement rootElement;
        
        public TemplateContext(UIView view) {
            this.view = view;
        }
        
       
    }

}