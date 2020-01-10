using NUnit.Framework;
using UIForia.Util;

namespace Util {

    public class StringUtilTests {

        [Test]
        public void CharStringBuilder_Append() {
            CharStringBuilder builder = new CharStringBuilder();
            builder.Append("content");
            builder.Append("-");
            builder.Append("content");
            builder.Append("-");
            builder.Append("content");
            Assert.AreEqual("content-content-content", builder.ToString());
        }

    }

}