using System;
using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace TemplateCompiler2Test {

    public class TestTemplateCompiler2 {

        [Template("SimpleTemplate.xml")]
        public class SimpleTemplate : UIElement {

            public string parentVal;

            public override void OnUpdate() { }

            public void HandleMouseDown(MouseInputEvent evt) { }

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
            
            MockApplication.PreCompile<SimpleTemplate>(nameof(Works));

            // Application app = MockApplication.Create(new Generated_TestApp());
            
        }

    }

}