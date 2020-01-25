using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.UIInput;
using UnityEngine;
using static Tests.TestUtils;

namespace DragEventTests {

    public class DragEventTests {

        [SetUp]
        public void Setup() {
            MockApplication.s_GenerateCode = true;
            MockApplication.s_UsePreCompiledTemplates = true;
        }

        public class TestDragEvent : DragEvent {

            public string sourceName;

            public TestDragEvent(string sourceName) : base(null) {
                this.sourceName = sourceName;
            }

        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_method")]
        public class DragTestThing_CreateMethod : UIElement {

            public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
                return new TestDragEvent("child" + index);
            }

        }

        [Test]
        public void DragCreate_FromChildTemplate_Method() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateMethod>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("child0", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_lambda")]
        public class DragTestThing_CreateLambda : UIElement {

            public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
                return new TestDragEvent("child" + index);
            }

        }

        [Test]
        public void DragCreate_FromChildTemplate_Lambda() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateLambda>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("child1", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_lambda_arg")]
        public class DragTestThing_CreateLambdaArg : UIElement {

            public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
                return new TestDragEvent("child" + index);
            }

        }

        [Test]
        public void DragCreate_FromChildTemplate_LambdaArg() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateLambdaArg>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("child3", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_lambda_arg_invalid_retn")]
        public class DragTestThing_CreateLambdaArgInvalidRetn : UIElement {

            public void CreateDragFromChild(MouseInputEvent evt, int index) { }

        }

        [Test]
        public void CreateLambdaArgInvalidRetn() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateLambdaArgInvalidRetn>("DragTestThing_CreateLambdaArgInvalidRetn"); });
            Assert.IsTrue(exception.Message.Contains(@"drag:create=""(e) => CreateDragFromChild(e, 3)"""));
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_param")]
        public class DragTestThing_CreateAnnotationWithParameter : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag(MouseInputEvent evt) {
                return new TestDragEvent("from class: " + evt.MousePosition);
            }

        }

        [Test]
        public void DragCreate_CreateAnnotationWithParameter() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateAnnotationWithParameter>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("from class: " + new Vector2(25, 20), As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_param")]
        public class DragTestThing_CreateAnnotationNoParameter : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag() {
                return new TestDragEvent("from class");
            }

        }

        [Test]
        public void CreateAnnotationNoParameter() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateAnnotationNoParameter>();
            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
            Assert.AreEqual("from class", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_invalid_param")]
        public class DragTestThing_CreateAnnotationInvalidParameter : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag(int x) {
                return new TestDragEvent("from class");
            }

        }

        [Test]
        public void CreateAnnotationInvalidParameter() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateAnnotationInvalidParameter>("DragTestThing_CreateAnnotationInvalidParameter"); });
            Assert.AreEqual(CompileException.InvalidInputAnnotation("CreateDrag", typeof(DragTestThing_CreateAnnotationInvalidParameter), typeof(OnDragCreateAttribute), typeof(MouseInputEvent), typeof(int)).Message, exception.Message);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_invalid_param_count")]
        public class DragTestThing_CreateAnnotationInvalidParameterCount : UIElement {

            [OnDragCreate]
            public DragEvent CreateDrag(MouseInputEvent evt, int x) {
                return new TestDragEvent("from class");
            }

        }

        [Test]
        public void CreateAnnotationInvalidParameterCount() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateAnnotationInvalidParameterCount>("DragTestThing_CreateAnnotationInvalidParameterCount"); });
            Assert.AreEqual(CompileException.TooManyInputAnnotationArguments("CreateDrag", typeof(DragTestThing_CreateAnnotationInvalidParameterCount), typeof(OnDragCreateAttribute), typeof(MouseInputEvent), 2).Message, exception.Message);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_invalid_return")]
        public class DragTestThing_CreateAnnotationInvalidReturn : UIElement {

            [OnDragCreate]
            public void CreateDrag(MouseInputEvent evt) { }

        }

        [Test]
        public void CreateAnnotationInvalidReturn() {
            CompileException exception = Assert.Throws<CompileException>(() => { MockApplication.Setup<DragTestThing_CreateAnnotationInvalidReturn>("DragTestThing_CreateAnnotationInvalidReturn"); });
            Assert.AreEqual(CompileException.InvalidDragCreatorAnnotationReturnType("CreateDrag", typeof(DragTestThing_CreateAnnotationInvalidReturn), typeof(void)).Message, exception.Message);
        }

        [Template("Data/DragEvents/DragEventTest_Drag.xml#drag_create_annotation_null")]
        public class DragTestThing_CreateAnnotationNull : UIElement {

            public bool wasCalled;

            [OnDragCreate]
            public DragEvent CreateDrag(MouseInputEvent evt) {
                wasCalled = true;
                return null;
            }

        }

        [Test]
        public void CreateDragAnnotationNull() {
            MockApplication testView = MockApplication.Setup<DragTestThing_CreateAnnotationNull>();
            DragTestThing_CreateAnnotationNull root = testView.RootElement as DragTestThing_CreateAnnotationNull;

            testView.Update();
            testView.SetViewportRect(new Rect(0, 0, 1000, 1000));

            testView.InputSystem.MouseDown(new Vector2(20, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);

            testView.InputSystem.MouseDragMove(new Vector2(25, 20));
            testView.Update();

            Assert.IsNull(testView.InputSystem.CurrentDragEvent);
            Assert.IsTrue(root.wasCalled);
        }

    }

}