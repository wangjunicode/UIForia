using UIForia;
using UIForia.Text;

namespace Tests.Mocks {

    public class MockLayoutSystem : UIForia.Systems.LayoutSystem {

        public MockLayoutSystem(Application application, ElementSystem elementSystem, TextSystem textSystem)
            : base(application, elementSystem, textSystem) { }

    }

}