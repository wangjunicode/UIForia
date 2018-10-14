using System.Collections.Generic;
using NUnit.Framework;
using Rendering;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Util;
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
                <Repeat list='{gridItems}'>
                    <Group style.width='100f' style.height='100f'/>
                </Repeat>
            </Contents>
        </UITemplate>
    ")]
    public class GridLayoutRepeat : UIElement {

        public List<int> gridItems = new List<int>();

        public UIElement GetChild(int i) {
            return FindByType<UIGroupElement>()[i];
        }

        public void SetChildCount(int childCount) {
            gridItems.Clear();
            for (int i = 0; i < childCount; i++) {
                gridItems.Add(i);
            }
        }

    }

    [Test]
    public void ExplicitPlaced_Fixed3x1() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;
        root.child0.style.SetGridItemPlacement(0, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);


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
        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_Fixed3x1Overlap() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetGridItemColStart(0, StyleState.Normal);
        root.child0.style.SetGridItemColSpan(3, StyleState.Normal);
        root.child0.style.SetGridItemRowStart(0, StyleState.Normal);

        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);


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
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_AlignCenter() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetGridItemColStart(0, StyleState.Normal);
        root.child0.style.SetGridItemColSpan(3, StyleState.Normal);
        root.child0.style.SetGridItemRowStart(0, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(CrossAxisAlignment.Center, StyleState.Normal);

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
        mockView.Update();

        Assert.AreEqual(new Rect(100, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_AlignEnd() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetGridItemColStart(0, StyleState.Normal);
        root.child0.style.SetGridItemColSpan(3, StyleState.Normal);
        root.child0.style.SetGridItemRowStart(0, StyleState.Normal);

        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(CrossAxisAlignment.End, StyleState.Normal);

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
        mockView.Update();

        Assert.AreEqual(new Rect(200, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_AlignStretch() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetGridItemColStart(0, StyleState.Normal);
        root.child0.style.SetGridItemColSpan(3, StyleState.Normal);
        root.child0.style.SetGridItemRowStart(0, StyleState.Normal);

        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);

        root.style.SetGridLayoutColAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);

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
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 300, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_MinWidthSingleColumn() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetGridItemPlacement(0, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);

        var rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        var colTemplate = new[] {
            new GridTrackSize(100f),
            GridTrackSize.MinContent,
            new GridTrackSize(100f)
        };

        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void ExplicitPlaced_MinWidthMultiColumn() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetPreferredWidth(50f, StyleState.Normal);
        root.child1.style.SetPreferredWidth(100f, StyleState.Normal);
        root.child2.style.SetPreferredWidth(100f, StyleState.Normal);

        root.child0.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);

        var rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        var colTemplate = new[] {
            new GridTrackSize(100f),
            GridTrackSize.MinContent, // affects positioning of items, not item width
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
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        mockView.layoutSystem.SetViewportRect(new Rect(0, 0, 1000, 1000));
        root.child0.style.SetPreferredWidth(50f, StyleState.Normal);
        root.child1.style.SetPreferredWidth(100f, StyleState.Normal);
        root.child2.style.SetPreferredWidth(100f, StyleState.Normal);

        root.child0.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(1, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(1, 2, 0, 1, StyleState.Normal);

        var rowTemplate = new List<GridTrackSize>(new[] {
            new GridTrackSize(100f)
        });

        var colTemplate = new[] {
            new GridTrackSize(100f),
            GridTrackSize.Flex,
            new GridTrackSize(100f)
        };

        root.style.SetPreferredWidth(400, StyleState.Normal);
        root.style.SetGridLayoutColAlignment(CrossAxisAlignment.Stretch, StyleState.Normal);
        root.style.SetGridLayoutColTemplate(colTemplate, StyleState.Normal);
        root.style.SetGridLayoutRowTemplate(rowTemplate, StyleState.Normal);
        mockView.Update();

        Assert.AreEqual(new Rect(100, 0, 200, 100), root.child0.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 200, 100), root.child1.layoutResult.ScreenRect);
        Assert.AreEqual(new Rect(100, 0, 300, 100), root.child2.layoutResult.ScreenRect);
    }


    [Test]
    public void ImplicitRowPlaced_Fixed3x1() {
        MockView mockView = new MockView(typeof(GridLayoutThing3x1));
        mockView.Initialize();
        GridLayoutThing3x1 root = (GridLayoutThing3x1) mockView.RootElement;

        root.child0.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, 0, 1, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
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

        root.child0.style.SetGridItemPlacement(0, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, 0, 1, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
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

        root.child0.style.SetGridItemPlacement(0, 1, 0, 1, StyleState.Normal);
        root.child1.style.SetGridItemPlacement(IntUtil.UnsetValue, 1, 0, 1, StyleState.Normal);
        root.child2.style.SetGridItemPlacement(2, 1, 0, 1, StyleState.Normal);

        root.style.SetGridLayoutDensity(GridLayoutDensity.Dense, StyleState.Normal);
        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.child0.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 0), root.child1.layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.child2.layoutResult.localPosition);
    }

    [Test]
    public void MixImplicitAndExplicit() {
        MockView mockView = new MockView(typeof(GridLayoutRepeat));
        mockView.Initialize();
        GridLayoutRepeat root = (GridLayoutRepeat) mockView.RootElement;
        root.SetChildCount(6);
        mockView.Update();

        root.GetChild(0).style.SetGridItemPlacement(0, 2, 0, 1, StyleState.Normal);
        root.GetChild(1).style.SetGridItemPlacement(2, 2, 0, 2, StyleState.Normal);
        root.GetChild(2).style.SetGridItemPlacement(0, 1, 1, 1, StyleState.Normal);
        root.GetChild(3).style.SetGridItemPlacement(1, 1, 1, 1, StyleState.Normal);
        root.GetChild(4).style.SetGridItemPlacement(3, 1, 0, 2, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);
        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.GetChild(0).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(200, 0), root.GetChild(1).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 100), root.GetChild(2).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(100, 100), root.GetChild(3).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(300, 0), root.GetChild(4).layoutResult.localPosition);
    }


    [Test]
    public void DefiningAGrid_WithGaps() {
        MockView mockView = new MockView(typeof(GridLayoutRepeat));
        mockView.Initialize();
        GridLayoutRepeat root = (GridLayoutRepeat) mockView.RootElement;
        root.SetChildCount(6);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColGap(10f, StyleState.Normal);
        root.style.SetGridLayoutRowGap(10f, StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Vector2(0, 0), root.GetChild(0).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(110, 0), root.GetChild(1).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(220, 0), root.GetChild(2).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(0, 110), root.GetChild(3).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(110, 110), root.GetChild(4).layoutResult.localPosition);
        Assert.AreEqual(new Vector2(220, 110), root.GetChild(5).layoutResult.localPosition);
    }


    [Test]
    public void ColSize_MaxContent() {
        MockView mockView = new MockView(typeof(GridLayoutRepeat));
        mockView.Initialize();
        GridLayoutRepeat root = (GridLayoutRepeat) mockView.RootElement;
        root.SetChildCount(6);
        mockView.Update();

        root.GetChild(0).style.SetPreferredWidth(400f, StyleState.Normal);
        root.GetChild(3).style.SetPreferredWidth(600f, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            GridTrackSize.MaxContent,
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 400, 100), root.GetChild(0).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(600, 0, 100, 100), root.GetChild(1).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(700, 0, 100, 100), root.GetChild(2).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(0, 100, 600, 100), root.GetChild(3).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(600, 100, 100, 100), root.GetChild(4).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(700, 100, 100, 100), root.GetChild(5).layoutResult.LocalRect);
    }

    [Test]
    public void ColSize_MinContent() {
        MockView mockView = new MockView(typeof(GridLayoutRepeat));
        mockView.Initialize();
        GridLayoutRepeat root = (GridLayoutRepeat) mockView.RootElement;
        root.SetChildCount(6);
        mockView.Update();

        root.GetChild(0).style.SetPreferredWidth(400f, StyleState.Normal);
        root.GetChild(3).style.SetPreferredWidth(600f, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            GridTrackSize.MinContent,
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutRowAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 400, 100), root.GetChild(0).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(400, 0, 100, 100), root.GetChild(1).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(500, 0, 100, 100), root.GetChild(2).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(0, 100, 600, 100), root.GetChild(3).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(400, 100, 100, 100), root.GetChild(4).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(500, 100, 100, 100), root.GetChild(5).layoutResult.LocalRect);
    }


    [Test]
    public void RowSize_MaxContent() {
        MockView mockView = new MockView(typeof(GridLayoutRepeat));
        mockView.Initialize();
        GridLayoutRepeat root = (GridLayoutRepeat) mockView.RootElement;
        root.SetChildCount(4);
        mockView.Update();

        root.GetChild(0).style.SetPreferredHeight(400f, StyleState.Normal);
        root.GetChild(1).style.SetPreferredHeight(600f, StyleState.Normal);

        root.style.SetGridLayoutRowTemplate(new[] {
            GridTrackSize.MaxContent,
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 400), root.GetChild(0).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(100, 0, 100, 600), root.GetChild(1).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(0, 600, 100, 100), root.GetChild(2).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(100, 600, 100, 100), root.GetChild(3).layoutResult.LocalRect);
    }

    [Test]
    public void RowSize_MinContent() {
        MockView mockView = new MockView(typeof(GridLayoutRepeat));
        mockView.Initialize();
        GridLayoutRepeat root = (GridLayoutRepeat) mockView.RootElement;
        root.SetChildCount(4);
        mockView.Update();

        root.GetChild(0).style.SetPreferredHeight(400f, StyleState.Normal);
        root.GetChild(1).style.SetPreferredHeight(600f, StyleState.Normal);

        root.style.SetGridLayoutRowTemplate(new[] {
            GridTrackSize.MinContent,
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColTemplate(new[] {
            new GridTrackSize(100f),
            new GridTrackSize(100f)
        }, StyleState.Normal);

        root.style.SetGridLayoutColAutoSize(new GridTrackSize(100f), StyleState.Normal);

        mockView.Update();

        Assert.AreEqual(new Rect(0, 0, 100, 400), root.GetChild(0).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(100, 0, 100, 600), root.GetChild(1).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(0, 400, 100, 100), root.GetChild(2).layoutResult.LocalRect);
        Assert.AreEqual(new Rect(100, 400, 100, 100), root.GetChild(3).layoutResult.LocalRect);
    }

}