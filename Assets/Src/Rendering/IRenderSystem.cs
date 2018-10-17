using Src.Systems;

namespace Rendering {

    public interface IRenderSystem : ISystem {

        void MarkGeometryDirty(IDrawable element);

        void MarkMaterialDirty(IDrawable element);

    }

}