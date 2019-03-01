using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Templates;

namespace UIForia.AttributeProcessors {

    public interface IAttributeProcessor {

        void Process(UIElement element, UITemplate template, IReadOnlyList<ElementAttribute> attributes);

    }

}
