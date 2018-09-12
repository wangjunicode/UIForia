using Rendering;
using Src.Systems;

namespace Tests.Mocks {

    public class MockLayoutSystem : LayoutSystem {

        public MockLayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem)
            : base(textSizeCalculator, styleSystem) {
            
        }

    }

}