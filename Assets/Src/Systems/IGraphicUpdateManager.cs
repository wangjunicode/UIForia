using Src.Elements;

namespace Src.Systems {

    public interface IGraphicUpdateManager {

        void MarkGeometryDirty(IDrawable element);

        void MarkMaterialDirty(IDrawable uiGraphicElement);

    }

}