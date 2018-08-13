using System.Collections.Generic;
using Rendering;

namespace Src {

    public class TemplateScope {

        public UIView view;
        public TemplateContext context;
        public List<UIElement> inputChildren;

        public List<StyleTemplate> styleTemplates;

        public StyleTemplate GetStyleTemplate(string id) {
            for (int i = 0; i < styleTemplates.Count; i++) {
                if (styleTemplates[i].id == id) return styleTemplates[i];
            }

            return null;
        }

    }

}