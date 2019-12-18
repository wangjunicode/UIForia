namespace UIForia.Compilers {

    public class TemplateConstants {

        public const string InitSource = @"using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_::APPNAME:: : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {
::STYLE_FILE_PATHS::
        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
::TEMPLATE_CODE::
        }

        public TemplateMetaData[] LoadTemplateMetaData(UIStyleGroupContainer[] styleMap) {
::TEMPLATE_META_CODE::
        }

        public Action<UIElement, UIElement>[] LoadBindings() {
::BINDING_CODE::
        }

        public Func<UIElement, TemplateScope, UIElement>[] LoadSlots() {
::SLOT_CODE::
        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
::ELEMENT_CONSTRUCTORS::
            }
            return null;
        }

    }

}";

        public const string TemplateSource = @"using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_::APPNAME:: {
        
        public Func<UIElement, TemplateScope, UIElement> Template_::GUID:: = ::CODE:: 
        ::BINDINGS::
        ::SLOTS::
    }

}
                ";

    }

}