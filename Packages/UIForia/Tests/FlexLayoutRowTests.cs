using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

[TestFixture]
public class FlexLayoutRowTests {

    
    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents style.layoutType='LayoutType.Flex'/>
        </UITemplate>
    ")]
    public class FlexRowLayoutThing : UIElement {

        public UIGroupElement child0;
        public UIGroupElement child1;
        public UIGroupElement child2;

        public override void OnCreate() {
            child0 = FindById<UIGroupElement>("child0");
            child1 = FindById<UIGroupElement>("child1");
            child2 = FindById<UIGroupElement>("child2");
        }

        

    }
    
    [Test]
    public void AppliesCrossAxisStart() {
    
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        
    }
    
    [Test]
    public void AppliesCrossAxisEnd() {
    
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        
    }
    
    [Test]
    public void AppliesCrossAxisCenter() {
    
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        
    }
    
    [Test]
    public void AppliesCrossAxisStretch() {
    
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 500, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 500, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 500, 100), root.child2.layoutResult.ScreenRect);
        
    }

    [Test]
    public void AppliesMainAxisEnd() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.End, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 300, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void AppliesMainAxisStart() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.Start, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void AppliesMainAxisSpaceBetween() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.SpaceBetween, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void AppliesMainAxisSpaceAround() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='600f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutMainAxisAlignment(MainAxisAlignment.SpaceAround, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 50, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 250, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 450, 100, 100), root.child2.layoutResult.ScreenRect);
    }
//
//    [Test]
//    public void HandlesCustomOrdering() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f'>
//                <Group x-id='child0' style='w100h100'/>
//                <Group x-id='child1' style='w100h100' />
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.child1.style.SetFlexItemOrder(-1, StyleState.Normal);
//        mockView.Update();
//        
//        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child1.layoutResult.ScreenRect);
//        
//        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
//    }

    [Test]
    public void HandleCrossAxisOverride() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        root.child1.style.SetFlexItemSelfAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void GrowsBasedOnGrowthFactor() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.flexItemGrow='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 300), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ShrinksBasedOnShrinkFactor() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='250f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.flexItemShrink='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 50), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 150, 100, 100), root.child2.layoutResult.ScreenRect);
    }

//    [Test]
//    public void WrapsOnExactSizeMatch() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='500f'/>
//                <Group x-id='child1' style='w100h100'/>
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 500), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
//    }
//    
//    [Test]
//    public void WrapsOnSizeOverflow() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='450f'/>
//                <Group x-id='child1' style='w100h100'/>
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 450), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
//        
//    }
//    
//    [Test]
//    public void WrapsOnSizeExceeded() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='550f'/>
//                <Group x-id='child1' style='w100h100'/>
//                <Group x-id='child2' style='w100h100'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 550), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 100, 100, 100), root.child2.layoutResult.ScreenRect);
//    }
//
//    [Test]
//    public void GrowWrappedTracks() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='500f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='450f' style.flexItemGrow='1'/>
//                <Group x-id='child1' style='w100h100' style.flexItemGrow='1'/>
//                <Group x-id='child2' style='w100h100' style.flexItemGrow='1'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 500), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 100, 250), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 250, 100, 250), root.child2.layoutResult.ScreenRect);
//    }
//
//    [Test]
//    public void ShrinkWrappedTracks() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='400f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='450f' style.flexItemShrink='1'/>
//                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='300f' style.flexItemShrink='1'/>
//                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='300f' style.flexItemShrink='1'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 400), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 100, 200), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 200, 100, 200), root.child2.layoutResult.ScreenRect);
//    }

    [Test]
    public void RespectsPaddingValues() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Row' style.preferredWidth='500f' style.preferredHeight='$content(100)' style.paddingTop='5f' style.paddingBottom='5f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 500, 310), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 5, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 105, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 205, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void RespectsMainAxisMarginValues() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Row' 
                      style.preferredWidth='500f'
                      style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w90h90m10'/>
                <Group x-id='child1' style='w90h90m10'/>
                <Group x-id='child2' style='w90h90m10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 500, 300), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 5, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 105, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 205, 90, 90), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void RespectsCrossAxisMarginValues() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Row' 
                      style.preferredWidth='500f'
                      style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 500, 270), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 0, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 90, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 180, 90, 90), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void RespectsCrossAxisMarginValues_Center() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Row' 
                      style.preferredWidth='600f'
                      style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 600f, 270), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(255, 0, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(255, 90, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(255, 180, 90, 90), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void RespectsCrossAxisMarginValues_End() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Row' 
                      style.preferredWidth='600f'
                      style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 600f, 270), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(505, 0, 90, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(505, 90, 90, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(505, 180, 90, 90), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void RespectsCrossAxisMarginValues_Stretch() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex'
                      style.flexLayoutDirection='LayoutDirection.Row' 
                      style.preferredWidth='600f'
                      style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w90h90mc10'/>
                <Group x-id='child1' style='w90h90mc10'/>
                <Group x-id='child2' style='w90h90mc10'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        mockView.Update();
        Assert.AreEqual(new Rect(0, 0, 600f, 270), root.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 0, 590, 90), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 90, 590, 90), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(5, 180, 590, 90), root.child2.layoutResult.ScreenRect);
    }

//    [Test]
//    public void WrapWithMultipleColumnSizes() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='600f' style.preferredHeight='400f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='450f'/>
//                <Group x-id='child1' style.preferredWidth='200f' style.preferredHeight='200f'/>
//                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='200f'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 450), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 200, 200), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 200, 100, 200), root.child2.layoutResult.ScreenRect);
//    }
//    
//    [Test]
//    public void Wrap_AlignTracks() {
//        string template = @"
//        <//                        <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
//            <Contents style.layoutType='LayoutType.Flex' style.preferredWidth='400f' style.preferredHeight='400f'>
//                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='450f'/>
//                <Group x-id='child1' style.preferredWidth='200f' style.preferredHeight='200f'/>
//                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='200f'/>
//            </Contents>
//        </UITemplate>
//        ";
//        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
//        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
//        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
//        root.style.SetFlexLayoutWrap(LayoutWrap.Wrap, StyleState.Normal);
//        root.style.SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
//        mockView.Update();
//        Assert.AreEqual(new Rect(0, 0, 100, 450), root.child0.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(100, 0, 200, 200), root.child1.layoutResult.ScreenRect);
//        Assert.AreEqual(new Rect(150, 200, 100, 200), root.child2.layoutResult.ScreenRect);
//    }

    [Test]
    public void SetsActualWidthAndHeight() {
        string template = @"
        <UITemplate>
            <Style path='Tests/Styles/FlexLayoutRowTests.style'/>
            <Contents style.layoutType='LayoutType.Flex' style.flexLayoutDirection='LayoutDirection.Row' style.preferredWidth='$content(100)' style.preferredHeight='$content(100)'>
                <Group x-id='child0' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child1' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
                <Group x-id='child2' style='w100h100' style.paddingLeft='5f' style.paddingRight='5f'/>
            </Contents>
        </UITemplate>
        ";
        MockApplication mockView = new MockApplication(typeof(FlexRowLayoutThing), template);
        mockView.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        Assert.AreEqual(100, root.layoutResult.ActualWidth);
        Assert.AreEqual(300, root.layoutResult.ActualHeight);
    }
}