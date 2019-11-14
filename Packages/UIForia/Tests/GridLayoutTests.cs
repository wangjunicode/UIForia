using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

public class GridLayoutTests {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents style.layoutType='LayoutType.Grid'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayoutThing3Children : UIElement {

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
            <Contents style.layoutType='LayoutType.Grid'>
                <Group>A</Group>
                <Group>B</Group>
                <Group>C</Group>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayoutVanilla3x1 : UIElement { }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents style.layoutType='LayoutType.Grid'>
                <Group style.preferredWidth='100f' style.preferredHeight='100f'>A</Group>
                <Group style.preferredWidth='100f' style.preferredHeight='100f'>B</Group>
                <Group style.preferredWidth='100f' style.preferredHeight='100f'>C</Group>
                <Group style.preferredWidth='100f' style.preferredHeight='100f'>D-F</Group>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayout3x1Plus1x3 : UIElement { }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents style.layoutType='LayoutType.Grid'>
                <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child1' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child2' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child3' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child4' style.preferredWidth='100f' style.preferredHeight='100f'/>
                <Group x-id='child5' style.preferredWidth='100f' style.preferredHeight='100f'/>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayout6Children : UIElement {

        public UIElement GetChildAt(int i) {
            return FindByType<UIGroupElement>()[i];
        }

    }

    [Test]
    public void ExplicitPlaced_Fixed3x1_Auto() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        root.style.SetGridLayoutDirection(LayoutDirection.Horizontal, StyleState.Normal);
        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_Fixed3x1_DefinedTrack() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        root.style.SetGridLayoutDirection(LayoutDirection.Horizontal, StyleState.Normal);
        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        GridTrackSize[] rowTemplate = new[] {
            new GridTrackSize(100f)
        };

        GridTrackSize[] colTemplate = new[] {
            new GridTrackSize(200f),
            new GridTrackSize(200f),
            new GridTrackSize(200f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(400, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_Fixed3x1Overlap() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 0, StyleState.Normal);
        root.child0.style.SetGridItemWidth(3, StyleState.Normal);

        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        GridTrackSize[] rowTemplate = {
            new GridTrackSize(100f)
        };

        GridTrackSize[] colTemplate = {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 300, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ExplicitPlaced_AlignCenter() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);
        root.child0.style.SetGridItemHeight(3, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(GridAxisAlignment.Center, StyleState.Normal);

        var rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        var colTemplate = new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 100, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_AlignEnd() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemX(0, StyleState.Normal);
        root.child0.style.SetGridItemY(0, StyleState.Normal);
        root.child0.style.SetGridItemHeight(3, StyleState.Normal);

        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(GridAxisAlignment.End, StyleState.Normal);
        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100), StyleState.Normal);

        GridTrackSize[] colTemplate = new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        };

        List<GridTrackSize> rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 200, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ExplicitPlaced_AlignStretch() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemX(0, StyleState.Normal);
        root.child0.style.SetGridItemY(0, StyleState.Normal);
        root.child0.style.SetGridItemHeight(3, StyleState.Normal);

        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(GridAxisAlignment.Grow, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100), StyleState.Normal);

