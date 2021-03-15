using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

namespace Tests.Data.Image {
    public class ImageTest {


        [Template("Data/Image/ImageTest_SrcInvalid.xml")]
        class ImageTest_SrcInvalid : UIElement {
            
        }
        
        [Test]
        public void Image_SrcInvalid() {
            MockApplication app = MockApplication.Setup<ImageTest_SrcInvalid>();
            Assert.DoesNotThrow(() => app.Update());
        }
    }
}