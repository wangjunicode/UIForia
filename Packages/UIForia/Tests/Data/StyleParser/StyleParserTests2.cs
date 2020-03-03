using NUnit.Framework;
using UIForia.Style2;

namespace Tests.StyleParser {

    public class StyleParserTests2 {

        [Test]
        public void ParseStyleBlock() {
            StyleSheetParser parser = new StyleSheetParser();
            
            parser.ParseString(@"style xxx { BackgroundColor = red; }");
            
        }

    }

}