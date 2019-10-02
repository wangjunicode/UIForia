using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;
using static Tests.TestUtils;

[TestFixture]
public class InputSystem_DragTests {

    public class TestDragEvent : DragEvent {

        public string sourceName;

        public TestDragEvent(string sourceName) : base(null) {
            this.sourceName = sourceName;
        }

    }

    [Template(TemplateType.String, @"
    
        <UITemplate>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group onDragCreate='CreateDragFromChild($event, 0)' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>

    ")]
    public class DragTestThing : UIElement {

        public bool ignoreChildDrag;

        [OnDragCreate]
        public DragEvent CreateDragEvent() {
            return new TestDragEvent("root");
        }

        public DragEvent CreateDragFromChild(MouseInputEvent evt, int index) {
            if (ignoreChildDrag) return null;
            return new TestDragEvent("child" + index);
        }

    }

    [Template(TemplateType.String, @"
    
        <UITemplate>
            <Contents>
            </Contents>
        </UITemplate>

    ")]
    public class FailDragTestThing : UIElement {

        [OnDragCreate]
        public void CreateDragEvent() {
        }

    }

    [Test]
    public void DragCreate_CreateFromAnnotation() {
        MockApplication testView = new MockApplication(typeof(DragTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragTestThing root = (DragTestThing) testView.RootElement.GetChild(0);
        root.ignoreChildDrag = true;
        testView.InputSystem.MouseDown(new Vector2(20, 20));
        testView.Update();

        Assert.IsNull(testView.InputSystem.CurrentDragEvent);

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.IsInstanceOf<TestDragEvent>(testView.InputSystem.CurrentDragEvent);
        Assert.AreEqual("root", As<TestDragEvent>(testView.InputSystem.CurrentDragEvent).sourceName);
    }

    [Test]
    public void DragCreate_CreateFromChildTemplate() {
        MockApplication testView = new MockApplication(typeof(DragTestThing));
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

    [Test]
    public void DragCreate_MustReturnDragEvent() {
        var exception = Assert.Throws<Exception>(() => { new MockApplication(typeof(FailDragTestThing)); });
        /*
         * Unfortunately some broken state is left if the application crashes because of an exception.
         * The next thing that renders will be rendered incorrectly but clears some of that mysterious broken state
         * so that the next rendering will be correct again... So until we find out how and what to reset after this
         * test we just start another non-broken TestThing so the next test isn't affected.
         */
        new MockApplication(typeof(DragTestThing));
        Assert.AreEqual($"Methods annotated with {nameof(OnDragCreateAttribute)} must return an instance of {nameof(DragEvent)}", exception.Message);
    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style>
                style grid-container {
                    LayoutType = Flex;
                    FlexLayoutDirection = Row;
                    PreferredSize = 300px, 100px;
                }
    
                style grid-cell {
                    PreferredSize = 100px;
                    TransformPositionY = 0px;
                    AnchorTop = 0px;
                }
    
                style c1 {
                    TransformPositionX = 0px;
                    AnchorLeft = 0px;
                }
                style c2 {
                    TransformPositionX = 0;
                    AnchorLeft = 100px;
                }
                style c3 {
                    TransformPositionX = 0;
                    AnchorLeft = 200px;
                }
            </Style>
            <Contents style='grid-container'>
                <Group onDragEnter='{HandleDragEnterChild($element, 0)}' onDragExit='{HandleDragExitChild($element, 0)}' style='grid-cell c1'/>
                <Group onDragEnter='{HandleDragEnterChild($element, 1)}' onDragExit='{HandleDragExitChild($element, 1)}' style='grid-cell c2'/>
                <Group onDragEnter='{HandleDragEnterChild($element, 2)}' onDragExit='{HandleDragExitChild($element, 2)}' style='grid-cell c3'/>
            </Contents>
        </UITemplate>
    ")]
    public class DragHandlerTestThing : UIElement {

        public List<string> dragList = new List<string>();
        public bool ignoreEnter;
        public bool ignoreExit;

        public void HandleDragEnterChild(UIElement el, int index) {
            if (ignoreEnter) return;
            dragList.Add("enter:child" + index);
        }

        public void HandleDragExitChild(UIElement el, int index) {
            if (ignoreExit) return;
            dragList.Add("exit:child" + index);
        }

        [OnDragCreate]
        public TestDragEvent OnDragCreate() {
            return new TestDragEvent("root");
        }

    }

    [Test]
    public void DragEnter_Fires() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);
        testView.Update();

        Assert.AreEqual(0, root.dragList.Count);
        root.ignoreExit = true;
        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());

        testView.InputSystem.MouseDragMove(new Vector2(130, 30));
        testView.Update();

        Assert.AreEqual(2, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0", "enter:child1"}, root.dragList.ToArray());
    }

    [Test]
    public void DragEnter_DoesNotFireAgainForSamePosition() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);
        testView.Update();

        Assert.AreEqual(0, root.dragList.Count);
        root.ignoreExit = true;
        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(1, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());
    }

    [Test]
    public void DragEnter_DoesNotFireAgainForPositionSameElement() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);
        testView.Update();

        Assert.AreEqual(0, root.dragList.Count);
        root.ignoreExit = true;
        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(1, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());
    }

    [Test]
    public void DragEnter_FiresForNewElement() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);
        testView.Update();

        Assert.AreEqual(0, root.dragList.Count);
        root.ignoreExit = true;

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(130, 30));
        testView.Update();

        Assert.AreEqual(2, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0", "enter:child1"}, root.dragList.ToArray());
    }

