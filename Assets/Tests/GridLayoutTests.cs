using System.Collections.Generic;
using NUnit.Framework;
using Rendering;
using Src;
using Src.Layout;
using Tests.Mocks;
using UnityEngine;

[TestFixture]
public class GridLayoutTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents style.layoutType='Grid'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayoutThing3x1 : UIElement {

        public UIGroupElement child0;
        public UIGroupElement child1;
        public UIGroupElement child2;
        
        public override void OnCreate() {
            
            child0 = FindById<UIGroupElement>("child0");
            child1 = FindById<UIGroupElement>("child1");
            child2 = FindById<UIGroupElement>("child2");
            
        }

    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents style.layoutType='Grid'>
                <Group x-id='child0' style.width='100f' style.height='100f'/>
                <Group x-id='child1' style.width='100f' style.height='100f'/>
                <Group x-id='child2' style.width='100f' style.height='100f'/>
                <Group x-id='child3' style.width='100f' style.height='100f'/>
                <Group x-id='child4' style.width='100f' style.height='100f'/>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayoutThing5 : UIElement {

        public List<UIElement> gridItems;
        
        public override void OnCreate() {
            gridItems = new List<UIElement>();
            int i = 0;
            while (true) {
                var item = FindById("child" + i);
                if (item == null) break;
                gridItems.Add(item);
            }
        }

    }

    
    [Test]
    public void ExplicitPlaced_Fixed3x1() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;
        root.child0.style.gridItem = new GridPlacementParameters(0, 1, 0, 1);
        root.child1.style.gridItem = new GridPlacementParameters(1, 1, 0, 1);
        root.child2.style.gridItem = new GridPlacementParameters(2, 1, 0, 1);
            
        GridDefinition grid = new GridDefinition();
            
        grid.rowTemplate = new [] {
            new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f))
        };

        grid.colTemplate = new [] {
            new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f)),
            new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f)),
            new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f))
        };
        
        root.style.gridDefinition = grid;
        mockView.Update();
        
        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
        
    }

    [Test]
    public void ImplicitRowPlaced_Fixed3x1() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;
        GridDefinition grid = new GridDefinition();
        
        root.child0.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
        root.child1.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
        root.child2.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
        
        grid.autoColSize = new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100));

        root.style.gridDefinition = grid;
        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void PartialImplicitPlaced() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;
        GridDefinition grid = new GridDefinition();
        
        root.child0.style.gridItem = new GridPlacementParameters(0, 1, 0, 1);
        root.child1.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
        root.child2.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
        
        grid.autoColSize = new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100));

        root.style.gridDefinition = grid;
        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }
    
    [Test]
    public void PartialImplicitPlaced_Dense() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;
        GridDefinition grid = new GridDefinition();
        
        root.child0.style.gridItem = new GridPlacementParameters(0, 1, 0, 1);
        root.child1.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
        root.child2.style.gridItem = new GridPlacementParameters(2, 1, 0, 1);
        
        grid.autoColSize = new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100));

        root.style.gridDefinition = grid;
        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }
    
//    [Test]
//    public void PartialImplicitPlacedOutOfOrder() {
//        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
//        mockView.Initialize();
//        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;
//        GridDefinition grid = new GridDefinition();
//        
//        root.child0.style.gridItem = new GridPlacementParameters(0, 1, 0, 1);
//        root.child1.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
//        root.child2.style.gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, 0, 1);
//        
//        grid.autoColSize = new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100));
//
//        root.style.gridDefinition = grid;
//        mockView.Update();
//
//        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
//        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
//        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
//    }
}