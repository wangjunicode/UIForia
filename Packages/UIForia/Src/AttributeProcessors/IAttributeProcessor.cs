using System.Collections.Generic;

namespace UIForia.AttributeProcessors {

    public interface IAttributeProcessor {

        void Process(UIElement element, UITemplate template, IReadOnlyList<ElementAttribute> attributes);

    }

}