        List<GridTrackSize> rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        GridTrackSize[] colTemplate = new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 300), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_MinWidthSingleColumn() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        List<GridTrackSize> rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        GridTrackSize[] colTemplate = new[] {
            new GridTrackSize(100f),
            GridTrackSize.MinContent,
            new GridTrackSize(100f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ExplicitPlaced_MinWidthMultiColumn() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetPreferredWidth(50f, StyleState.Normal);
        root.child1.style.SetPreferredWidth(100f, StyleState.Normal);
        root.child2.style.SetPreferredWidth(100f, StyleState.Normal);

        root.child0.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(GridAxisAlignment.Start, StyleState.Normal);

        List<GridTrackSize> rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        GridTrackSize[] colTemplate = new[] {
            new GridTrackSize(100f),
            GridTrackSize.MinContent,
            new GridTrackSize(100f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(100, 0, 50, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(150, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void ExplicitPlaced_Flex() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        mockView.SetViewportRect(new Rect(0, 0, 1000, 1000));
        root.child0.style.SetPreferredWidth(50f, StyleState.Normal);
        root.child1.style.SetPreferredWidth(100f, StyleState.Normal);
        root.child2.style.SetPreferredWidth(100f, StyleState.Normal);

        root.child0.style.SetGridItemPlacement(1, 0, 1, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 0, 1, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(1, 0, 2, 1, StyleState.Normal);

        var rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        var colTemplate = new[] {
            new GridTrackSize(100f),
            GridTrackSize.FractionalRemaining,
            new GridTrackSize(100f)
        };

        root.style.SetPreferredWidth(400, StyleState.Normal);
        root.style.SetGridLayoutColAlignment(GridAxisAlignment.Grow, StyleState.Normal);
        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(100, 0, 200, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 200, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 300, 100), root.child2.layoutResult.ScreenRect);
    }


    [Test]
    public void ImplicitRowPlaced_Fixed3x1() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(-1, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(-1, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(-1, 0, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ImplicitColumnUnplaced_Fixed3x1() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.style.SetGridLayoutDirection(LayoutDirection.Vertical, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(GridTrackSize.MaxContent, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child2.layoutResult.ScreenRect);
    }

    [Test]
    public void PartialImplicitPlaced() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Vector2(0, 100), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 100), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 100), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void PartialImplicitPlaced_Dense() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.child0.style.SetGridItemPlacement(0, 0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(IntUtil.UnsetValue, 0, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 0, StyleState.Normal);

        root.style.SetGridLayoutDensity(GridLayoutDensity.Dense, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void MixImplicitAndExplicit() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.GetChildAt(0).style.SetGridItemPlacement(0, 0, 2, 1, StyleState.Normal);
        
        root.GetChildAt(1).style.SetGridItemPlacement(2, 0, 2, 2, StyleState.Normal);
        root.GetChildAt(2).style.SetGridItemPlacement(0, 1, 1, 1, StyleState.Normal);
        root.GetChildAt(3).style.SetGridItemPlacement(1, 1, 1, 1, StyleState.Normal);
        root.GetChildAt(4).style.SetGridItemPlacement(3, 0, 1, 2, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.GetChildAt(0).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.GetChildAt(1).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.GetChildAt(2).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 100), root.GetChildAt(3).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(300, 0), root.GetChildAt(4).layoutResult.localPosition);
    }


    [Test]
    public void DefiningAGrid_WithGaps() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(GridTrackSize.MaxContent, StyleState.Normal);
        root.style.SetGridLayoutColGap(10f, StyleState.Normal);
        root.style.SetGridLayoutRowGap(10f, StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.GetChildAt(0).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(110, 0), root.GetChildAt(1).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(220, 0), root.GetChildAt(2).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 110), root.GetChildAt(3).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(110, 110), root.GetChildAt(4).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(220, 110), root.GetChildAt(5).layoutResult.localPosition);
    }

    [Test]
    public void RowSize_MinContent() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.GetChildAt(0).style.SetPreferredHeight(400f, StyleState.Normal);
        root.GetChildAt(1).style.SetPreferredHeight(600f, StyleState.Normal);

        root.style.SetGridLayoutRowTemplate(new[] {
            GridTrackSize.MinContent,
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAlignment(GridAxisAlignment.Start, StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 400), root.GetChildAt(0).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 0, 100, 400), root.GetChildAt(1).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.GetChildAt(2).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 400, 100, 100), root.GetChildAt(3).layoutResult.AllocatedRect);
    }

    [Test]
    public void RowStartLocked_ColumnFlow() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.style.SetGridLayoutDirection(LayoutDirection.Vertical, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(null, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(null, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100), StyleState.Normal);
        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100), StyleState.Normal);

        root.GetChildAt(0).style.SetGridItemX(0, StyleState.Normal);
        root.GetChildAt(1).style.SetGridItemX(0, StyleState.Normal);
        root.GetChildAt(2).style.SetGridItemX(0, StyleState.Normal);

        root.GetChildAt(3).style.SetGridItemX(1, StyleState.Normal);
        root.GetChildAt(4).style.SetGridItemX(1, StyleState.Normal);
        root.GetChildAt(5).style.SetGridItemX(1, StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.GetChildAt(0).layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.GetChildAt(1).layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.GetChildAt(2).layoutResult.ScreenRect);

        Assert.AreEqual(new Rect(100, 0, 100, 100), root.GetChildAt(3).layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 100, 100, 100), root.GetChildAt(4).layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 200, 100, 100), root.GetChildAt(5).layoutResult.ScreenRect);
    }

    [Test]
    public void ColumnMargin_ShouldOffsetFromCell() {
        MockApplication mockView = new MockApplication(typeof(GridLayoutThing3Children));
        GridLayoutThing3Children root = (GridLayoutThing3Children) mockView.RootElement.GetChild(0);

        root.style.SetGridLayoutDirection(LayoutDirection.Horizontal, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100), StyleState.Normal);
        
        root.GetChild(0).style.SetMarginLeft(30, StyleState.Normal);
        root.GetChild(1).style.SetMarginTop(30, StyleState.Normal);
        root.GetChild(2).style.SetMarginBottom(30, StyleState.Normal);
        root.GetChild(2).style.SetMarginRight(30, StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(30, 0, 100, 100), root.GetChild(0).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 30, 100, 100), root.GetChild(1).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.GetChild(2).layoutResult.AllocatedRect);
        
    }

    [Test]
    public void ComplexLayoutHorizontalSparse() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.GetChildAt(1).style.SetGridItemHeight(2, StyleState.Normal);
        root.GetChildAt(3).style.SetGridItemWidth(2, StyleState.Normal);
        root.style.SetGridLayoutDensity(GridLayoutDensity.Sparse, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(GridTrackSize.MaxContent, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.GetChildAt(0).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 0, 100, 200), root.GetChildAt(1).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.GetChildAt(2).layoutResult.AllocatedRect);
        
        Assert.AreEqual(new Rect(200, 100, 200, 100), root.GetChildAt(3).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 200, 100, 100), root.GetChildAt(4).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 200, 100, 100), root.GetChildAt(5).layoutResult.AllocatedRect);
    }

    [Test]
    public void ComplexLayoutHorizontalDense() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.GetChildAt(1).style.SetGridItemHeight(2, StyleState.Normal);
        root.GetChildAt(3).style.SetGridItemWidth(2, StyleState.Normal);
        root.style.SetGridLayoutDensity(GridLayoutDensity.Dense, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(GridTrackSize.MaxContent, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.GetChildAt(0).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 0, 100, 200), root.GetChildAt(1).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(200, 0, 100, 100), root.GetChildAt(2).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(200, 100, 200, 100), root.GetChildAt(3).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(300, 0, 100, 100), root.GetChildAt(4).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 100, 100, 100), root.GetChildAt(5).layoutResult.AllocatedRect);
    }

    [Test]
    public void ComplexLayoutVerticalSparse() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.style.SetGridLayoutRowTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.GetChildAt(1).style.SetGridItemHeight(2, StyleState.Normal);
        root.GetChildAt(3).style.SetGridItemWidth(2, StyleState.Normal);
        root.style.SetGridLayoutDirection(LayoutDirection.Vertical, StyleState.Normal);
        root.style.SetGridLayoutDensity(GridLayoutDensity.Sparse, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(100f, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.GetChildAt(0).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 100, 100, 200), root.GetChildAt(1).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 300, 100, 100), root.GetChildAt(2).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 0, 200, 100), root.GetChildAt(3).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 100, 100, 100), root.GetChildAt(4).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 200, 100, 100), root.GetChildAt(5).layoutResult.AllocatedRect);
    }

    [Test] // same as sparse
    public void ComplexLayoutVerticalDense() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);
        mockView.Update();

        root.style.SetGridLayoutRowTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.GetChildAt(1).style.SetGridItemHeight(2, StyleState.Normal);
        root.GetChildAt(3).style.SetGridItemWidth(2, StyleState.Normal);
        root.style.SetGridLayoutDirection(LayoutDirection.Vertical, StyleState.Normal);
        root.style.SetGridLayoutDensity(GridLayoutDensity.Dense, StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(100f, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.GetChildAt(0).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 100, 100, 200), root.GetChildAt(1).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(0, 300, 100, 100), root.GetChildAt(2).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 0, 200, 100), root.GetChildAt(3).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 100, 100, 100), root.GetChildAt(4).layoutResult.AllocatedRect);
        Assert.AreEqual(new Rect(100, 200, 100, 100), root.GetChildAt(5).layoutResult.AllocatedRect);
    }

    [Test]
    public void AssignBasicPlacements() {
        MockApplication mockView = new MockApplication(typeof(GridLayout6Children));
        GridLayout6Children root = (GridLayout6Children) mockView.RootElement.GetChild(0);

        root[0].style.SetGridItemPlacement(0, 0, 1, 1, StyleState.Normal);
        root[1].style.SetGridItemPlacement(1, 0, 1, 1, StyleState.Normal);
        root[2].style.SetGridItemPlacement(2, 0, 1, 1, StyleState.Normal);
        root[3].style.SetGridItemPlacement(0, 1, 1, 1, StyleState.Normal);
        root[4].style.SetGridItemPlacement(1, 1, 1, 1, StyleState.Normal);
        root[5].style.SetGridItemPlacement(2, 1, 1, 1, StyleState.Normal);

        mockView.Update();

        AwesomeGridLayoutBox box = (AwesomeGridLayoutBox) root.awesomeLayoutBox;

        Assert.AreEqual(2, box.RowCount);
        Assert.AreEqual(3, box.ColCount);
    }

   
}