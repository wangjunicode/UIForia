using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.UIInput;
using UnityEngine;
using Application = UnityEngine.Application;

namespace TemplateBinding {

    public class TemplateBindingTests {

        private bool usePreCompiledTemplates = true;
        private bool generateCode = true;

        public MockApplication Setup<T>(string appName = null) {
            if (appName == null) {
                StackTrace stackTrace = new StackTrace();
                appName = stackTrace.GetFrame(1).GetMethod().Name;
            }

            TemplateSettings settings = new TemplateSettings();
            settings.rootType = typeof(T);
            settings.applicationName = appName;
            settings.assemblyName = GetType().Assembly.GetName().Name;
            settings.outputPath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
            settings.codeFileExtension = "generated.xml.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
            settings.templateResolutionBasePath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests");

            if (generateCode) {
                TemplateCodeGenerator.Generate(typeof(T), settings);
            }

            return MockApplication.Setup(settings, usePreCompiledTemplates);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_BasicBinding.xml")]
        public class TemplateBindingTest_BasicBindingOuter : UIElement { }

        [Template("Data/TemplateBindings/TemplateBindingTest_BasicBinding.xml#inner")]
        public class TemplateBindingTest_BasicBindingInner : UIElement {

            public int intVal = 5;

        }

        [Test]
        public void SimpleBinding() {
            MockApplication app = Setup<TemplateBindingTest_BasicBindingOuter>();
            TemplateBindingTest_BasicBindingInner inner = (TemplateBindingTest_BasicBindingInner) app.RootElement[0];
            Assert.AreEqual(5, inner.intVal);
            app.Update();
            Assert.AreEqual(25, inner.intVal);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_CreatedBinding.xml")]
        public class TemplateBindingTest_CreatedBindingOuter : UIElement {

            public int value = 15;

            public int GetValue() {
                return value;
            }

        }

        [Template("Data/TemplateBindings/TemplateBindingTest_CreatedBinding.xml#inner")]
        public class TemplateBindingTest_CreatedBindingInner : UIElement {

            public int intVal;

        }

        [Test]
        public void CreatedBinding() {
            MockApplication app = Setup<TemplateBindingTest_CreatedBindingOuter>();

            TemplateBindingTest_CreatedBindingOuter outer = (TemplateBindingTest_CreatedBindingOuter) app.RootElement;
            TemplateBindingTest_CreatedBindingInner inner = (TemplateBindingTest_CreatedBindingInner) app.RootElement[0];

            int original = outer.value;

            Assert.AreEqual(original, inner.intVal);
            outer.value = 25;
            app.Update();
            Assert.AreEqual(original, inner.intVal);
            Assert.AreEqual(25, outer.GetValue());
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_AttributeBinding.xml")]
        public class TemplateBindingTest_AttributeBinding : UIElement {

            public int intVal = 18;

        }

        [Test]
        public void AttributeBinding() {
            MockApplication app = Setup<TemplateBindingTest_AttributeBinding>();

            TemplateBindingTest_AttributeBinding outer = (TemplateBindingTest_AttributeBinding) app.RootElement;
            UIElement inner = outer[0];

            Assert.AreEqual("attr-value", inner.GetAttribute("someAttr"));
            Assert.AreEqual("", inner.GetAttribute("dynamicAttr"));

            app.Update();

            Assert.AreEqual("attr-value", inner.GetAttribute("someAttr"));
            Assert.AreEqual("dynamic18", inner.GetAttribute("dynamicAttr"));
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_MouseBinding.xml")]
        public class TemplateBindingTest_MouseBindingBinding : UIElement {

            public string output_NoParams;
            public string output_EvtParam;
            public string output_MixedParams;
            public string output_NoEvtParam;
            public string output_anonWithEvt;

            public void HandleMouseClick_NoParams() {
                output_NoParams = "No Params Was Called";
            }

            public void HandleMouseClick_EvtParam(MouseInputEvent evt) {
                output_EvtParam = $"EvtParam was called {evt.MousePosition.x}, {evt.MousePosition.y}";
            }

            public void HandleMouseClick_MixedParams(MouseInputEvent evt, int param) {
                output_MixedParams = $"MixedParams was called {evt.MousePosition.x}, {evt.MousePosition.y} param = {param}";
            }

            public void HandleMouseClick_NoEvtParam(string str, int param) {
                output_NoEvtParam = $"NoEvtParam was called str = {str} param = {param}";
            }

            public float output_value;

            public void SetValue(float value) {
                output_value = value;
            }

        }

        public class TemplateBindingTest_ThingWithMouseHandler : UIContainerElement {

            public string downNoParam;
            public string downParam;

            [OnMouseDown]
            public void HandleMouseDown_NoParam() {
                downNoParam = "was down";
            }

            [OnMouseDown]
            public void HandleMouseDown_Param(MouseInputEvent evt) {
                downParam = "down " + evt.MousePosition.y;
            }

        }

        [Test]
        public void MouseHandlerBinding() {
            MockApplication app = Setup<TemplateBindingTest_MouseBindingBinding>();
            TemplateBindingTest_MouseBindingBinding e = (TemplateBindingTest_MouseBindingBinding) app.RootElement;
            app.SetScreenSize(1000, 1000);

            app.Update();

            app.InputSystem.MouseDown(new Vector2(50, 50));

            app.Update();

            Assert.AreEqual("No Params Was Called", e.output_NoParams);

            app.Update();

            app.InputSystem.MouseDown(new Vector2(50, 150));

            app.Update();

            Assert.AreEqual("EvtParam was called 50, 150", e.output_EvtParam);

            app.InputSystem.MouseDown(new Vector2(50, 250));

            app.Update();

            Assert.AreEqual("MixedParams was called 50, 250 param = 250", e.output_MixedParams);

            app.InputSystem.MouseDown(new Vector2(50, 350));

            app.Update();

            Assert.AreEqual("NoEvtParam was called str = string goes here param = 250", e.output_NoEvtParam);

            app.InputSystem.MouseDown(new Vector2(50, 450));

            app.Update();

            TemplateBindingTest_ThingWithMouseHandler thing = e["mousedownthing"] as TemplateBindingTest_ThingWithMouseHandler;
            Assert.AreEqual("was down", thing.downNoParam);
            Assert.AreEqual("down 450", thing.downParam);

            app.InputSystem.MouseDown(new Vector2(50, 550));

            app.Update();

            Assert.AreEqual(550, e.output_value);

            app.InputSystem.MouseDown(new Vector2(50, 650));

            app.Update();

            Assert.AreEqual(650, e.output_value);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_KeyboardBinding.xml")]
        public class TemplateBindingTest_KeyboardBinding : UIElement {

            public string output_NoParams;
            public string output_EvtParam;
            public string output_MixedParams;
            public string output_NoEvtParam;

            public void HandleKeyDown_NoParams() {
                output_NoParams = "No Params Was Called";
            }

            public void HandleKeyDown_EvtParam(KeyboardInputEvent evt) {
                output_EvtParam = $"EvtParam was called {evt.character}";
            }

            public void HandleKeyDown_MixedParams(KeyboardInputEvent evt, int param) {
                output_MixedParams = $"MixedParams was called {evt.character} param = {param}";
            }

            public void HandleKeyDown_NoEvtParam(string str, int param) {
                output_NoEvtParam = $"NoEvtParam was called str = {str} param = {param}";
            }

            public float output_value;

            public void SetValue(float value) {
                output_value = value;
            }

        }

        [Test]
        public void KeyboardHandlerBinding() {
            MockApplication app = Setup<TemplateBindingTest_MouseBindingBinding>();
            TemplateBindingTest_MouseBindingBinding e = (TemplateBindingTest_MouseBindingBinding) app.RootElement;

            throw new NotImplementedException("Keyboard input needs a re-write");
            // app.Update();

            // app.InputSystem.SetKeyDown('a');

            // app.Update();

            // Assert.AreEqual("No Params Was Called", e.output_NoParams);
            //
            // app.Update();
            //
            // app.InputSystem.KeyDown('a');
            //
            // app.Update();
            //
            // Assert.AreEqual("EvtParam was called 50, 150", e.output_EvtParam);
            //
            // app.InputSystem.MouseDown(new Vector2(50, 250));
            //
            // app.Update();
            //
            // Assert.AreEqual("MixedParams was called 50, 250 param = 250", e.output_MixedParams);
            //
            // app.InputSystem.MouseDown(new Vector2(50, 350));
            //
            // app.Update();
            //
            // Assert.AreEqual("NoEvtParam was called str = string goes here param = 250", e.output_NoEvtParam);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_ConditionalBinding.xml")]
        public class TemplateBindingTest_ConditionalBinding : UIElement {

            public bool condition;

            public bool SomeCondition() {
                return condition;
            }

        }

        [Test]
        public void ConditionBinding() {
            MockApplication app = Setup<TemplateBindingTest_ConditionalBinding>();
            TemplateBindingTest_ConditionalBinding e = (TemplateBindingTest_ConditionalBinding) app.RootElement;

            app.Update();

            UITextElement textElementTrue = e[0] as UITextElement;
            UITextElement textElementFalse = e[1] as UITextElement;
            Assert.IsTrue(textElementFalse.isEnabled);
            Assert.IsTrue(textElementTrue.isDisabled);

            e.condition = true;
            app.Update();

            Assert.IsTrue(textElementFalse.isDisabled);
            Assert.IsTrue(textElementTrue.isEnabled);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_StyleBinding.xml")]
        public class TemplateBindingTest_StyleBinding : UIElement { }

        [Test]
        public void StyleBinding() {
            MockApplication app = Setup<TemplateBindingTest_StyleBinding>();
            TemplateBindingTest_StyleBinding e = (TemplateBindingTest_StyleBinding) app.RootElement;

            app.Update();

            Assert.AreEqual(Color.red, e[0].style.BackgroundColor);
            Assert.AreEqual(new OffsetMeasurement(53, OffsetMeasurementUnit.ViewportWidth), e[0].style.Hover.TransformPositionX);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_DynamicStyleBinding.xml")]
        public class TemplateBindingTest_DynamicStyleBinding : UIElement {

            public bool useDynamic;

            public UIStyleGroupContainer dynamicStyleReference;

            public string[] styleList;

            public string[] GetStyleList() {
                return styleList;
            }

        }

        [Test]
        public void DynamicStyleBinding() {
            MockApplication app = Setup<TemplateBindingTest_DynamicStyleBinding>();
            TemplateBindingTest_DynamicStyleBinding e = (TemplateBindingTest_DynamicStyleBinding) app.RootElement;

            e.useDynamic = false;

            app.Update();

            UIElement d0 = e[0];

            List<UIStyleGroupContainer> d0Styles = d0.style.GetBaseStyles();
            Assert.AreEqual(2, d0Styles.Count);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-1"), d0Styles[0]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-2"), d0Styles[1]);

            e.useDynamic = true;

            app.Update();

            d0Styles = d0.style.GetBaseStyles();

            Assert.AreEqual(3, d0Styles.Count);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-1"), d0Styles[0]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-2"), d0Styles[1]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("dynamicStyle"), d0Styles[2]);

            e.useDynamic = false;
            e.styleList = new[] {"list-1", "list-2"};

            app.Update();

            d0Styles = d0.style.GetBaseStyles();

            Assert.AreEqual(4, d0Styles.Count);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-1"), d0Styles[0]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-2"), d0Styles[1]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("list-1"), d0Styles[2]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("list-2"), d0Styles[3]);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_UnresolvedDynamicStyle.xml")]
        public class TemplateBindingTest_UnresolvedDynamicStyle : UIElement {

            public int val;

        }

        [Test]
        public void DynamicStyleBinding_UnresolvedDynamic() {
            MockApplication app = Setup<TemplateBindingTest_UnresolvedDynamicStyle>();
            TemplateBindingTest_UnresolvedDynamicStyle e = (TemplateBindingTest_UnresolvedDynamicStyle) app.RootElement;

            e.val = 1;
            app.Update();

            UIElement d0 = e[0];

            List<UIStyleGroupContainer> d0Styles = d0.style.GetBaseStyles();
            Assert.AreEqual(2, d0Styles.Count);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-1"), d0Styles[0]);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-2"), d0Styles[1]);

            e.val = 300;

            app.Update();
            d0Styles = d0.style.GetBaseStyles();
            Assert.AreEqual(1, d0Styles.Count);
            Assert.AreEqual(e.templateMetaData.ResolveStyleByName("style-2"), d0Styles[0]);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_ContextVariable.xml")]
        public class TemplateBindingTest_ContextVariable : UIElement { }

        [Template("Data/TemplateBindings/TemplateBindingTest_ContextVariable.xml#slotexposer")]
        public class TemplateBindingTest_ContextVariable_SlotExposer : UIElement { }

        [Test]
        public void ContextVariableBinding() {
            MockApplication app = Setup<TemplateBindingTest_ContextVariable>();
            TemplateBindingTest_ContextVariable e = (TemplateBindingTest_ContextVariable) app.RootElement;

            app.Update();

            UITextElement textElement = app.RootElement[0][0] as UITextElement;

            Assert.AreEqual("answer = 25", textElement.text.Trim());

            UIElement nested = e["text-el"];
            Assert.NotNull(nested);

            UITextElement nestedTextEl = nested as UITextElement;
            Assert.AreEqual("slot answer is = 50", nestedTextEl.text.Trim());
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#out_of_scope")]
        public class TemplateBindingTest_ContextVariableOutOfScope : UIElement { }

        [Test]
        public void LocalContextVariable() {
            CompileException exception = Assert.Throws<CompileException>(() => { Setup<TemplateBindingTest_ContextVariableOutOfScope>(nameof(TemplateBindingTest_ContextVariableOutOfScope)); });
            Assert.AreEqual(CompileException.UnknownAlias("cvar0").Message, exception.Message);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#use_alias")]
        public class TemplateBindingTest_ContextVariable_UseAlias : UIElement { }

        [Test]
        public void ContextVariable_UseAlias() {
            MockApplication app = Setup<TemplateBindingTest_ContextVariable_UseAlias>();
            TemplateBindingTest_ContextVariable_UseAlias e = (TemplateBindingTest_ContextVariable_UseAlias) app.RootElement;

            app.Update();

            Assert.AreEqual("var 0", GetText(e["outer"][0]));
            Assert.AreEqual("var 0", GetText(e["nested"][0]));
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#use_alias_out_of_scope")]
        public class TemplateBindingTest_ContextVariable_UseAliasOutOfScope : UIElement { }

        [Test]
        public void ContextVariable_UseAliasOutOfScope() {
            CompileException exception = Assert.Throws<CompileException>(() => Setup<TemplateBindingTest_ContextVariable_UseAliasOutOfScope>());
            Assert.AreEqual(CompileException.UnknownAlias("custom").Message, exception.Message);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#use_alias_on_own_context")]
        public class TemplateBindingTest_ContextVariable_UseAliasOnOwnContext : UIElement { }

        [Test]
        public void ContextVariable_UseAliasOnOwnContext() {
            CompileException exception = Assert.Throws<CompileException>(() => Setup<TemplateBindingTest_ContextVariable_UseAliasOnOwnContext>());
            Assert.AreEqual(CompileException.UnknownAlias("custom").Message, exception.Message);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#not_exposed_inner")]
        public class TemplateBindingTest_ContextVariable_NonExposed_NotAvailable_Inner : UIElement { }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#not_exposed_outer")]
        public class TemplateBindingTest_ContextVariable_NonExposed_NotAvailable_Outer : UIElement { }

        [Test]
        public void ContextVariable_NonExposed_NotAvailable() {
            CompileException exception = Assert.Throws<CompileException>(() => Setup<TemplateBindingTest_ContextVariable_NonExposed_NotAvailable_Outer>(nameof(TemplateBindingTest_ContextVariable_NonExposed_NotAvailable_Outer)));
            Assert.AreEqual(CompileException.UnknownAlias("thing").Message, exception.Message);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#expose_context_var_slotted_outer")]
        public class TemplateBindingTest_ContextVariable_Expose_Slotted_Outer : UIElement {

            public string value = "val";

        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#expose_context_var_slotted_inner")]
        public class TemplateBindingTest_ContextVariable_Expose_Slotted_Inner : UIElement {

            public string variable0 = "var 0";
            public string variable1 = "var 1";

        }

        [Test]
        public void ContextVariable_Expose_Slotted() {
            MockApplication app = Setup<TemplateBindingTest_ContextVariable_Expose_Slotted_Outer>();
            TemplateBindingTest_ContextVariable_Expose_Slotted_Outer e = (TemplateBindingTest_ContextVariable_Expose_Slotted_Outer) app.RootElement;

            app.Update();

            Assert.AreEqual("var 0 + var 1hello", GetText(e["text"]));
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_LocalContextVariable.xml#expose_context_out_of_scope")]
        public class TemplateBindingTest_ContextVariable_Expose_OutOfScope : UIElement {

            public string value = "val";

        }

        [Test]
        public void ContextVariable_Expose_OutOfScope() {
            CompileException exception = Assert.Throws<CompileException>(() => Setup<TemplateBindingTest_ContextVariable_Expose_OutOfScope>());
            Assert.AreEqual(CompileException.UnknownAlias("variable0").Message, exception.Message);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_RepeatTemplate.xml#repeat_count")]
        public class TemplateBindingTest_RepeatCount : UIElement {

            public int count;

        }

        [Test]
        public void RepeatCount() {
            MockApplication app = Setup<TemplateBindingTest_RepeatCount>();
            TemplateBindingTest_RepeatCount e = (TemplateBindingTest_RepeatCount) app.RootElement;

            e.count = 5;

            app.Update();

            Assert.AreEqual(5, e[0].children.size);
            Assert.AreEqual("repeat me 0", GetText(e[0][0]));
            Assert.AreEqual("repeat me 1", GetText(e[0][1]));
            Assert.AreEqual("repeat me 2", GetText(e[0][2]));
            Assert.AreEqual("repeat me 3", GetText(e[0][3]));
            Assert.AreEqual("repeat me 4", GetText(e[0][4]));

            e.count = 7;

            var e0 = e[0][0];
            var e1 = e[0][1];
            var e2 = e[0][2];
            var e3 = e[0][3];
            var e4 = e[0][4];

            app.Update();

            Assert.AreEqual(7, e[0].children.size);
            Assert.AreEqual("repeat me 0", GetText(e[0][0]));
            Assert.AreEqual("repeat me 1", GetText(e[0][1]));
            Assert.AreEqual("repeat me 2", GetText(e[0][2]));
            Assert.AreEqual("repeat me 3", GetText(e[0][3]));
            Assert.AreEqual("repeat me 4", GetText(e[0][4]));
            Assert.AreEqual("repeat me 5", GetText(e[0][5]));
            Assert.AreEqual("repeat me 6", GetText(e[0][6]));

            Assert.AreEqual(e0, e[0][0]);
            Assert.AreEqual(e1, e[0][1]);
            Assert.AreEqual(e2, e[0][2]);
            Assert.AreEqual(e3, e[0][3]);
            Assert.AreEqual(e4, e[0][4]);

            e.count = 2;

            app.Update();

            Assert.AreEqual(2, e[0].children.size);
            Assert.AreEqual("repeat me 0", GetText(e[0][0]));
            Assert.AreEqual("repeat me 1", GetText(e[0][1]));
            Assert.AreEqual(e0, e[0][0]);
            Assert.AreEqual(e1, e[0][1]);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_RepeatTemplate.xml#repeat_list")]
        public class TemplateBindingTest_RepeatList : UIElement {

            public IList<Vector3> data;

        }

        [Test]
        public void RepeatList_ArrayData() {
            MockApplication app = Setup<TemplateBindingTest_RepeatList>();
            TemplateBindingTest_RepeatList e = (TemplateBindingTest_RepeatList) app.RootElement;

            e.data = new[] {
                Vector3.zero,
                Vector3.one,
                Vector3.forward,
                Vector3.back
            };

            app.Update();

            Assert.AreEqual(4, e[0].children.size);
            Assert.AreEqual("repeat me " + Vector3.zero, GetText(e[0][0]));
            Assert.AreEqual("repeat me " + Vector3.one, GetText(e[0][1]));
            Assert.AreEqual("repeat me " + Vector3.forward, GetText(e[0][2]));
            Assert.AreEqual("repeat me " + Vector3.back, GetText(e[0][3]));

            UIElement c0 = e[0][0];
            UIElement c1 = e[0][1];
            UIElement c2 = e[0][2];
            UIElement c3 = e[0][3];

            e.data = new[] {
                Vector3.zero,
                Vector3.one,
                Vector3.forward,
                Vector3.back,
                Vector3.left
            };

            app.Update();

            Assert.AreEqual(5, e[0].children.size);
            Assert.AreEqual("repeat me " + Vector3.zero, GetText(e[0][0]));
            Assert.AreEqual("repeat me " + Vector3.one, GetText(e[0][1]));
            Assert.AreEqual("repeat me " + Vector3.forward, GetText(e[0][2]));
            Assert.AreEqual("repeat me " + Vector3.back, GetText(e[0][3]));
            Assert.AreEqual("repeat me " + Vector3.left, GetText(e[0][4]));
            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c1, e[0][1]);
            Assert.AreEqual(c2, e[0][2]);
            Assert.AreEqual(c3, e[0][3]);

            e.data = new[] {
                Vector3.zero,
                Vector3.one,
            };

            app.Update();

            Assert.AreEqual(2, e[0].children.size);
            Assert.AreEqual("repeat me " + Vector3.zero, GetText(e[0][0]));
            Assert.AreEqual("repeat me " + Vector3.one, GetText(e[0][1]));
            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c1, e[0][1]);
        }

        [Test]
        public void RepeatList_ListData() {
            MockApplication app = Setup<TemplateBindingTest_RepeatList>();
            TemplateBindingTest_RepeatList e = (TemplateBindingTest_RepeatList) app.RootElement;

            e.data = new List<Vector3>(new[] {
                Vector3.zero,
                Vector3.one,
                Vector3.forward,
                Vector3.back
            });

            app.Update();

            Assert.AreEqual(4, e[0].children.size);
            Assert.AreEqual("repeat me " + Vector3.zero, GetText(e[0][0]));
            Assert.AreEqual("repeat me " + Vector3.one, GetText(e[0][1]));
            Assert.AreEqual("repeat me " + Vector3.forward, GetText(e[0][2]));
            Assert.AreEqual("repeat me " + Vector3.back, GetText(e[0][3]));

            UIElement c0 = e[0][0];
            UIElement c1 = e[0][1];
            UIElement c2 = e[0][2];
            UIElement c3 = e[0][3];

            e.data.Add(Vector3.left);

            app.Update();

            Assert.AreEqual(5, e[0].children.size);
            Assert.AreEqual("repeat me " + Vector3.zero, GetText(e[0][0]));
            Assert.AreEqual("repeat me " + Vector3.one, GetText(e[0][1]));
            Assert.AreEqual("repeat me " + Vector3.forward, GetText(e[0][2]));
            Assert.AreEqual("repeat me " + Vector3.back, GetText(e[0][3]));
            Assert.AreEqual("repeat me " + Vector3.left, GetText(e[0][4]));
            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c1, e[0][1]);
            Assert.AreEqual(c2, e[0][2]);
            Assert.AreEqual(c3, e[0][3]);

            e.data.RemoveAt(e.data.Count - 1);
            e.data.RemoveAt(e.data.Count - 1);
            e.data.RemoveAt(e.data.Count - 1);

            app.Update();

            Assert.AreEqual(2, e[0].children.size);
            Assert.AreEqual("repeat me " + Vector3.zero, GetText(e[0][0]));
            Assert.AreEqual("repeat me " + Vector3.one, GetText(e[0][1]));
            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c1, e[0][1]);

            e.data.Clear();

            app.Update();

            Assert.AreEqual(0, e[0].children.size);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_RepeatTemplate.xml#repeat_list_key_fn")]
        public class TemplateBindingTest_RepeatList_Key : UIElement {

            public IList<Vector3> data;

        }

        [Test]
        public void RepeatList_KeyFn() {
            MockApplication app = Setup<TemplateBindingTest_RepeatList_Key>();
            TemplateBindingTest_RepeatList_Key e = (TemplateBindingTest_RepeatList_Key) app.RootElement;

            e.data = new List<Vector3>(new[] {
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(3, 0, 0),
            });

            app.Update();

            Assert.AreEqual(3, e[0].children.size);
            Assert.AreEqual("repeat me 1", GetText(e[0][0]));
            Assert.AreEqual("repeat me 2", GetText(e[0][1]));
            Assert.AreEqual("repeat me 3", GetText(e[0][2]));

            UIElement c0 = e[0][0];
            UIElement c1 = e[0][1];
            UIElement c2 = e[0][2];

            e.data.Insert(1, new Vector3(4, 0, 0));

            app.Update();

            Assert.AreEqual(4, e[0].children.size);
            Assert.AreEqual("repeat me 1", GetText(e[0][0]));
            Assert.AreEqual("repeat me 4", GetText(e[0][1]));
            Assert.AreEqual("repeat me 2", GetText(e[0][2]));
            Assert.AreEqual("repeat me 3", GetText(e[0][3]));

            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c1, e[0][2]);
            Assert.AreEqual(c2, e[0][3]);

            UIElement c3 = e[0][1];

            e.data.RemoveAt(2);

            app.Update();

            Assert.AreEqual(3, e[0].children.size);
            Assert.AreEqual("repeat me 1", GetText(e[0][0]));
            Assert.AreEqual("repeat me 4", GetText(e[0][1]));
            Assert.AreEqual("repeat me 3", GetText(e[0][2]));

            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c3, e[0][1]);
            Assert.AreEqual(c2, e[0][2]);

            // reorder data
            e.data.RemoveAt(1);
            e.data.Add(new Vector3(4, 0, 0));

            app.Update();

            Assert.AreEqual(3, e[0].children.size);
            Assert.AreEqual("repeat me 1", GetText(e[0][0]));
            Assert.AreEqual("repeat me 3", GetText(e[0][1]));
            Assert.AreEqual("repeat me 4", GetText(e[0][2]));

            Assert.AreEqual(c0, e[0][0]);
            Assert.AreEqual(c2, e[0][1]);
            Assert.AreEqual(c3, e[0][2]);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_RepeatTemplate.xml#repeat_multi_child")]
        public class TemplateBindingTest_RepeatMultiChild : UIElement {

            public IList<Vector3> data;

        }

        [Test]
        public void RepeatMultiChild() {
            MockApplication app = Setup<TemplateBindingTest_RepeatMultiChild>();
            TemplateBindingTest_RepeatMultiChild e = (TemplateBindingTest_RepeatMultiChild) app.RootElement;
            e.data = new [] {new Vector3(1, 2, 3), new Vector3(4, 5, 6)};
            
            app.Update();
            
            Assert.AreEqual(2, e[0].ChildCount);
            Assert.AreEqual("repeat me 1", GetText(e[0][0][0][0]));
            Assert.AreEqual("3", GetText(e[0][0][1][0]));
            Assert.AreEqual("repeat me 4", GetText(e[0][1][0][0]));
            Assert.AreEqual("6", GetText(e[0][1][1][0]));
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_SyncBinding.xml#sync")]
        public class TemplateBindingTest_SyncBinding_Sync : UIElement {

            public string syncedValue;

        }

        [Template("Data/TemplateBindings/TemplateBindingTest_SyncBinding.xml#fake_input")]
        public class TemplateBindingTest_SyncBinding_FakeInput : UIElement {

            public string value;

            public override void OnUpdate() {
                value = value + "__afterSync";
            }

        }

        [Test]
        public void SyncBinding_Sync() {
            MockApplication app = Setup<TemplateBindingTest_SyncBinding_Sync>();
            TemplateBindingTest_SyncBinding_Sync e = (TemplateBindingTest_SyncBinding_Sync) app.RootElement;
            TemplateBindingTest_SyncBinding_FakeInput child = (TemplateBindingTest_SyncBinding_FakeInput) e[0];

            e.syncedValue = "synced";

            app.Update();

            Assert.AreEqual("synced__afterSync", child.value);
            Assert.AreEqual("synced__afterSync", e.syncedValue);
        }


        [Template("Data/TemplateBindings/TemplateBindingTest_InnerContext.xml")]
        public class TemplateBindingTest_InnerContext_Outer : UIElement { }


        [Template("Data/TemplateBindings/TemplateBindingTest_InnerContext.xml#dynamic_styled")]
        public class TemplateBindingTest_InnerContext_Inner : UIElement {

            public string dynamicFromInner;

        }

        [Test]
        public void InnerContext_Styled() {
            MockApplication app = Setup<TemplateBindingTest_InnerContext_Outer>();
            TemplateBindingTest_InnerContext_Outer e = (TemplateBindingTest_InnerContext_Outer) app.RootElement;
            TemplateBindingTest_InnerContext_Inner child = (TemplateBindingTest_InnerContext_Inner) e[0];

            child.dynamicFromInner = "one";

            app.Update();

            List<UIStyleGroupContainer> styles = child.style.GetBaseStyles();

            Assert.AreEqual(2, styles.Count);
            Assert.AreEqual("one", styles[0].name);
            Assert.AreEqual("from-outer", styles[1].name);

            child.dynamicFromInner = "two";

            app.Update();
            styles = child.style.GetBaseStyles();
            Assert.AreEqual(2, styles.Count);
            Assert.AreEqual("two", styles[0].name);
            Assert.AreEqual("from-outer", styles[1].name);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_OnChange.xml")]
        public class TemplateBindingTest_OnChange_Outer : UIElement {

            public string myValue;
            public string myValueAfterChange;

            public void HandleChange() {
                myValueAfterChange = myValue + " changed";
            }

        }


        public class TemplateBindingTest_OnChange_Inner : UIContainerElement {

            public string value;

            public override void OnAfterPropertyBindings() {
                value += "__changed";
            }

        }

        [Test]
        public void OnChange() {
            MockApplication app = Setup<TemplateBindingTest_OnChange_Outer>();
            TemplateBindingTest_OnChange_Outer e = (TemplateBindingTest_OnChange_Outer) app.RootElement;
            TemplateBindingTest_OnChange_Inner child = (TemplateBindingTest_OnChange_Inner) e[0];

            e.myValue = "baseVal";

            app.Update();

            Assert.AreEqual("baseVal changed", e.myValueAfterChange);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_OnChange.xml#with_old_value")]
        public class TemplateBindingTest_OnChange_WithOldValue : UIElement {

            public string myValue;
            public string oldValue;

            public void HandleChange(string oldValue) {
                this.oldValue = oldValue;
            }

        }


        [Test]
        public void OnChange_WithOldValue() {
            MockApplication app = Setup<TemplateBindingTest_OnChange_WithOldValue>();
            TemplateBindingTest_OnChange_WithOldValue e = (TemplateBindingTest_OnChange_WithOldValue) app.RootElement;
            TemplateBindingTest_OnChange_Inner child = (TemplateBindingTest_OnChange_Inner) e[0];

            e.myValue = "baseVal";

            app.Update();

            Assert.AreEqual("baseVal__changed", child.value);
            Assert.AreEqual("baseVal", e.oldValue);
        }


        [Template("Data/TemplateBindings/TemplateBindingTest_OnChange.xml#with_new_value")]
        public class TemplateBindingTest_OnChange_WithNewValue : UIElement {

            public string myValue;
            public string newValue;

            public void HandleChange(string newValue) {
                this.newValue = newValue;
            }

        }

        [Test]
        public void OnChange_WithNewValue() {
            MockApplication app = Setup<TemplateBindingTest_OnChange_WithNewValue>();
            TemplateBindingTest_OnChange_WithNewValue e = (TemplateBindingTest_OnChange_WithNewValue) app.RootElement;
            TemplateBindingTest_OnChange_Inner child = (TemplateBindingTest_OnChange_Inner) e[0];

            e.myValue = "baseVal";

            app.Update();

            Assert.AreEqual("baseVal__changed", child.value);
            Assert.AreEqual("baseVal__changed", e.newValue);
        }

        [Template("Data/TemplateBindings/TemplateBindingTest_OnChange.xml#with_sync")]
        public class TemplateBindingTest_OnChange_WithSync : UIElement {

            public string myValue;
            public string newValue;
            public string oldValue;

            public void HandleChange(string newValue, string oldValue) {
                this.newValue = newValue;
                this.oldValue = oldValue;
            }

        }

        [Test]
        public void OnChange_WithSync() {
            MockApplication app = Setup<TemplateBindingTest_OnChange_WithSync>();
            TemplateBindingTest_OnChange_WithSync e = (TemplateBindingTest_OnChange_WithSync) app.RootElement;
            TemplateBindingTest_OnChange_Inner child = (TemplateBindingTest_OnChange_Inner) e[0];

            e.myValue = "baseVal";

            app.Update();

            Assert.AreEqual("baseVal__changed", e.myValue);
            Assert.AreEqual("baseVal__changed", child.value);
            Assert.AreEqual("baseVal__changed", e.newValue);
            Assert.AreEqual("baseVal", e.oldValue);
        }

        public static string GetText(UIElement element) {
            UITextElement textEl = element as UITextElement;
            return textEl.text.Trim();
        }

    }

}