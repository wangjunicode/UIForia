using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class LayoutSystemTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Vertical'>
                <Group x-id='child0' style='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
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

    }

    [Test]
    public void Works() {
        MockApplication app = new MockApplication(typeof(LayoutTestThing));
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        app.Update();
        Assert.AreEqual(300, root.child0.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child0.layoutResult.actualSize.height);
        Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(100, root.child1.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child1.layoutResult.actualSize.height);

        Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
        Assert.AreEqual(100, root.child2.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child2.layoutResult.actualSize.height);
    }

    [Test]
    public void Updates() {
        MockApplication app = new MockApplication(typeof(LayoutTestThing));
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        app.Update();
        Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(100, root.child1.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child1.layoutResult.actualSize.height);

        Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
        Assert.AreEqual(100, root.child2.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child2.layoutResult.actualSize.height);

        root.child2.style.SetPreferredWidth(200, StyleState.Normal);
        app.Update();
        Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(100, root.child1.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child1.layoutResult.actualSize.height);

        Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
        Assert.AreEqual(200, root.child2.layoutResult.actualSize.width);
        Assert.AreEqual(100, root.child2.layoutResult.actualSize.height);
    }

    [Test]
    public void ContentSized() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                    <Group x-id='nested-child' style.preferredWidth='300f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 50), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void MaxSizeChanges() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                    <Group x-id='nested-child' style.preferredWidth='300f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 50), root.child1.layoutResult.AllocatedRect);
        root.child1.style.SetMaxWidth(150f, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 150, 50), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void WidthSizeConstraintChangesToContent() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='400f' style.preferredHeight='$content(100)'>
                    <Group x-id='nested-child' style.preferredWidth='300f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 400, 50), root.child1.layoutResult.AllocatedRect);
        root.child1.style.SetMaxWidth(UIMeasurement.Content100, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 50), root.child1.layoutResult.AllocatedRect);
        root.child1.FindById("nested-child").style.SetPreferredWidth(150f, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 150, 50), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void HeightSizeConstraintChangesToMaxContent() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='400f' style.preferredHeight='300f'>
                    <Group x-id='nested-child' style.preferredWidth='300f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);

        app.Update();
        Assert.AreEqual(new Rect(0, 100, 400, 300), root.child1.layoutResult.AllocatedRect);

        root.child1.style.SetMaxHeight(UIMeasurement.Content100, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 100, 400, 50), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void HeightSizeConstraintChangesToMinContent() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='400f' style.preferredHeight='300f'>
                    <Group x-id='nested-child' style.preferredWidth='300f' style.preferredHeight='500f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);

        app.Update();
        Assert.AreEqual(new Rect(0, 100, 400, 300), root.child1.layoutResult.AllocatedRect);

        root.child1.style.SetMinHeight(UIMeasurement.Content100, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 100, 400, 500), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void ChildEnabled() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Vertical'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                    <Group if='false' x-id='nested-child-1' style.preferredWidth='300f' style.preferredHeight='50f'/>
                    <Group x-id='nested-child-2' style.preferredWidth='200f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        app.Update();
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        Assert.IsFalse(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 200, 50), root.child1.layoutResult.AllocatedRect);
        root.child1.FindById("nested-child-1").SetEnabled(true);
        Assert.IsTrue(root.child1.FindById("nested-child-1").isEnabled);
        app.Update();
        Assert.IsTrue(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 300, 100), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void ChildDisabled() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                    <Group x-id='nested-child-1' style.preferredWidth='300f' style.preferredHeight='50f'/>
                    <Group x-id='nested-child-2' style.preferredWidth='200f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        app.Update();
        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);

        Assert.IsTrue(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 300, 100), root.child1.layoutResult.AllocatedRect);

        root.child1.FindById("nested-child-1").SetEnabled(false);

        app.Update();
        Assert.IsFalse(root.child1.FindById("nested-child-1").isEnabled);
        Assert.AreEqual(new Rect(0, 100, 200, 50), root.child1.layoutResult.AllocatedRect);
    }

    [Test]
    public void ScreenPositionsGetUpdated() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.layoutType='LayoutType.Flex'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style='marker' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                    <Group x-id='nested-child-1' style.preferredWidth='300f' style.preferredHeight='50f'/>
                    <Group x-id='nested-child-2' style.preferredWidth='200f' style.preferredHeight='50f'/>
                </Group>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);

        LayoutTestThing root = (LayoutTestThing) app.RootElement.GetChild(0);
        UIElement nestedChild1 = root.child1.FindById("nested-child-1");
        UIElement nestedChild2 = root.child1.FindById("nested-child-2");
        app.Update();

        Assert.AreEqual(new Rect(0, 100, 300, 100), root.child1.layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 100, 300, 50), nestedChild1.layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 150, 200, 50), nestedChild2.layoutResult.AllocatedRect);

        nestedChild1.style.SetPreferredHeight(500f, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new Rect(0, 100, 300, 550), root.child1.layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 650, 100, 100), root.child2.layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 100, 300, 500), nestedChild1.layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 600, 200, 50), nestedChild2.layoutResult.AllocatedRect);
    }

    [Test]
    public void AssignsProperClipRects_WithoutLayers() {
        string template = @"
        <UITemplate>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        root.style.SetOverflowX(Overflow.Hidden, StyleState.Normal);
        app.SetViewportRect(new Rect(0, 0, 400, 400));
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 400, 400), root.layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 400), root.FindById("child0").layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 400), root.FindById("child1").layoutResult.clipRect);
        root.style.SetOverflowY(Overflow.Hidden, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 400, 400), root.layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("child0").layoutResult.clipRect);
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("child1").layoutResult.clipRect);
    }

    [Test]
    public void AssignsProperClipRects_NestedWithoutLayers() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'>
                    <Group x-id='nested-child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                </Group>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        root.style.SetOverflowX(Overflow.Hidden, StyleState.Normal);
        root.style.SetOverflowY(Overflow.Hidden, StyleState.Normal);
        app.SetViewportRect(new Rect(0, 0, 400, 400));
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("nested-child0").layoutResult.clipRect);
    }

    [Test]
    public void AssignsProperClipRects_NestedOverflowWithoutLayers() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='child0' style.preferredWidth='50f' style.preferredHeight='100f'>
                    <Group x-id='nested-child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                </Group>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        root.style.SetOverflowX(Overflow.Hidden, StyleState.Normal);
        root.style.SetOverflowY(Overflow.Hidden, StyleState.Normal);
        root.FindById("child0").style.SetOverflowX(Overflow.Hidden, StyleState.Normal);
        app.SetViewportRect(new Rect(0, 0, 400, 400));
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 50, 200), root.FindById("nested-child0").layoutResult.clipRect);
        root.FindById("child0").style.SetOverflowX(Overflow.Visible, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 100, 200), root.FindById("nested-child0").layoutResult.clipRect);
    }


    [Test]
    public void AssignsProperZIndex() {
        string template = @"
        <UITemplate>
            <Style>
                style child1 {
                    MinWidth = 300px;
                }
            </Style>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child3' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        app.SetViewportRect(new Rect(0, 0, 400, 400));

        app.Update();

        UIElement child0 = root.FindById("child0");
        UIElement child1 = root.FindById("child1");
        UIElement child2 = root.FindById("child2");
        UIElement child3 = root.FindById("child3");

        Assert.AreEqual(4, child0.layoutResult.zIndex);
        Assert.AreEqual(3, child1.layoutResult.zIndex);
        Assert.AreEqual(2, child2.layoutResult.zIndex);
        Assert.AreEqual(1, child3.layoutResult.zIndex);

        child0.style.SetZIndex(1, StyleState.Normal);
        child2.style.SetZIndex(2, StyleState.Normal);
        child1.style.SetZIndex(3, StyleState.Normal);
        child3.style.SetZIndex(4, StyleState.Normal);

        app.Update();

        Assert.AreEqual(4, child0.layoutResult.zIndex);
        Assert.AreEqual(3, child2.layoutResult.zIndex);
        Assert.AreEqual(2, child1.layoutResult.zIndex);
        Assert.AreEqual(1, child3.layoutResult.zIndex);
    }

    [Test]
    public void SortByLayersInAscendingOrder() {
        string template = @"
        <UITemplate>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child3' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        app.SetViewportRect(new Rect(0, 0, 400, 400));

        app.Update();

        LightList<UIElement> elements = app.LayoutSystem.GetVisibleElements();
        Assert.AreEqual(root.FindById("child1"), elements[0]);
        Assert.AreEqual(root.FindById("child3"), elements[1]);
        Assert.AreEqual(root.FindById("child2"), elements[2]);
        Assert.AreEqual(root.FindById("child0"), elements[3]);
        Assert.AreEqual(root, elements[4]);
    }

    [Test]
    public void SortByZIndexInAscendingOrder() {
        string template = @"
        <UITemplate>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                <Group x-id='child2' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                <Group x-id='child3' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        app.SetViewportRect(new Rect(0, 0, 400, 400));

        app.Update();

        LightList<UIElement> elements = app.LayoutSystem.GetVisibleElements();
        Assert.AreEqual(root.FindById("child1"), elements[0]);
        Assert.AreEqual(root.FindById("child2"), elements[1]);
        Assert.AreEqual(root.FindById("child3"), elements[2]);
        Assert.AreEqual(root.FindById("child0"), elements[3]);
        Assert.AreEqual(root, elements[4]);
    }

    [Test]
    public void SortByLayerAndZIndexInAscendingOrder() {
        string template = @"
        <UITemplate>
            <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
                <Group x-id='e0' style.preferredWidth='100f' style.preferredHeight='100f'>
                    <Group x-id='e1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                    <Group x-id='e2' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                    <Group x-id='e3' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    <Group x-id='e4'>
                        <Group x-id='e5' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                        <Group x-id='e6' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                        <Group x-id='e7' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    </Group>
                </Group>
                <Group x-id='e8' style.preferredWidth='100f' style.preferredHeight='100f'>
                    <Group x-id='e9' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    <Group x-id='e10' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                    <Group x-id='e11' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    <Group x-id='e12'>
                        <Group x-id='e13' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                        <Group x-id='e14' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                        <Group x-id='e15' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    </Group>
                </Group>
                <Group x-id='e16' style.preferredWidth='100f' style.preferredHeight='100f'>
                    <Group x-id='e17' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                    <Group x-id='e18' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                    <Group x-id='e19' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    <Group x-id='e20'>
                        <Group x-id='e21' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
                        <Group x-id='e22' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
                        <Group x-id='e23' style.preferredWidth='100f' style.preferredHeight='100f'/>
                    </Group>
                </Group>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(LayoutTestThing), template);
        LayoutTestThing root = (LayoutTestThing) app.RootElement;
        app.SetViewportRect(new Rect(0, 0, 400, 400));

        app.Update();

        LightList<UIElement> elements = app.LayoutSystem.GetVisibleElements();

        Assert.AreEqual(root.FindById("e21"), elements[0]);
        Assert.AreEqual(root.FindById("e17"), elements[1]);
        Assert.AreEqual(root.FindById("e13"), elements[2]);
        Assert.AreEqual(root.FindById("e5"), elements[3]);
        Assert.AreEqual(root.FindById("e1"), elements[4]);
        Assert.AreEqual(root.FindById("e22"), elements[5]);
        Assert.AreEqual(root.FindById("e18"), elements[6]);
        Assert.AreEqual(root.FindById("e14"), elements[7]);
        Assert.AreEqual(root.FindById("e10"), elements[8]);
        Assert.AreEqual(root.FindById("e6"), elements[9]);
        Assert.AreEqual(root.FindById("e2"), elements[10]);
        Assert.AreEqual(root.FindById("e23"), elements[11]);
        Assert.AreEqual(root.FindById("e20"), elements[12]);
        Assert.AreEqual(root.FindById("e19"), elements[13]);
        Assert.AreEqual(root.FindById("e16"), elements[14]);
        Assert.AreEqual(root.FindById("e15"), elements[15]);
        Assert.AreEqual(root.FindById("e12"), elements[16]);
        Assert.AreEqual(root.FindById("e11"), elements[17]);
        Assert.AreEqual(root.FindById("e9"), elements[18]);
        Assert.AreEqual(root.FindById("e8"), elements[19]);
        Assert.AreEqual(root.FindById("e7"), elements[20]);
        Assert.AreEqual(root.FindById("e4"), elements[21]);
        Assert.AreEqual(root.FindById("e3"), elements[22]);
        Assert.AreEqual(root.FindById("e0"), elements[23]);
        Assert.AreEqual(root, elements[24]);
    }

}