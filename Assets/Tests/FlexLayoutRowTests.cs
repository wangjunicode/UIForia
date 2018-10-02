using NUnit.Framework;
using Rendering;
using Src;
using Src.Layout;
using Tests.Mocks;
using UnityEngine;

[TestFixture]
public class FlexLayoutRowTests {

    
    [Template(TemplateType.String, @"
        <UITemplate>
            <Style classPath='LayoutSystemTests+LayoutTestThing+Style'/>
            <Contents style.layoutType='Flex'>
              
            </Contents>
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
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Start, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        
    }
    
    [Test]
    public void AppliesCrossAxisEnd() {
    
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.End, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(400, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        
    }
    
    [Test]
    public void AppliesCrossAxisCenter() {
    
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 200, 100, 100), root.child2.layoutResult.ScreenRect);
        
    }
    
    [Test]
    public void AppliesCrossAxisStretch() {
    
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 500, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 500, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 500, 100), root.child2.layoutResult.ScreenRect);
        
    }

    [Test]
    public void AppliesMainAxisEnd() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.End, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 300, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void AppliesMainAxisStart() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.Start, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void AppliesMainAxisSpaceBetween() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.SpaceBetween, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    [Test]
    public void AppliesMainAxisSpaceAround() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='600f' style.height='600f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100'/>
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexMainAxisAlignment(MainAxisAlignment.SpaceAround, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 50, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 250, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 450, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    public void IgnoresOutOfFlow() { }

    [Test]
    public void HandlesCustomOrdering() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.child1.style.SetFlexItemOrderOverride(-1, StyleState.Normal);
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void HandleCrossAxisOverride() {
        string template = @"
        <UITemplate>
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        root.style.SetFlexCrossAxisAlignment(CrossAxisAlignment.Center, StyleState.Normal);
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
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='600f' style.height='500f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.growthFactor='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
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
            <Style classPath='FlexLayoutRowTests+FlexRowLayoutThing+Style'/>
            <Contents style.layoutType='Flex' style.width='600f' style.height='250f'>
                <Group x-id='child0' style='w100h100'/>
                <Group x-id='child1' style='w100h100' style.shrinkFactor='1' />
                <Group x-id='child2' style='w100h100'/>
            </Contents>
        </UITemplate>
        ";
        MockView mockView = new MockView(typeof(FlexRowLayoutThing), template);
        mockView.Initialize();
        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
        FlexRowLayoutThing root = (FlexRowLayoutThing) mockView.RootElement;
        mockView.Update();
        
        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 50), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 150, 100, 100), root.child2.layoutResult.ScreenRect);
    }
    
    public void LimitsGrowthToMaxWidth() { }

//    [Test]
//    public void DefaultsToRowLayout() { }
    //  [Test]
    // public void DefaultsToCrossAxisStart() { }

    // [Test]
    //public void DefaultsToMainAxisStart() { }

}