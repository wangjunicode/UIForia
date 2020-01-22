using System.Collections.Generic;
using NUnit.Framework;
using Tests;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;

namespace TemplateLoading {

    public class TemplateLoadingTests {

        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadNestedTemplate.xml#outer")]
        public class Outer : UIElement { }

        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadNestedTemplate.xml#inner")]
        public class Inner : UIElement { }

        [Test]
        public void LoadNestedTemplate() {
            MockApplication app = MockApplication.Setup<Outer>();
            Assert.IsInstanceOf<Outer>(app.RootElement);
            Assert.IsInstanceOf<Inner>(app.RootElement[0]);
        }

        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadNestedTemplateDefault.xml")]
        public class OuterDefault : UIElement { }

        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadNestedTemplateDefault.xml#inner")]
        public class InnerDefault : UIElement { }

        [Test]
        public void LoadNestedTemplateDefault() {
            MockApplication app = MockApplication.Setup<OuterDefault>();
            Assert.IsInstanceOf<OuterDefault>(app.RootElement);
            Assert.IsInstanceOf<InnerDefault>(app.RootElement[0]);
        }

        [Template]
        public class DefaultPathElement : UIElement { }


        [Test]
        public void ResolveUsingDefaultPath() {
            MockApplication app = MockApplication.Setup<DefaultPathElement>();
            Assert.IsInstanceOf<DefaultPathElement>(app.RootElement);
        }

        public class DefaultPathElementNoAttr : UIElement { }

        [Test]
        public void ResolveUsingDefaultPathNoAttr() {
            MockApplication app = MockApplication.Setup<DefaultPathElementNoAttr>();
            Assert.IsInstanceOf<DefaultPathElementNoAttr>(app.RootElement);
            UITextElement textElement = TestUtils.AssertInstanceOfAndReturn<UITextElement>(app.RootElement[0]);
            Assert.AreEqual("Default Path! No Attr", textElement.text.Trim());
        }

        public class DefaultPathElementNoAttrNotFound : UIElement { }

        [Test]
        public void ThrowWhenDefaultNotFound() {
            TemplateNotFoundException ex = Assert.Throws<TemplateNotFoundException>(() => { MockApplication.Setup<DefaultPathElementNoAttrNotFound>(); });
        }

        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadGeneric.xml")]
        public class TemplateLoadingTest_LoadGenericOuter : UIElement { }

        [GenericElementTypeResolvedBy(nameof(value))]
        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadGeneric.xml#generic")]
        public class TemplateLoadingTest_Generic<T> : UIElement {

            public T value;

        }

        [Template("Data/TemplateLoading/TemplateLoadingTest_LoadGeneric.xml#generic")]
        public class TemplateLoadingTest_Generic2<T, U> : UIElement {

            public T value0;
            public U value1;
            
           // [ResolveGenericTemplateArguments]
            public Dictionary<T, U> dictionary;

        }
        
        public class TemplateLoadingTest_Generic3<T, U, V> : UIElement {

            [ResolveGenericTemplateArguments]
            public U value1;
            
            [ResolveGenericTemplateArguments]
            public Dictionary<T, U> dictionary;

        }
        
        [Test]
        public void DistinctGenericTemplates() {
            MockApplication app = MockApplication.Setup<TemplateLoadingTest_LoadGenericOuter>();
            TemplateLoadingTest_LoadGenericOuter e = (TemplateLoadingTest_LoadGenericOuter) app.RootElement;
            
            app.Update();
            
            Assert.AreEqual("0.5", GetText(e[0][0]));
            Assert.AreEqual("7", GetText(e[1][0]));
            Assert.AreEqual("str", GetText(e[2][0]));
        }
        
        public TemplateSettings GetSettings<T>(string defaultPath) {
            TemplateSettings retn = MockApplication.GetDefaultSettings(defaultPath);

            retn.filePathResolver = (type, s) => "Data/TemplateLoading/TemplateLoadingTest_" + type.Name + ".xml";

            return retn;
        }

        string GetText(UIElement element) {
            UITextElement textEl = element as UITextElement;
            return textEl.text.Trim();
        }
    }

}