    [Test]
    public void DragEnter_FiresForReEnteringElement() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);

        Assert.AreEqual(0, root.dragList.Count);
        root.ignoreExit = true;

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(130, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(3, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0", "enter:child1", "enter:child0"}, root.dragList.ToArray());
    }

    [Test]
    public void DragExit_FiresAndPropagates() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(1, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());

        testView.InputSystem.MouseDragMove(new Vector2(130, 30));
        testView.Update();

        Assert.AreEqual(3, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0", "exit:child0", "enter:child1"}, root.dragList.ToArray());
    }

    [Test]
    public void DragExit_FireOnlyForExitedElement() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(1, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());

        testView.InputSystem.MouseDragMove(new Vector2(40, 30));
        testView.Update();

        Assert.AreEqual(1, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0"}, root.dragList.ToArray());
    }

    [Test]
    public void DragExit_FireAgainWhenReenteredElement() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing root = (DragHandlerTestThing) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(130, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(5, root.dragList.Count);
        Assert.AreEqual(new[] {"enter:child0", "exit:child0", "enter:child1", "exit:child1", "enter:child0"}, root.dragList.ToArray());
    }

    [Template(TemplateType.String, @"
    
        <UITemplate>
            <Style>
                style container {
                    LayoutType = Flex;
                    FlexLayoutDirection = Row;
                    PreferredSize = 300px, 100px;
                }
        
                style cell {
                    PreferredSize = 100px;
                }
            </Style>
            <Contents style='container'>
                <Group onDragMove='{HandleDragMoveChild(0)}' onDragHover='{HandleDragHoverChild(0)}' style='cell c0'/>
                <Group onDragMove='{HandleDragMoveChild(1)}' onDragHover='{HandleDragHoverChild(1)}' style='cell c1'/>
                <Group onDragMove='{HandleDragMoveChild(2)}' onDragHover='{HandleDragHoverChild(2)}' style='cell c2'/>
            </Contents>
        </UITemplate>

    ")]
    public class DragHandlerTestThing_Move : UIElement {

        public List<string> dragList = new List<string>();

        public void HandleDragMoveChild(int index) {
            dragList.Add("move:child" + index);
        }

        public void HandleDragHoverChild(int index) {
            dragList.Add("hover:child" + index);
        }

        [OnDragCreate]
        public TestDragEvent OnDragCreate() {
            return new TestDragEvent("root");
        }

    }

    [Test]
    public void DragMove_FiresAndPropagates() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing_Move));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing_Move root = (DragHandlerTestThing_Move) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(new string[0], root.dragList.ToArray());

        testView.InputSystem.MouseDragMove(new Vector2(30, 20));
        testView.Update();

        Assert.AreEqual(new[] {"move:child0"}, root.dragList.ToArray());
    }

    [Test]
    public void DragMove_FiresAgainWhenMovedAndContains() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing_Move));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing_Move root = (DragHandlerTestThing_Move) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 20));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(31, 20));
        testView.Update();

        Assert.AreEqual(new[] {"move:child0", "move:child0"}, root.dragList.ToArray());
    }

    [Test]
    public void DragMove_DoesNotFireAgainWhenNotMovedAndContains() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing_Move));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing_Move root = (DragHandlerTestThing_Move) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(31, 20));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(31, 20));
        testView.Update();

        Assert.AreEqual(new[] {"move:child0", "hover:child0"}, root.dragList.ToArray());
    }

    [Template(TemplateType.String, @"
    
        <UITemplate>
            <Style>
                style container {
                    LayoutType = Flex;
                    FlexLayoutDirection = Row;
                    PreferredSize = 300px, 100px;
                }
        
                style cell {
                    PreferredSize = 100px;
                }
            </Style>
            <Contents style='container'>
                <Group onDragMove='{HandleDragMoveChild($event, 0)}' onDragHover='{HandleDragHoverChild($event, 0)}' style='cell c0'/>
                <Group onDragMove='{HandleDragMoveChild($event, 1)}' onDragHover='{HandleDragHoverChild($event, 1)}' style='cell c1'/>
                <Group onDragMove='{HandleDragMoveChild($event, 2)}' onDragHover='{HandleDragHoverChild($event, 2)}' style='cell c2'/>
            </Contents>
        </UITemplate>

    ")]
    public class DragHandlerTestThing_MoveWithDragEvent : UIElement {

        public List<string> dragList = new List<string>();

        public void HandleDragMoveChild(DragEvent evt, int index) {
            if (evt is TestDragEvent textEvt) dragList.Add($"move:child{index}:{textEvt.sourceName}");
        }

        public void HandleDragHoverChild(DragEvent evt, int index) {
            if (evt is TestDragEvent textEvt) dragList.Add($"hover:child{index}:{textEvt.sourceName}");
        }

        [OnDragCreate]
        public TestDragEvent OnDragCreate() {
            return new TestDragEvent("root");
        }

    }

    [Test]
    public void DragMove_FiresAndPropagatesWithDragEvent() {
        MockApplication testView = new MockApplication(typeof(DragHandlerTestThing_MoveWithDragEvent));
        testView.Update();
        testView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        DragHandlerTestThing_MoveWithDragEvent root = (DragHandlerTestThing_MoveWithDragEvent) testView.RootElement.GetChild(0);

        testView.InputSystem.MouseDown(new Vector2(10, 10));
        testView.Update();

        testView.InputSystem.MouseDragMove(new Vector2(30, 30));
        testView.Update();

        Assert.AreEqual(new string[0], root.dragList.ToArray());

        testView.InputSystem.MouseDragMove(new Vector2(30, 20));
        testView.Update();

        Assert.AreEqual(new[] {"move:child0:root"}, root.dragList.ToArray());
    }

}