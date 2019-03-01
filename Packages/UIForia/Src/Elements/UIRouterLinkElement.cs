using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Util;

namespace UIForia.Elements {

    [TemplateTagName("RouterLink")]
    public class UIRouterLinkElement : UIContainerElement {

        public string path;

        public UIRouterLinkElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        [OnMouseClick]
        public void GoToPath() {
            List<ElementAttribute> attrs = GetAttributes();
            for (int i = 0; i < attrs.Count; i++) {
                int index = attrs[i].name.IndexOf("parameters.", StringComparison.Ordinal);
                if (index == 0) {
                    //  Expression<string> exp = new ExpressionCompiler(null).Compile<string>(attrs[i].value);
                }
            }

            ListPool<ElementAttribute>.Release(ref attrs);
            throw new NotImplementedException();
        }

    }

    [TemplateTagName("RouterLinkBack")]
    public class UIRouterLinkBackElement : UIContainerElement {

        public override void OnUpdate() {
        }

        [OnMouseClick]
        public void GoBack() {
            throw new NotImplementedException();
        }

    }

}
