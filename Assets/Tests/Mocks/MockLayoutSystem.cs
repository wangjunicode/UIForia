using Rendering;
using Src.Layout;
using Src.Systems;

namespace Tests.Mocks {

    public class MockLayoutSystem : LayoutSystem2 {

        public MockLayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem)
            : base(textSizeCalculator, styleSystem) {
        }

//        public LayoutBox GetBoxForElement(UIElement element) {
//            LayoutBox box;
//            m_LayoutBoxMap.TryGetValue(element.id, out box);
//            return box;
//        }

    }

}