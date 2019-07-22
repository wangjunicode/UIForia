using UIForia;
using UIForia.Systems;

namespace Tests.Mocks {

    public class MockLayoutSystem : FastLayoutSystem {

        public MockLayoutSystem(Application application, IStyleSystem styleSystem)
            : base(application, styleSystem) { }

    }

}