using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global

namespace TemplateStructure {

    public class TestTemplateStructure {

        [Template("SlotOverride/TemplateStructure_SlotOverride_Main.xml")]
        public class TemplateStructure_SlotOverride_Main : UIElement { }

        [Template("SlotOverride/TemplateStructure_SlotOverride_Expand.xml")]
        public class TemplateStructure_SlotOverride_Expand : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_UseOverride() {
            using (MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverride_Main>()) {

                Assert.IsInstanceOf<TemplateStructure_SlotOverride_Expand>(app.RootElement[0]);
                Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0]);
                Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0]);
                UITextElement textElement = (UITextElement) app.RootElement[0][0][0];
                Assert.AreEqual("Override successful", textElement.GetText().Trim());
            }
        }

        [Template("SlotOverride_Default/TemplateStructure_SlotOverrideDefault_Main.xml")]
        public class TemplateStructure_SlotOverrideDefault_Main : UIElement { }

        [Template("SlotOverride_Default/TemplateStructure_SlotOverrideDefault_Expand.xml")]
        public class TemplateStructure_SlotOverrideDefault_Expand : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_UseDefault() {
            using (MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverrideDefault_Main>()) {
                Assert.IsInstanceOf<TemplateStructure_SlotOverrideDefault_Expand>(app.RootElement[0]);
                Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0]);
                UITextElement textElement = (UITextElement) app.RootElement[0][0][0];
                Assert.AreEqual("Override failed", textElement.GetText().Trim());
            }
        }

        [Template("SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Main.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Main : UIElement { }

        [Template("SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Exposer.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Exposer : UIElement { }

        [Template("SlotOverride_Extern_OuterOverride/TemplateStructure_SlotOverrideExtern_Definer.xml")]
        public class TemplateStructure_SlotOverrideExternOuterOverride_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverrideExtern_OuterOverride() {
            using (MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverrideExternOuterOverride_Main>()) {
                Assert.IsInstanceOf<TemplateStructure_SlotOverrideExternOuterOverride_Exposer>(app.RootElement[0]);
                Assert.IsInstanceOf<TemplateStructure_SlotOverrideExternOuterOverride_Definer>(app.RootElement[0][0]);
                Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
                Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
                UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
                Assert.AreEqual("Override from outer layer", textElement.GetText().Trim());
            }
        }

        [Template("SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Main.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Main : UIElement { }

        [Template("SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer : UIElement { }

        [Template("SlotOverride_Extern_ExternDefault/TemplateStructure_SlotOverride_Extern_ExternDefault_Definer.xml")]
        public class TemplateStructure_SlotOverride_Extern_ExternDefault_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_Extern_ExternDefault() {
            using (MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverride_Extern_ExternDefault_Main>()) {
                Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_ExternDefault_Exposer>(app.RootElement[0]);
                Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_ExternDefault_Definer>(app.RootElement[0][0]);
                Assert.IsInstanceOf<UISlotOverride>(app.RootElement[0][0][0]);
                Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
                UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
                Assert.AreEqual("Override from exposer layer", textElement.GetText().Trim());
            }
        }

        [Template("SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Main.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Main : UIElement { }

        [Template("SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer : UIElement { }

        [Template("SlotOverride_Extern_DefinerDefault/TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer.xml")]
        public class TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer : UIElement { }

        [Test]
        public void TemplateStructure_SlotOverride_Extern_DefinerDefault() {
            using (MockApplication app = MockApplication.Setup<TemplateStructure_SlotOverride_Extern_DefinerDefault_Main>()) {
                Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_DefinerDefault_Exposer>(app.RootElement[0]);
                Assert.IsInstanceOf<TemplateStructure_SlotOverride_Extern_DefinerDefault_Definer>(app.RootElement[0][0]);
                Assert.IsInstanceOf<UISlotDefinition>(app.RootElement[0][0][0]);
                Assert.IsInstanceOf<UITextElement>(app.RootElement[0][0][0][0]);
                UITextElement textElement = (UITextElement) app.RootElement[0][0][0][0];
                Assert.AreEqual("Not overridden", textElement.GetText().Trim());
            }
        }


        [Template("ChildrenSlot.xml")]
        public class TemplateStructure_RadioButtonTest : UIElement {

            public string selectedRadioOption;

        }

        [Template("ChildrenSlot.xml#button")]
        public class RadioButton<T> : UIElement {

            public T value;

        }

        [Template("ChildrenSlot.xml#group")]
        public class RadioGroup<T> : UIElement where T : IEquatable<T> {

            public T value;

        }

        [Test]
        public void SlotOverrideContext() {
            using(MockApplication app = MockApplication.Setup<TemplateStructure_RadioButtonTest>()) {}
        }

        [Template("AliasStyles.xml")]
        public class TemplateStructure_AliasStyles : UIElement { }

        [Test]
        public void AliasStyles() {
            using (MockApplication app = MockApplication.Setup<TemplateStructure_AliasStyles>()) {
                TemplateStructure_AliasStyles root = (TemplateStructure_AliasStyles) app.RootElement;
            }
        }

        [Template("TestTemplateStructure_SlotDefine.xml#outer")]
        public class TestTemplateStructure_UseDefaultSlotContent_Outer : UIElement { }

        [Template("TestTemplateStructure_SlotDefine.xml#slot_definer")]
        public class TestTemplateStructure_UseDefaultSlotContent_Inner : UIElement {

            public string str;

        }


        [Test]
        public void UseDefaultSlotContent() {
            using (MockApplication app = MockApplication.Setup<TestTemplateStructure_UseDefaultSlotContent_Outer>()) {
                TestTemplateStructure_UseDefaultSlotContent_Outer root = (TestTemplateStructure_UseDefaultSlotContent_Outer) app.RootElement;

                app.Update();

                Assert.AreEqual("from default slot", GetText(root[1][0][0]));
            }
        }

        [Template("TestTemplateStructure_SlotDefine.xml#outer_override")]
        public class TestTemplateStructure_UseDefaultSlotContent_OuterOverride : UIElement { }

        [Test]
        public void OverrideSlotContent() {
            using (MockApplication app = MockApplication.Setup<TestTemplateStructure_UseDefaultSlotContent_OuterOverride>()) {
                TestTemplateStructure_UseDefaultSlotContent_OuterOverride root = (TestTemplateStructure_UseDefaultSlotContent_OuterOverride) app.RootElement;

                app.Update();

                Assert.AreEqual("from override slot", GetText(root[0][0][0]));
            }
        }


        [Template("TestTemplateStructure_SlotDefine.xml#use_default_children_outer")]
        public class UseDefaultChildrenOuter : UIElement { }

        [Template("TestTemplateStructure_SlotDefine.xml#use_default_children_inner")]
        public class UseDefaultChildrenInner : UIElement { }

        [Test]
        public void UseDefaultChildren() {
            using (MockApplication app = MockApplication.Setup<UseDefaultChildrenOuter>()) {
                UseDefaultChildrenOuter root = (UseDefaultChildrenOuter) app.RootElement;

                app.Update();

                Assert.AreEqual("default children", GetText(root[0][0][0]));
            }
        }

        [Template("TestTemplateStructure_SlotDefine.xml#override_children_outer")]
        public class OverrideChildrenOuter : UIElement {

            public string overrideBinding;

        }

        [Template("TestTemplateStructure_SlotDefine.xml#override_children_inner")]
        public class OverrideChildrenInner : UIElement { }

        [Test]
        public void OverrideDefaultChildren() {
            using (MockApplication app = MockApplication.Setup<OverrideChildrenOuter>()) {
                OverrideChildrenOuter root = (OverrideChildrenOuter) app.RootElement;
                root.overrideBinding = "fromRoot";
                app.Update();

                Assert.AreEqual("fromRoot", GetText(root[0][0][0]));
            }
        }


        [ContainerElement]
        public class GenericThing1<T> : UIContainerElement { }

        [ContainerElement]
        public class GenericThing2<T, U> : UIContainerElement { }

        [ContainerElement]
        public class GenericThing3<T, U, V> : UIContainerElement { }

        [Template("TestTemplateStructure_ResolveGeneric.xml")]
        public class ResolveGeneric : UIElement { }

        [Test]
        public void ResolveGenericType() {
            using (MockApplication app = MockApplication.Setup<ResolveGeneric>()) {
                ResolveGeneric root = (ResolveGeneric) app.RootElement;

                app.Update();

                Assert.IsInstanceOf<GenericThing1<int>>(root[0]);
                Assert.IsInstanceOf<GenericThing1<float>>(root[1]);
                Assert.IsInstanceOf<GenericThing1<List<string>>>(root[2]);
                Assert.IsInstanceOf<GenericThing1<Dictionary<List<string>, int>>>(root[3]);
            }
        }


        [Template("TestTemplateStructure_ModifySlot.xml#require_type_main")]
        public class TestTemplateStructure_ModifySlot_RequireTypeMain : UIElement { }

        [Template("TestTemplateStructure_ModifySlot.xml#radio_group_div")]
        public class TestTemplateStructure_ModifySlot_RadioGroupDiv : UIElement { }

        [Test]
        public void ModifySlotRequireChildrenOfElementType() {
            Assert.DoesNotThrow(() => { MockApplication.Setup<TestTemplateStructure_ModifySlot_RequireTypeMain>(); });
        }

        [Template("TestTemplateStructure_ModifySlot.xml#require_type_main_invalid")]
        public class TestTemplateStructure_ModifySlot_RequireTypeMainInvalid : UIElement { }

        [Test]
        public void ModifySlotRequireChildrenOfElementTypeInvalid() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<TestTemplateStructure_ModifySlot_RequireTypeMainInvalid>(); });
            Assert.IsTrue(exception.Message.Contains($"Expected element that can be assigned to {typeof(UIDivElement)} but {typeof(UITextElement)} (<Text>) is not."));
        }

        [Template("TestTemplateStructure_ModifySlot.xml#radio_group_with_attr_main")]
        public class TestTemplateStructure_ModifySlot_RadioGroupWithAttrMain : UIElement { }

        [Template("TestTemplateStructure_ModifySlot.xml#radio_group_with_attr")]
        public class TestTemplateStructure_ModifySlot_RadioGroupWithAttr : UIElement {

            public int index;

        }

        [Test]
        public void ModifySlot_WithAttr() {
            using (MockApplication app = MockApplication.Setup<TestTemplateStructure_ModifySlot_RadioGroupWithAttrMain>()) {
                TestTemplateStructure_ModifySlot_RadioGroupWithAttrMain root = (TestTemplateStructure_ModifySlot_RadioGroupWithAttrMain) app.RootElement;
                TestTemplateStructure_ModifySlot_RadioGroupWithAttr child = (TestTemplateStructure_ModifySlot_RadioGroupWithAttr) root[0];

                app.Update();

                Assert.AreEqual("true", child[0][0].GetAttribute("selected"));
                Assert.AreEqual("false", child[0][1].GetAttribute("selected"));
                Assert.AreEqual("false", child[0][2].GetAttribute("selected"));

                child.index = 1;
                app.Update();

                Assert.AreEqual("false", child[0][0].GetAttribute("selected"));
                Assert.AreEqual("true", child[0][1].GetAttribute("selected"));
                Assert.AreEqual("false", child[0][2].GetAttribute("selected"));
            }
        }

        [Test]
        public void ModifySlot_TypedWithField() {
            using (MockApplication app = MockApplication.Setup<TestTemplateStructure_ModifySlot_TypedWithFieldMain>()) {
                TestTemplateStructure_ModifySlot_TypedWithFieldMain root = (TestTemplateStructure_ModifySlot_TypedWithFieldMain) app.RootElement;

                TestTemplateStructure_ModifySlot_TypedWithFieldInner inner = (TestTemplateStructure_ModifySlot_TypedWithFieldInner) root[0];

                TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing c0 = inner[0][0] as TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing;
                TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing c1 = inner[0][1] as TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing;
                TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing c2 = inner[0][2] as TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing;

                inner.value = 10;

                app.Update();

                Assert.AreEqual(c0.typedField, 0 * inner.value);
                Assert.AreEqual(c1.typedField, 1 * inner.value);
                Assert.AreEqual(c2.typedField, 2 * inner.value);

                inner.value = 20;

                app.Update();

                Assert.AreEqual(c0.typedField, 0 * inner.value);
                Assert.AreEqual(c1.typedField, 1 * inner.value);
                Assert.AreEqual(c2.typedField, 2 * inner.value);
            }
        }

        [Test]
        public void ModifySlot_RequireGeneric() {
            using (MockApplication app = MockApplication.Setup<TestTemplateStructure_ModifySlot_RequireGenericMain>()) {
                TestTemplateStructure_ModifySlot_RequireGenericMain root = (TestTemplateStructure_ModifySlot_RequireGenericMain) app.RootElement;

                TestTemplateStructure_ModifySlot_RequireGenericInner<string> inner = (TestTemplateStructure_ModifySlot_RequireGenericInner<string>) root[0];

                TestTemplateStructure_ModifySlot_RequireGenericThing<string> c0 = inner[0][0] as TestTemplateStructure_ModifySlot_RequireGenericThing<string>;
                TestTemplateStructure_ModifySlot_RequireGenericThing<string> c1 = inner[0][1] as TestTemplateStructure_ModifySlot_RequireGenericThing<string>;
                TestTemplateStructure_ModifySlot_RequireGenericThing<string> c2 = inner[0][2] as TestTemplateStructure_ModifySlot_RequireGenericThing<string>;
            }
        }

        public static string GetText(UIElement element) {
            UITextElement textEl = element as UITextElement;
            return textEl.text.Trim();
        }

    }

    [Template("TestTemplateStructure_ModifySlot.xml#type_with_field_main")]
    public class TestTemplateStructure_ModifySlot_TypedWithFieldMain : UIElement { }

    [Template("TestTemplateStructure_ModifySlot.xml#type_with_field_inner")]
    public class TestTemplateStructure_ModifySlot_TypedWithFieldInner : UIElement {

        public int value;

    }

    [ContainerElement]
    public class TestTemplateStructure_ModifySlot_TypedWithFieldInnerThing : UIContainerElement {

        public int typedField;

    }

    public interface IInterface {

        string Value { get; set; }

    }
    
    [Template("TestTemplateStructure_ModifySlot.xml#require_generic_main")]
    public class TestTemplateStructure_ModifySlot_RequireGenericMain : UIElement { }

    [Template("TestTemplateStructure_ModifySlot.xml#require_generic_inner")]
    public class TestTemplateStructure_ModifySlot_RequireGenericInner<T> : UIElement {

        public T value;

    }

    [ContainerElement]
    public class TestTemplateStructure_ModifySlot_RequireGenericThing<T> : UIContainerElement {

        public T field;

    }

}