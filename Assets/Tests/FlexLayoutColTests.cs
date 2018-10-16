using NUnit.Framework;
using Rendering;
using Src;
using Src.Layout;
using Tests.Mocks;
using UnityEditor.VersionControl;
using UnityEngine;

[TestFixture]
public class FlexLayoutColTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
              
            </Contents>
        </UITemplate>
    ")]
    public class FlexColLayoutThing : UIElement {

        public UIGroupElement child0;
        public UIGroupElement child1;
        public UIGroupElement child2;

        public override void OnCreate() {
            child0 = FindById<UIGroupElement>("child0");
            child1 = FindById<UIGroupElement>("child1");
            child2 = FindById<UIGroupElement>("child2");
        }

        public class Style {

            [ExportStyle("w100h100")]
            public static UIStyle W100H100() {
                return new UIStyle() {
                    PreferredWidth = 100f,
                    PreferredHeight = 100f
                };
            }

        }

    }

    [Test]
    public void AppliesCrossAxisStart() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(000, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesCrossAxisEnd() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(000, 400, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 400, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesCrossAxisCenter() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 200, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesCrossAxisStretch() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 500), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 500), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 500), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisEnd() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.End, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(300, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisStart() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.Start, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisSpaceBetween() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.SpaceBetween, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisSpaceAround() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='600f' style.height='600f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.SpaceAround, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(50, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(250, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(450, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

//    public void IgnoresOutOfFlow() { }

    [Test]
    public void HandlesCustomOrdering() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.child1.style.SetFlexItemOrderOverride(-1, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child1.layoutResult.ScreenRect);

        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void HandleCrossAxisOverride() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        root.child1.style.SetFlexItemSelfAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void GrowsBasedOnGrowthFactor() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.growthFactor='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 300, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ShrinksBasedOnShrinkFactor() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='250f' style.height='600f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.shrinkFactor='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 50, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(150, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void WrapsOnExactSizeMatch() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style.width='500f' style.height='100f'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexWrapMode(LayoutWrap.Wrap, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 500, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void WrapsOnSizeOverflow() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style.width='450f' style.height='100f'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexWrapMode(LayoutWrap.Wrap, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 450, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void WrapsOnSizeExceeded() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style.width='550f' style.height='100f'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexWrapMode(LayoutWrap.Wrap, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 550, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void GrowWrappedTracks() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='500f' style.height='500f'>
                <Group x-id='child0' style.height='100f' style.width='450f' style.growthFactor='1'/>
                <Group x-id='child1' style='w100h100' style.growthFactor='1'/>
                <Group x-id='child2' style='w100h100' style.growthFactor='1'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexWrapMode(LayoutWrap.Wrap, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 500, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 250, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(250, 100, 250, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ShrinkWrappedTracks() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='400f' style.height='600f'>
                <Group x-id='child0' style.height='100f' style.width='450f' style.shrinkFactor='1'/>
                <Group x-id='child1' style.height='100f' style.width='300f' style.shrinkFactor='1'/>
                <Group x-id='child2' style.height='100f' style.width='300f' style.shrinkFactor='1'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexWrapMode(LayoutWrap.Wrap, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 400, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 200, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 100, 200, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsPaddingValues() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='content(100)' style.height='500f' style.paddingLeft='5f' style.paddingRight='5f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 310, 500), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(105, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(205, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsPaddingValuesOnChildren() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.layoutDirection='Column' style.width='content(100)' style.height='500f'>
                <Group x-id='child0' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child1' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child2' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexColLayoutThing), template);
        mockView.Initialize();
        mockView.LayoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 300, 500), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

}