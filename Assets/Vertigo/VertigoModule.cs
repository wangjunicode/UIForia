using UIForia;
using UIForia.Style;
using UIForia.Util;

namespace Vertigo {

    [RecordFilePath]
    public class VertigoModule : Module {

        public override void BuildCustomStyles(IStyleCodeGenerator generator) { }

        public override void Configure() {
            // add any external module dependencies here
            
            SetTemplateResolver((lookup => lookup.elementFilePath.Replace(".cs", ".xml")));
            
        }

    }

}