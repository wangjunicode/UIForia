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

        // todo -- shared instances
        // todo -- might not belong here since we want 1 instance across all templates not just scope
        public UIStyle GetStyleInstance(string id) {
            return styleTemplates.Find((t) => t.id == id)?.Instantiate();
        }
    }

}