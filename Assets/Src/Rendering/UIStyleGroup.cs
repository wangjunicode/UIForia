using Rendering;

namespace Src.Rendering {

    public class UIStyleGroup {

        public string name;
        public UIStyle hover;
        public UIStyle normal;
        public UIStyle active;
        public UIStyle inactive;
        public UIStyle focused;

        public bool TryGetValue(StylePropertyId propertyId, out StyleProperty retn) {
            throw new System.NotImplementedException();
        }

    }

}