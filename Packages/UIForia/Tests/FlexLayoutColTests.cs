using NUnit.Framework;
using UIForia.Rendering;
using UIForia;
using UIForia.Layout;
using Tests.Mocks;
using UnityEngine;

[TestFixture]
public class FlexLayoutColTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style path='LayoutTestThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex'>
              
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

            [ExportStyle("w90h90m10")]
            public static UIStyle W90H90M10() {
                return new UIStyle() {
                    MarginLeft = 5f,
                    MarginRight = 5f,
                    PreferredWidth = 90f,
                    PreferredHeight = 90f
                };
            }

            [ExportStyle("w90h90mc10")]
            public static UIStyle W90H90MC10() {
                return new UIStyle() {
                    MarginTop = 5f,
                    MarginBottom = 5f,
                    PreferredWidth = 90f,
                    PreferredHeight = 90f
                };
            }

        }

    }

    [Test]
    public void AppliesCrossAxisStart() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(000, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesCrossAxisEnd() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(000, 400, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 400, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesCrossAxisCenter() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 200, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesCrossAxisStretch() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 500), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 500), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 500), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisEnd() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.End, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(300, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisStart() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.Start, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisSpaceBetween() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.SpaceBetween, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void AppliesMainAxisSpaceAround() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='600f' style.preferredHeight='600f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.SpaceAround, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(50, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(250, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(450, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

//    [Test]
//    public void HandlesCustomOrdering() {
//        string template = @"
//        <UITemplate>
//            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f'>
//                <Group x-id='child0' style='w100h100'/>
//                <Group x-id='child1' style='w100h100' />
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
//        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
//        root.child1.style.SetFlexItemOrder(-1, StyleState.Normal);
//        app.Update();
//
//        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child1.layoutResult.ScreenRect);
//
//        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
//    }

    [Test]
    public void RespectsMainAxisMarginValues() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Column' 
                      style.preferredWidth='$content(100)'
                      style.preferredHeight='500'>
                <Group x-id='child0' style='w90h90m10'/>
                <Group x-id='child1' style='w90h90m10'/>
                <Group x-id='child2' style='w90h90m10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexColLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 300, 500), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 0, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(105, 0, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(205, 0, 90, 90), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsCrossAxisMarginValues() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Column' 
                      style.preferredWidth='$content(100)'
                      style.preferredHeight='500f'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexColLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 270, 500), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 5, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(90, 5, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(180, 5, 90, 90), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsCrossAxisMarginValues_Center() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Column' 
                      style.preferredWidth='$content(100)'
                      style.preferredHeight='600f'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexColLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 270f, 600f), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 255, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(90, 255, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(180, 255, 90, 90), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsCrossAxisMarginValues_End() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Column' 
                      style.preferredWidth='$content(100)'
                      style.preferredHeight='600f'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexColLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 270f, 600f), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 505, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(90, 505, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(180, 505, 90, 90), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsCrossAxisMarginValues_Stretch() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Column' 
                      style.preferredWidth='$content(100)'
                      style.preferredHeight='600f'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexColLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 270, 600), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 5, 90, 590), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(90, 5, 90, 590), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(180, 5, 90, 590), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void HandleCrossAxisOverride() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        root.child1.style.SetFlexItemSelfAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        app.Update();

        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void GrowsBasedOnGrowthFactor() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.flexItemGrow='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        app.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 300, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ShrinksBasedOnShrinkFactor() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='250f' style.preferredHeight='600f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.flexItemShrink='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        app.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 50, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(150, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

//    [Test]
//    public void WrapsOnExactSizeMatch() {
//        string template = @"
//        <UITemplate>
//            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='500f' style.preferredHeight='100f'/>
//                <Group x-id='child1' style='w100h100'/>
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
//        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        app.Update();
//        Assert.AreEqual(new Rect(0, 0, 500, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
//    }
//
//    [Test]
//    public void WrapsOnSizeOverflow() {
//        string template = @"
//        <UITemplate>
//            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='450f' style.preferredHeight='100f'/>
//                <Group x-id='child1' style='w100h100'/>
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
//        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        app.Update();
//        Assert.AreEqual(new Rect(0, 0, 450, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
//    }
//
//    [Test]
//    public void WrapsOnSizeExceeded() {
//        string template = @"
//        <UITemplate>
//            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='550f' style.preferredHeight='100f'/>
//                <Group x-id='child1' style='w100h100'/>
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
//        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        app.Update();
//        Assert.AreEqual(new Rect(0, 0, 550, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
//    }
//
//    [Test]
//    public void GrowWrappedTracks() {
//        string template = @"
//        <UITemplate>
//            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='500f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredHeight='100f' style.preferredWidth='450f' style.flexItemGrow='1'/>
//                <Group x-id='child1' style='w100h100' style.flexItemGrow='1'/>
//                <Group x-id='child2' style='w100h100' style.flexItemGrow='1'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
//        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        app.Update();
//        Assert.AreEqual(new Rect(0, 0, 500, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(0, 100, 250, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(250, 100, 250, 100), root.child2.layoutResult.ScreenRect);
//    }
//
//    [Test]
//    public void ShrinkWrappedTracks() {
//        string template = @"
//        <UITemplate>
//            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='400f' style.preferredHeight='600f'>
//                <Group x-id='child0' style.preferredHeight='100f' style.preferredWidth='450f' style.flexItemShrink='1'/>
//                <Group x-id='child1' style.preferredHeight='100f' style.preferredWidth='300f' style.flexItemShrink='1'/>
//                <Group x-id='child2' style.preferredHeight='100f' style.preferredWidth='300f' style.flexItemShrink='1'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
//        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        app.Update();
//        Assert.AreEqual(new Rect(0, 0, 400, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(0, 100, 200, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(200, 100, 200, 100), root.child2.layoutResult.ScreenRect);
//    }

    [Test]
    public void RespectsPaddingValues() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' 
                      style.flexLayoutDirection='LayoutDirection.Column'
                      style.preferredWidth='$content(100)' 
                      style.preferredHeight='500f' 
                      style.paddingLeft='5f'
                      style.paddingRight='5f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 310, 500), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(105, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(205, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void RespectsPaddingValuesOnChildren() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='$content(100)' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child1' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child2' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        app.Update();
        Assert.AreEqual(new Rect(0, 0, 300, 500), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void SetsActualWidthAndHeight() {
        string template = @"
        <UITemplate>
            <Style path='FlexLayoutColTests+FlexColLayoutThing+Style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Column' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child1' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child2' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication app = new MockApplication(typeof(FlexColLayoutThing), template);
        app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexColLayoutThing root = (FlexColLayoutThing) app.RootElement;
        app.Update();
        Assert.AreEqual(300, root.layoutResult.ActualWidth);
        Assert.AreEqual(100, root.layoutResult.ActualHeight);
    }

}