using Rendering;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Systems;

namespace Tests.Mocks {

    public class MockLayoutSystem : LayoutSystem {

        public MockLayoutSystem(IStyleSystem styleSystem)
            : base(styleSystem) {
        }

        public LayoutBox GetBoxForElement(UIElement element) {
            LayoutBox box;
            m_LayoutBoxMap.TryGetValue(element.id, out box);
            return box;
        }

    }

}