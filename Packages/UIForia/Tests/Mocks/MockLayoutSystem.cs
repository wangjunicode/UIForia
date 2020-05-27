using UIForia;
using UIForia.Systems;

namespace Tests.Mocks {

    public class MockLayoutSystem : UIForia.Systems.LayoutSystem {

        public MockLayoutSystem(Application application, ElementSystem elementSystem)
            : base(application, elementSystem) { }

    }

}