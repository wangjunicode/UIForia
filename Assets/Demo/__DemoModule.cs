using UI;
using UIForia;
using UIForia.Util;

namespace Demo {

    [RecordFilePath]
    public class DemoModule : Module {

        public override void Configure() {
            AddDependency<KlangWindowModule>();
        }

    }

}