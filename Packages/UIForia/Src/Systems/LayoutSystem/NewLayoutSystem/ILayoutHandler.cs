using UIForia.Rendering;
using UIForia.Systems;

namespace UIForia.Layout {

    public interface ILayoutHandler {

        float ResolveAutoWidth(ElementId elementId, UIMeasurement measurement);

    }

}