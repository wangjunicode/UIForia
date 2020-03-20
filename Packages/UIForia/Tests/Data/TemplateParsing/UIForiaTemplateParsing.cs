using NUnit.Framework;
using UIForia.Util;

namespace TemplateParsing {

    public class UIForiaTemplateParsing {

        [Test]
        public void ParseAttributes() {
            
            string input = "attr:x/>";
            char s;
            CharStream stream = new CharStream(input);
        
            bool found = stream.TryGetStreamUntilWithoutWhitespace(out CharStream substream, out char end, '=', '/', '>');
            
            Assert.IsTrue(found);
            Assert.AreEqual('/', end);
            
            
        }
        
    }

}