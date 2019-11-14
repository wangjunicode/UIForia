using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class GridLayoutTest {

        [Template("Data/Layout/Grid/GridLayout_RowSizeMinContent.xml")]
        public class GridLayout_RowSize_MinContent : UIElement { }

        [Test]
        public void RowSize_MinContent() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_RowSize_MinContent));
            GridLayout_RowSize_MinContent root = (GridLayout_RowSize_MinContent) mockView.RootElement.GetChild(0);

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 400), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 400), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 400, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 400, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 500, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ColSizeMaxContent.xml")]
        public class GridLayout_ColSizeMaxContent : UIElement { }

        [Test]
        public void ColSize_MaxContent() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_ColSizeMaxContent));
            GridLayout_ColSizeMaxContent root = (GridLayout_ColSizeMaxContent) mockView.RootElement.GetChild(0);

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 600, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(600, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(700, 0, 100, 100), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Rect(0, 100, 600, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(600, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(700, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ColSizeMinContent.xml")]
        public class GridLayout_ColSizeMinContent : UIElement { }

        [Test]
        public void ColSize_MinContent() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_ColSizeMinContent));
            GridLayout_ColSizeMinContent root = (GridLayout_ColSizeMinContent) mockView.RootElement.GetChild(0);

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 400, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_GrowToMaxSize.xml")]
        public class GridLayout_GrowToMaxSize : UIElement { }

        [Test]
        public void GrowToMaxSize_NoFractions() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_GrowToMaxSize));
            mockView.Update();
            GridLayout_GrowToMaxSize root = (GridLayout_GrowToMaxSize) mockView.RootElement.GetChild(0);
            Assert.AreEqual(1000, root.layoutResult.actualSize.width);

            Assert.AreEqual(0, root[0].layoutResult.allocatedPosition.x);
            Assert.AreEqual(300, root[1].layoutResult.allocatedPosition.x);
            Assert.AreEqual(700, root[2].layoutResult.allocatedPosition.x);
        }

        [Template("Data/Layout/Grid/GridLayout_GrowToMaxSize_Fractional.xml")]
        public class GridLayout_GrowToMaxSize_Fractional : UIElement { }

        [Test]
        public void GrowToMaxSize_Fractions() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_GrowToMaxSize_Fractional));
            GridLayout_GrowToMaxSize_Fractional root = (GridLayout_GrowToMaxSize_Fractional) mockView.RootElement.GetChild(0);

            mockView.Update();

            Assert.AreEqual(1000, root.layoutResult.actualSize.width);

            Assert.AreEqual(0, root[0].layoutResult.allocatedPosition.x);
            Assert.AreEqual(400, root[0].layoutResult.allocatedSize.width);

            Assert.AreEqual(400, root[1].layoutResult.allocatedPosition.x);
            Assert.AreEqual(400, root[1].layoutResult.allocatedSize.width);

            Assert.AreEqual(800, root[2].layoutResult.allocatedPosition.x);
            Assert.AreEqual(200, root[2].layoutResult.allocatedSize.width);
        }


        [Template("Data/Layout/Grid/GridLayout_ResolveMaxContentTrackSize.xml")]
        public class GridLayout_ResolveMaxContentTrackSize : UIElement { }

        [Test]
        public void ResolveMaxContentTrackSize() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_ResolveMaxContentTrackSize));
            mockView.Update();
            GridLayout_ResolveMaxContentTrackSize root = (GridLayout_ResolveMaxContentTrackSize) mockView.RootElement.GetChild(0);
            Assert.AreEqual(300, root.layoutResult.actualSize.width);

            Assert.AreEqual(0, root[0].layoutResult.allocatedPosition.x);
            Assert.AreEqual(100, root[1].layoutResult.allocatedPosition.x);
            Assert.AreEqual(0, root[2].layoutResult.allocatedPosition.x); // wraps to next line because prev was 2 wide
        }

        [Test]
        public void ResolveMaxContentTrackSize_WithColGap() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_ResolveMaxContentTrackSize));
            GridLayout_ResolveMaxContentTrackSize root = (GridLayout_ResolveMaxContentTrackSize) mockView.RootElement.GetChild(0);
            root.style.SetGridLayoutColGap(10, StyleState.Normal);
            mockView.Update();
            Assert.AreEqual(320, root.layoutResult.actualSize.width);
        }

        [Template("Data/Layout/Grid/GridLayout_ColCollapseMaxSizeContribution.xml")]
        public class GridLayout_ColCollapseMaxSizeContribution : UIElement { }

        [Test]
        public void ColCollapseMaxSizeContribution() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_ColCollapseMaxSizeContribution));
            GridLayout_ColCollapseMaxSizeContribution root = (GridLayout_ColCollapseMaxSizeContribution) mockView.RootElement.GetChild(0);

            mockView.Update();
            
            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 300, 200, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
            
        }
        
        [Template("Data/Layout/Grid/GridLayout_ColMaxSizeContribution_NotCollapsed.xml")]
        public class GridLayout_ColMaxSizeContribution_NotCollapsed : UIElement { }

        [Test]
        public void ColMaxSizeContribution_NotCollapsed() {
            MockApplication mockView = new MockApplication(typeof(GridLayout_ColMaxSizeContribution_NotCollapsed));
            GridLayout_ColMaxSizeContribution_NotCollapsed root = (GridLayout_ColMaxSizeContribution_NotCollapsed) mockView.RootElement.GetChild(0);

            mockView.Update();
            
            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 300, 200, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[6].layoutResult.AllocatedRect);
            
        }
        
    }

}