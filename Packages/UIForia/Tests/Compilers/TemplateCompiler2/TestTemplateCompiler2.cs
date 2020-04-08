using System;
using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;

namespace TemplateCompiler2Test {

    public class TestTemplateCompiler2 {

        [Template("SimpleTemplate.xml")]
        public class SimpleTemplate : UIElement {

            public string parentVal;

            [OnPropertyChanged(nameof(parentVal), PropertyChangedType.All)] // BindingRead or Synchronized
            public void ValueChanged(string oldValue, PropertyChangeSource changeSource) {
                switch (changeSource) {

                    case PropertyChangeSource.BindingRead:
                        break;

                    case PropertyChangeSource.Synchronized:
                        break;

                    case PropertyChangeSource.Initialized:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeSource), changeSource, null);
                }
            }

        }

        [Template("ExpandedTemplate.xml")]
        public class ExpandedTemplate : UIElement {

            public string value;

        }

        [Test]
        public void Works() {

            string path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGeneratedNew"));
            
            MockApplication.PreCompile<SimpleTemplate>(path);

            // Application app = MockApplication.Create(new Generated_TestApp());
            
        }

    }

}