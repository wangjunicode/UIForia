using UIForia;
using UIForia.Systems;

namespace Tests.Mocks {

    public class MockLayoutSystem : LayoutSystem {

        public MockLayoutSystem(Application application, IStyleSystem styleSystem)
            : base(application, styleSystem) { }

    }

}