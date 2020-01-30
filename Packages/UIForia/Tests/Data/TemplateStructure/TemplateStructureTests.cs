using System;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

namespace TemplateStructure {

    public class TestTemplateStructure {

        [Template("Data/TemplateStructure/SlotOverride/TemplateStructure_SlotOverride_Main.xml")]
        public class TemplateStructure_SlotOverride_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride/TemplateStructure_SlotOverride_Expand.xml")]
        public class TemplateStructure_SlotOverride_Expand : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_UseOverride() {
            MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverride_Main>();

            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Expand>(app.RootElement[0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0];
            Assert.AreEqual("Override successful", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Default/TemplateStructure_SlotOverrideDefault_Main.xml")]
        public class TemplateStructure_SlotOverrideDefault_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Default/TemplateStructure_SlotOverrideDefault_Expand.xml")]
        public class TemplateStructure_SlotOverrideDefault_Expand : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_UseDefault() {
            MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverrideDefault_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverrideDefault_Expand>(app.RootElement[0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0];
            Assert.AreEqual("Override failed", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Main.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Exposer.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Exposer : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Definer.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverrideExtern_OuterOverride() {
            MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverrideExternOuterOverride_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverrideExternOuterOverride_Exposer>(app.RootElement[0]);
            Assert.IsInstanceOf<TemplateStructure_SlotOverrideExternOuterOverride_Definer>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
            Assert.AreEqual("Override from outer layer", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Main.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Definer.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_Extern_ExternDefault() {
            MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverride_Extern_ExternDefault_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer>(app.RootElement[0]);
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_ExternDefault_Definer>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
            Assert.AreEqual("Override from exposer layer", textElement.GetText().Trim());
        }

        [Template("Data/TemplateStructure/SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Main.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Main : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer : UIElement { }

        [Template("Data/TemplateStructure/SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_Extern_DefinerDefault() {
            MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverride_Extern_DefinerDefault_Main>();
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer>(app.RootElement[0]);
            Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer>(app.RootElement[0][0]);
            Assert.IsInstanceOf<UISlotDefinition>(app.RootElement[0][0][0]);
            Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
            UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
            Assert.AreEqual("Not overridden", textElement.GetText().Trim());
        }


        [Template("Data/TemplateStructure/ChildrenSlot.xml")]
        public class TemplateStructure_RadioButtonTest : UIElement {

            public string selectedRadioOption;

        }

        [Template("Data/TemplateStructure/ChildrenSlot.xml#button")]
        public class RadioButton<T> : UIElement {

            public T value;

        }

        [Template("Data/TemplateStructure/ChildrenSlot.xml#group")]
        public class RadioGroup<T> : UIElement where T : IEquatable<T> {

            public T value;

        }

        [Test]
        public void SlotOverrideContext() {
            MockApplication app = MockApplication.Setup<TemplateStructure_RadioButtonTest>();
        }

        [Template("Data/TemplateStructure/AliasStyles.xml")]
        public class TemplateStructure_AliasStyles : UIElement { }

        [Test]
        public void AliasStyles() {
            MockApplication app = MockApplication.Setup<TemplateStructure_AliasStyles>();
            TemplateStructure_AliasStyles root = (TemplateStructure_AliasStyles) app.RootElement;
        }

        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#outer")]
        public class TestTemplateStructure_UseDefaultSlotContent_Outer : UIElement { }

        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#slot_definer")]
        public class TestTemplateStructure_UseDefaultSlotContent_Inner : UIElement {

            public string str;

        }


        [Test]
        public void UseDefaultSlotContent() {
            MockApplication app = MockApplication.Setup<TestTemplateStructure_UseDefaultSlotContent_Outer>();
            TestTemplateStructure_UseDefaultSlotContent_Outer root = (TestTemplateStructure_UseDefaultSlotContent_Outer) app.RootElement;

            app.Update();

            Assert.AreEqual("from default slot", GetText(root[1][0][0]));
        }

        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#outer_override")]
        public class TestTemplateStructure_UseDefaultSlotContent_OuterOverride : UIElement { }

        [Test]
        public void OverrideSlotContent() {
            MockApplication app = MockApplication.Setup<TestTemplateStructure_UseDefaultSlotContent_OuterOverride>();
            TestTemplateStructure_UseDefaultSlotContent_OuterOverride root = (TestTemplateStructure_UseDefaultSlotContent_OuterOverride) app.RootElement;

            app.Update();

            Assert.AreEqual("from override slot", GetText(root[0][0][0]));
        }


        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#use_default_children_outer")]
        public class UseDefaultChildrenOuter : UIElement { }

        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#use_default_children_inner")]
        public class UseDefaultChildrenInner : UIElement { }

        [Test]
        public void UseDefaultChildren() {
            MockApplication app = MockApplication.Setup<UseDefaultChildrenOuter>();
            UseDefaultChildrenOuter root = (UseDefaultChildrenOuter) app.RootElement;

            app.Update();

            Assert.AreEqual("default children", GetText(root[0][0][0]));
        }

        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#override_children_outer")]
        public class OverrideChildrenOuter : UIElement {

            public string overrideBinding;

        }

        [Template("Data/TemplateStructure/TestTemplateStructure_SlotDefine.xml#override_children_inner")]
        public class OverrideChildrenInner : UIElement { }

        [Test]
        public void OverrideDefaultChildren() {
            MockApplication app = MockApplication.Setup<OverrideChildrenOuter>();
            OverrideChildrenOuter root = (OverrideChildrenOuter) app.RootElement;
            root.overrideBinding = "fromRoot";
            app.Update();

            Assert.AreEqual("fromRoot", GetText(root[0][0][0]));
        }

        public static string GetText(UIElement element) {
            UITextElement textEl = element as UITextElement;
            return textEl.text.Trim();
        }

    }

}