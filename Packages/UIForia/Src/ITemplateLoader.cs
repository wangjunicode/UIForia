using System;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;

namespace UIForia {

    public interface ITemplateLoader{

        Func<UIElement, TemplateScope, UIElement>[] LoadTemplates();
        
        Func<UIElement, TemplateScope, UIElement>[] LoadSlots();
        
        Action<UIElement, UIElement>[] LoadBindings();

        TemplateMetaData[] LoadTemplateMetaData(UIStyleGroupContainer[] styleListArray);

        string[] StyleFilePaths { get; }

        UIElement ConstructElement(int typeId);

    }

}