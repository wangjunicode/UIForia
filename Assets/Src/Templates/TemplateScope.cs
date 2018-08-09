using System.Collections.Generic;
using Rendering;

namespace Src {

    public class TemplateScope {

        public UIView view;
        public UIElement rootElement;
        public TemplateContext context;
        public List<object> props;
        public List<UIElement> inputChildren;

    }

}