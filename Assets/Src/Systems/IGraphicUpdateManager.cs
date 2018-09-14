using Src.Elements;

namespace Src.Systems {

    public interface IGraphicUpdateManager {

        void MarkGeometryDirty(IGraphicElement element);

        void MarkMaterialDirty(IGraphicElement uiGraphicElement);

    }

}