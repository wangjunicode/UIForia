using NUnit.Framework;
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
    public class GridLayoutThing : UIElement {

        public UIGroupElement child0;
        public UIGroupElement child1;
        public UIGroupElement child2;
        
        public override void OnCreate() {
            
            child0 = FindById<UIGroupElement>("child0");
            child1 = FindById<UIGroupElement>("child1");
            child2 = FindById<UIGroupElement>("child2");
            
            child0.style.gridItem = new GridPlacement(0, 1, 0, 1);
            child1.style.gridItem = new GridPlacement(0, 1, 1, 1);
            child2.style.gridItem = new GridPlacement(0, 1, 2, 1);
            
            GridDefinition grid = new GridDefinition();
            
            grid.rowTemplate = new [] {
                new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f))
            };

            grid.colTemplate = new [] {
                new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f)),
                new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f)),
                new GridTrackSizer(new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f))
            };

            grid.autoColSize = new GridTrackSizer() {
                minSizingFunction = new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f),
                maxSizingFunction = new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100000f)
            };

            grid.autoRowSize = new GridTrackSizer() {
                minSizingFunction = new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100f),
                maxSizingFunction = new GridTrackSizeFn(GridCellMeasurementType.Pixel, 100000f)
            };
            
            style.gridDefinition = grid;
        }

    }

    [Test]
    public void GridTest() {
        MockView mockView = new MockView(typeof(GridLayoutThing));
        mockView.Initialize();
        mockView.Update();
        GridLayoutThing root = (GridLayoutThing) mockView.RootElement;
        
        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
        
    }

}