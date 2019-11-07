using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia {

    public interface ITemplateLoader{

        Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates();
        
        Func<UIElement, TemplateScope2, UIElement>[] LoadSlots();
        
        Action<UIElement, UIElement>[] LoadBindings();

        TemplateMetaData[] LoadTemplateMetaData();

    }

}