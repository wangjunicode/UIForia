using System.Collections.Generic;
using NUnit.Framework;
using Rendering;
using Src;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Systems;
using Tests.Mocks;
using UnityEngine;

[TestFixture]
public class LayoutSystemTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
    ")]
    public class LayoutTestThing : UIElement {

        public UIGroupElement child0;
        public UIGroupElement child1;
        public UIGroupElement child2;
        public List<int> list;

        public override void OnCreate() {
            child0 = FindById<UIGroupElement>("child0");
            child1 = FindById<UIGroupElement>("child1");
            child2 = FindById<UIGroupElement>("child2");
        }

        public class Style {

            [ExportStyle("child1")]
            public static UIStyle Style1() {
                return new UIStyle() {
                    MinWidth = 300f
                };
            }

        }

    }

    [Test]
    public void Works() {
        MockView mockView = new MockView(typeof(LayoutTestThing));
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(100, root.child1.layoutResult.AllocatedWidth);
        Assert.AreEqual(100, root.child1.layoutResult.AllocatedHeight);

        Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
        Assert.AreEqual(100, root.child2.layoutResult.AllocatedWidth);
        Assert.AreEqual(100, root.child2.layoutResult.AllocatedHeight);
    }

    [Test]
    public void Updates() {
        MockView mockView = new MockView(typeof(LayoutTestThing));
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(100, root.child1.layoutResult.AllocatedWidth);
        Assert.AreEqual(100, root.child1.layoutResult.AllocatedHeight);

        Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
        Assert.AreEqual(100, root.child2.layoutResult.AllocatedWidth);
        Assert.AreEqual(100, root.child2.layoutResult.AllocatedHeight);

        root.child2.style.SetPreferredWidth(200, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(100, root.child1.layoutResult.AllocatedWidth);
        Assert.AreEqual(100, root.child1.layoutResult.AllocatedHeight);

        Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
        Assert.AreEqual(200, root.child2.layoutResult.AllocatedWidth);
        Assert.AreEqual(100, root.child2.layoutResult.AllocatedHeight);
    }

    [Test]
    public void ContentSized() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='content(100)' style.height='content(100)'>
                    <Group x-id='nested-child' style.width='300f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 50), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void MaxSizeChanges() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='content(100)' style.height='content(100)'>
                    <Group x-id='nested-child' style.width='300f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 50), root.child1.layoutResult.ScreenRect);
        root.child1.style.SetMaxWidth(150f, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 150, 50), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void WidthSizeConstraintChangesToContent() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='400f' style.height='content(100)'>
                    <Group x-id='nested-child' style.width='300f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 400, 50), root.child1.layoutResult.ScreenRect);
        root.child1.style.SetMaxWidth(UIMeasurement.Content100, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 50), root.child1.layoutResult.ScreenRect);
        root.child1.FindById("nested-child").style.SetPreferredWidth(150f, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 150, 50), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void HeightSizeConstraintChangesToMaxContent() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='400f' style.height='300f'>
                    <Group x-id='nested-child' style.width='300f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;

        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 400, 300), root.child1.layoutResult.ScreenRect);

        root.child1.style.SetMaxHeight(UIMeasurement.Content100, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 100, 400, 50), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void HeightSizeConstraintChangesToMinContent() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='400f' style.height='300f'>
                    <Group x-id='nested-child' style.width='300f' style.height='500f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;

        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 400, 300), root.child1.layoutResult.ScreenRect);

        root.child1.style.SetMinHeight(UIMeasurement.Content100, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 100, 400, 500), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void ChildEnabled() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='content(100)' style.height='content(100)'>
                    <Group x-if='false' x-id='nested-child-1' style.width='300f' style.height='50f'/>
                    <Group x-id='nested-child-2' style.width='200f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        mockView.Update();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        Assert.IsFalse(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 200, 50), root.child1.layoutResult.ScreenRect);
        root.child1.FindById("nested-child-1").SetEnabled(true);
        Assert.IsTrue(root.child1.FindById("nested-child-1").isEnabled);
        mockView.Update();
        Assert.IsTrue(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 300, 100), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void ChildDisabled() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='content(100)' style.height='content(100)'>
                    <Group x-id='nested-child-1' style.width='300f' style.height='50f'/>
                    <Group x-id='nested-child-2' style.width='200f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        mockView.Update();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;

        Assert.IsTrue(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 300, 100), root.child1.layoutResult.ScreenRect);

        root.child1.FindById("nested-child-1").SetEnabled(false);

        mockView.Update();
        Assert.IsFalse(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 200, 50), root.child1.layoutResult.ScreenRect);
    }

    [Test]
    public void ScreenPositionsGetUpdated() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style='marker' style.width='content(100)' style.height='content(100)'>
                    <Group x-id='nested-child-1' style.width='300f' style.height='50f'/>
                    <Group x-id='nested-child-2' style.width='200f' style.height='50f'/>
                </Group>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();

        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        UIElement nestedChild1 = root.child1.FindById("nested-child-1");
        UIElement nestedChild2 = root.child1.FindById("nested-child-2");
        mockView.Update();

        Assert.AreEqual(new Rect(0, 100, 300, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 300, 50), nestedChild1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 150, 200, 50), nestedChild2.layoutResult.ScreenRect);

        nestedChild1.style.SetPreferredHeight(500f, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 550), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 650, 100, 100), root.child2.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 300, 500), nestedChild1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 600, 200, 50), nestedChild2.layoutResult.ScreenRect);
    }

    [Test]
    public void DoesNotAddLayoutBoxForNonLaidOutElements() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Repeat x-id='repeat' list='{list}'>
                    <Group x-id='repeat-child-1' style.width='300f' style.height='50f'/>
                </Repeat>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        LayoutBox box = layoutSystem.GetBoxForElement(root.FindById("repeat"));
        Assert.IsNull(box);
    }

    [Test]
    public void AssignsProperClipRects_WithoutLayers() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.width='100f' style.height='200f'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        root.style.SetOverflowX(Overflow.Truncate, StyleState.Normal);
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        layoutSystem.SetViewportRect(new Rect(0, 0, 400, 400));
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 400, 400), root.layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 400), root.FindById("child0").layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 400), root.FindById("child1").layoutResult.clipRect);
        root.style.SetOverflowY(Overflow.Truncate, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 400, 400), root.layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("child0").layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("child1").layoutResult.clipRect);
    }

    [Test]
    public void AssignsProperClipRects_NestedWithoutLayers() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.width='100f' style.height='200f'>
                <Group x-id='child0' style.width='100f' style.height='100f'>
                    <Group x-id='nested-child0' style.width='100f' style.height='100f'/>
                </Group>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        root.style.SetOverflowX(Overflow.Truncate, StyleState.Normal);
        root.style.SetOverflowY(Overflow.Truncate, StyleState.Normal);
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        layoutSystem.SetViewportRect(new Rect(0, 0, 400, 400));
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("nested-child0").layoutResult.clipRect);
    }

    [Test]
    public void AssignsProperClipRects_NestedOverflowWithoutLayers() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.width='100f' style.height='200f'>
                <Group x-id='child0' style.width='50f' style.height='100f'>
                    <Group x-id='nested-child0' style.width='100f' style.height='100f'/>
                </Group>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        root.style.SetOverflowX(Overflow.Truncate, StyleState.Normal);
        root.style.SetOverflowY(Overflow.Truncate, StyleState.Normal);
        root.FindById("child0").style.SetOverflowX(Overflow.Truncate, StyleState.Normal);
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        layoutSystem.SetViewportRect(new Rect(0, 0, 400, 400));
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 50, 200), root.FindById("nested-child0").layoutResult.clipRect);
        root.FindById("child0").style.SetOverflowX(Overflow.None, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("nested-child0").layoutResult.clipRect);
    }

    [Test]
    public void AssignsProperLayer() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.width='100f' style.height='200f'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        layoutSystem.SetViewportRect(new Rect(0, 0, 400, 400));
        mockView.Update();
        Assert.AreEqual(1, root.FindById("child0").layoutResult.layer);
        root.FindById("child0").style.SetRenderLayer(RenderLayer.Parent, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(0, root.FindById("child0").layoutResult.layer);
    }
    
    [Test]
    public void AssignsProperLayer_WithOffset() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.width='100f' style.height='200f'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        layoutSystem.SetViewportRect(new Rect(0, 0, 400, 400));
        mockView.Update();
        Assert.AreEqual(1, root.FindById("child0").layoutResult.layer);
        root.FindById("child0").style.SetRenderLayerOffset(5, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(-4, root.FindById("child0").layoutResult.layer);
        root.FindById("child0").style.SetRenderLayer(RenderLayer.Parent, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(-5, root.FindById("child0").layoutResult.layer);
    }
    
    [Test]
    public void AssignsProperZIndex() {
        string template = @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.width='100f' style.height='200f'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
                <Group x-id='child3' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(LayoutTestThing), template);
        mockView.Initialize();
        LayoutTestThing root = (LayoutTestThing) mockView.RootElement;
        MockLayoutSystem layoutSystem = (MockLayoutSystem) mockView.LayoutSystem;
        layoutSystem.SetViewportRect(new Rect(0, 0, 400, 400));
        
        mockView.Update();
        
        UIElement child0 = root.FindById("child0");
        UIElement child1 = root.FindById("child1");
        UIElement child2 = root.FindById("child2");
        UIElement child3 = root.FindById("child3");
        
        Assert.AreEqual(0, child0.layoutResult.zIndex);
        Assert.AreEqual(0, child1.layoutResult.zIndex);
        Assert.AreEqual(0, child2.layoutResult.zIndex);
        Assert.AreEqual(0, child3.layoutResult.zIndex);
        
        child0.style.SetZIndex(1, StyleState.Normal);
        child1.style.SetZIndex(2, StyleState.Normal);
        child2.style.SetZIndex(3, StyleState.Normal);
        child3.style.SetZIndex(4, StyleState.Normal);
        
        mockView.Update();
        
        Assert.AreEqual(1, child0.layoutResult.zIndex);
        Assert.AreEqual(2, child1.layoutResult.zIndex);
        Assert.AreEqual(3, child2.layoutResult.zIndex);
        Assert.AreEqual(4, child3.layoutResult.zIndex);
    }

}