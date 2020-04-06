using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;

namespace TemplateCompiler2Test {

    public class TestTemplateCompiler2 {

        [Template("SimpleTemplate.xml")]
        public class SimpleTemplate : UIElement { }

        [Template("ExpandedTemplate.xml")]
        public class ExpandedTemplate : UIElement { }

        [Test]
        public void Works() {

            string path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGeneratedNew"));
            
            MockApplication.PreCompile<SimpleTemplate>(path);
            
            // Application app = MockApplication.Create<SimpleTemplate>();
            
        }

    }

}