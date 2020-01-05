using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

namespace TemplateBinding {

    public class TemplateBindingTests {

        [Template("Data/TemplateBindings/TemplateBindingTest_BasicBinding#outer")]
        public class TemplateBindingTests_SimpleBinding : UIElement { }

        [Template("Data/TemplateBindings/TemplateBindingTest_BasicBinding#inner")]
        public class TemplateBindingTests_SimpleBindingInner : UIElement { }
        
        [Test]
        public void SimpleBinding() {
            
            MockApplication app = MockApplication.Setup<TemplateBindingTests_SimpleBinding>();

        }

    }

}