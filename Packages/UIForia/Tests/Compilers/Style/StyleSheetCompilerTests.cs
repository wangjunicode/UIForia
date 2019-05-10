using NUnit.Framework;
using UIForia;
using UIForia.Animation;
using UIForia.Compilers.Style;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

[TestFixture]
public class StyleSheetCompilerTests {

    public static StyleSheetCompiler NewStyleSheetCompiler() {
        return new StyleSheetCompiler(new StyleSheetImporter(null));
    }

    [Test]
    public void CompileEmptyStyle() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        Assert.IsNotNull(styleSheet);
        Assert.AreEqual(0, styleSheet.styleGroupContainers.Length);
    }

    [Test]
    public void CompileBackgroundImage() {
        var nodes = StyleParser.Parse(@"

const path = ""testimg/cat"";
export const img1 = url(@path);

style image1 { BackgroundImage = @img1; }
style image2 { BackgroundImage = url(@path); }
style image3 { BackgroundImage = url(testimg/cat); }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(3, containers.Length);

        Assert.AreEqual("cat", containers[0].groups[0].normal.BackgroundImage.name);
        Assert.AreEqual("cat", containers[1].groups[0].normal.BackgroundImage.name);
        Assert.AreEqual("cat", containers[2].groups[0].normal.BackgroundImage.name);
    }

    [Test]
    public void CompileCursor() {
        var nodes = StyleParser.Parse(@"

const path = ""testimg/Cursor1"";
export const cursor1 = url(@path);

style image1 { Cursor = @cursor1 1 2; }
style image2 { Cursor = url(@path) 20; }
style image3 { Cursor = url(testimg/Cursor1); }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(3, containers.Length);

        Assert.AreEqual("Cursor1", containers[0].groups[0].normal.Cursor.texture.name);
        Assert.AreEqual(new Vector2(1, 2), containers[0].groups[0].normal.Cursor.hotSpot);

        Assert.AreEqual("Cursor1", containers[1].groups[0].normal.Cursor.texture.name);
        Assert.AreEqual(new Vector2(20, 20), containers[1].groups[0].normal.Cursor.hotSpot);

        Assert.AreEqual("Cursor1", containers[2].groups[0].normal.Cursor.texture.name);
        Assert.AreEqual(new Vector2(0, 0), containers[2].groups[0].normal.Cursor.hotSpot);
    }

    [Test]
    public void CompileVisibility() {
        var nodes = StyleParser.Parse(@"

const v1 = Visible;
export const v2 = hidden;

style visi1 { Visibility = @v1; }
style visi2 { Visibility = @v2; }
style visi3 { Visibility = Visible; }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(3, containers.Length);

        Assert.AreEqual(Visibility.Visible, containers[0].groups[0].normal.Visibility);
        Assert.AreEqual(Visibility.Hidden, containers[1].groups[0].normal.Visibility);
        Assert.AreEqual(Visibility.Visible, containers[2].groups[0].normal.Visibility);
    }

    [Test]
    public void CompileOverflow() {
        var nodes = StyleParser.Parse(@"

const o1 = hidden;
const o2 = Scroll;

style overflow1 { Overflow = @o1 @o2; }
style overflow2 { Overflow = @o2; }
style overflow3 { OverflowX = @o2; }
style overflow4 { OverflowY = @o1; }
style overflow5 {
    Overflow = hidden; 
    OverflowY = Scroll;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(5, containers.Length);

        Assert.AreEqual(Overflow.Hidden, containers[0].groups[0].normal.OverflowX);
        Assert.AreEqual(Overflow.Scroll, containers[0].groups[0].normal.OverflowY);

        Assert.AreEqual(Overflow.Scroll, containers[1].groups[0].normal.OverflowX);
        Assert.AreEqual(Overflow.Scroll, containers[1].groups[0].normal.OverflowY);

        Assert.AreEqual(Overflow.Scroll, containers[2].groups[0].normal.OverflowX);
        Assert.AreEqual(Overflow.Unset, containers[2].groups[0].normal.OverflowY);

        Assert.AreEqual(Overflow.Unset, containers[3].groups[0].normal.OverflowX);
        Assert.AreEqual(Overflow.Hidden, containers[3].groups[0].normal.OverflowY);

        Assert.AreEqual(Overflow.Hidden, containers[4].groups[0].normal.OverflowX);
        Assert.AreEqual(Overflow.Scroll, containers[4].groups[0].normal.OverflowY);
    }

    [Test]
    public void CompileBackgroundColor() {
        var nodes = StyleParser.Parse(@"
            
const alpha = 255;
const redChannel = 255.000;

export const color0 = rgba(@redChannel, 0, 0, @alpha);
            
style myStyle {
    BackgroundColor = @color0;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(Color.red, containers[0].groups[0].normal.BackgroundColor);
    }

    [Test]
    public void CreateAttributeGroupsWithMeasurements() {
        var nodes = StyleParser.Parse(@"

export const m1 = 10%;

style myStyle {
    MarginTop = @m1;
    [hover] {
        MarginLeft = 20px;
    }
    [attr:myAttr=""val""] {
        MarginTop = 20px; 
    }
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);
        Assert.AreEqual(2, containers[0].groups.Count);

        Assert.AreEqual(10, containers[0].groups[0].normal.MarginTop.value);
        Assert.AreEqual(UIMeasurementUnit.Pixel, containers[0].groups[0].normal.MarginTop.unit);
        Assert.AreEqual(20, containers[0].groups[0].hover.MarginLeft.value);
        Assert.AreEqual(20, containers[0].groups[1].normal.MarginTop.value);
    }

    [Test]
    public void UseMarginPropertyShorthand() {
        var nodes = StyleParser.Parse(@"
            
export const m1 = 10pca;
export const m2 = @m3;
export const m3 = 10pca;
export const m4 = @m2;
            
style myStyle {
    Margin = @m1 @m2 10px @m4;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(10, containers[0].groups[0].normal.MarginTop.value);
        Assert.AreEqual(10, containers[0].groups[0].normal.MarginRight.value);
        Assert.AreEqual(10, containers[0].groups[0].normal.MarginBottom.value);
        Assert.AreEqual(10, containers[0].groups[0].normal.MarginLeft.value);
        Assert.AreEqual(UIMeasurementUnit.ParentContentArea, containers[0].groups[0].normal.MarginTop.unit);
        Assert.AreEqual(UIMeasurementUnit.ParentContentArea, containers[0].groups[0].normal.MarginRight.unit);
        Assert.AreEqual(UIMeasurementUnit.Pixel, containers[0].groups[0].normal.MarginBottom.unit);
        Assert.AreEqual(UIMeasurementUnit.ParentContentArea, containers[0].groups[0].normal.MarginLeft.unit);
    }

    [Test]
    public void UsePaddingPropertyShorthand() {
        var nodes = StyleParser.Parse(@"

export const p1 = 10%;
export const p2 = @p3;
export const p3 = 10%;
export const p4 = @p2;

style myStyle {
    Padding = @p1 @p2 20px @p4;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(10 * 0.01f, containers[0].groups[0].normal.PaddingTop.value);
        Assert.AreEqual(10 * 0.01f, containers[0].groups[0].normal.PaddingRight.value);
        Assert.AreEqual(20, containers[0].groups[0].normal.PaddingBottom.value);
        Assert.AreEqual(10 * 0.01f, containers[0].groups[0].normal.PaddingLeft.value);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.PaddingTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.PaddingRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, containers[0].groups[0].normal.PaddingBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.PaddingLeft.unit);
    }

    [Test]
    public void UseBorderPropertyShorthand() {
        var nodes = StyleParser.Parse(@"

export const b1 = 10%;
export const b2 = @b3;
export const b3 = 10%;
export const b4 = @b2;

style myStyle {
    Border = @b1 @b2 20px @b4;
    BorderColor = black;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(10 * 0.01f, containers[0].groups[0].normal.BorderTop.value);
        Assert.AreEqual(10 * 0.01f, containers[0].groups[0].normal.BorderRight.value);
        Assert.AreEqual(20, containers[0].groups[0].normal.BorderBottom.value);
        Assert.AreEqual(10 * 0.01f, containers[0].groups[0].normal.BorderLeft.value);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.BorderTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.BorderRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, containers[0].groups[0].normal.BorderBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.BorderLeft.unit);

        Assert.AreEqual(Color.black, containers[0].groups[0].normal.BorderColor);
    }

    [Test]
    public void CompileVisibilty() {
        var nodes = StyleParser.Parse(@"

const v = hidden;

style myStyle {
    Visibility = visible;
    [attr:disabled=""disabled""] {
        Visibility = @v;
    }
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);
        Assert.AreEqual(2, containers[0].groups.Count);

        Assert.AreEqual(Visibility.Visible, containers[0].groups[0].normal.Visibility);
        Assert.AreEqual(Visibility.Hidden, containers[0].groups[1].normal.Visibility);
        Assert.AreEqual("disabled", containers[0].groups[1].rule.attributeName);
        Assert.AreEqual("disabled", containers[0].groups[1].rule.attributeValue);
        Assert.AreEqual(false, containers[0].groups[1].rule.invert);
    }

    [Test]
    public void CompileGridItemColAndRowProperties() {
        var nodes = StyleParser.Parse(@"

const rowStart = 2;

style myStyle {
    GridItemColStart = 0;
    GridItemColSpan = 4;
    GridItemRowStart = @rowStart;
    GridItemRowSpan = 5;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(0, containers[0].groups[0].normal.GridItemColStart);
        Assert.AreEqual(4, containers[0].groups[0].normal.GridItemColSpan);
        Assert.AreEqual(2, containers[0].groups[0].normal.GridItemRowStart);
        Assert.AreEqual(5, containers[0].groups[0].normal.GridItemRowSpan);
    }

    [Test]
    public void CompileGridAxisAlignmentProperties() {
        var nodes = StyleParser.Parse(@"

const colSelfAlignment = Center;

style myStyle {
    GridItemColSelfAlignment = @colSelfAlignment;
    GridItemRowSelfAlignment = End;
    GridLayoutColAlignment = Shrink;
    GridLayoutRowAlignment = fit;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(GridAxisAlignment.Center, containers[0].groups[0].normal.GridItemColSelfAlignment);
        Assert.AreEqual(GridAxisAlignment.End, containers[0].groups[0].normal.GridItemRowSelfAlignment);
        Assert.AreEqual(GridAxisAlignment.Shrink, containers[0].groups[0].normal.GridLayoutColAlignment);
        Assert.AreEqual(GridAxisAlignment.Fit, containers[0].groups[0].normal.GridLayoutRowAlignment);
    }

    [Test]
    public void CompileGridLayoutDensity() {
        var nodes = StyleParser.Parse(@"

const density = dense;

style myStyle {
    GridLayoutDensity = @density;
    [hover] { GridLayoutDensity = sparse; }
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(GridLayoutDensity.Dense, containers[0].groups[0].normal.GridLayoutDensity);
        Assert.AreEqual(GridLayoutDensity.Sparse, containers[0].groups[0].hover.GridLayoutDensity);
    }

    [Test]
    public void CompileGridLayoutDirection() {
        var nodes = StyleParser.Parse(@"

const dir = Horizontal;

style myStyle {
    GridLayoutDirection = @dir;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);
        Assert.AreEqual(LayoutDirection.Horizontal, styleGroup[0].groups[0].normal.GridLayoutDirection);
    }

    [Test]
    public void CompileFlexLayoutDirection() {
        var nodes = StyleParser.Parse(@"

const dir = Vertical;

style myStyle {
    FlexLayoutDirection = @dir;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);
        Assert.AreEqual(LayoutDirection.Vertical, styleGroup[0].groups[0].normal.FlexLayoutDirection);
    }

    [Test]
    public void CompileGridLayoutColTemplate() {
        var nodes = StyleParser.Parse(@"

const colOne = 1mx;

style myStyle {
    GridLayoutColTemplate = @colOne 1mx 2fr 480px;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(4, styleGroup[0].groups[0].normal.GridLayoutColTemplate.Count);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.GridLayoutColTemplate[0]);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.GridLayoutColTemplate[1]);
        Assert.AreEqual(new GridTrackSize(2, GridTemplateUnit.FractionalRemaining), styleGroup[0].groups[0].normal.GridLayoutColTemplate[2]);
        Assert.AreEqual(new GridTrackSize(480, GridTemplateUnit.Pixel), styleGroup[0].groups[0].normal.GridLayoutColTemplate[3]);
    }

    [Test]
    public void CompileGridLayoutRowTemplate() {
        var nodes = StyleParser.Parse(@"

const colOne = 1mx;

style myStyle {
    GridLayoutRowTemplate = @colOne 1mx 2fr 480px;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(4, styleGroup[0].groups[0].normal.GridLayoutRowTemplate.Count);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.GridLayoutRowTemplate[0]);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.GridLayoutRowTemplate[1]);
        Assert.AreEqual(new GridTrackSize(2, GridTemplateUnit.FractionalRemaining), styleGroup[0].groups[0].normal.GridLayoutRowTemplate[2]);
        Assert.AreEqual(new GridTrackSize(480, GridTemplateUnit.Pixel), styleGroup[0].groups[0].normal.GridLayoutRowTemplate[3]);
    }

    [Test]
    public void CompileGridLayoutAxisAutoSize() {
        var nodes = StyleParser.Parse(@"
const main = 1fr;

style myStyle {
    GridLayoutMainAxisAutoSize = @main;
    GridLayoutCrossAxisAutoSize = 42px;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.FractionalRemaining), styleGroup[0].groups[0].normal.GridLayoutMainAxisAutoSize);
        Assert.AreEqual(new GridTrackSize(42, GridTemplateUnit.Pixel), styleGroup[0].groups[0].normal.GridLayoutCrossAxisAutoSize);
    }

    [Test]
    public void CompileGridLayoutGaps() {
        var nodes = StyleParser.Parse(@"
const colGap = 9;

style myStyle {
    GridLayoutColGap = @colGap;
    GridLayoutRowGap = 42.01f;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(9, styleGroup[0].groups[0].normal.GridLayoutColGap);
        Assert.AreEqual(42.01f, styleGroup[0].groups[0].normal.GridLayoutRowGap);
    }

    [Test]
    public void CompileFlexAlignments() {
        var nodes = StyleParser.Parse(@"
const axis = Stretch;

style myStyle {
    FlexItemSelfAlignment = Center;
    FlexLayoutCrossAxisAlignment = @axis;
    FlexLayoutMainAxisAlignment = SpaceAround;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(CrossAxisAlignment.Center, styleGroup[0].groups[0].normal.FlexItemSelfAlignment);
        Assert.AreEqual(CrossAxisAlignment.Stretch, styleGroup[0].groups[0].normal.FlexLayoutCrossAxisAlignment);
        Assert.AreEqual(MainAxisAlignment.SpaceAround, styleGroup[0].groups[0].normal.FlexLayoutMainAxisAlignment);
    }

    [Test]
    public void CompileFlexProperties() {
        var nodes = StyleParser.Parse(@"
export const wrap = WrapReverse;
export const grow = 1;

style myStyle {
    FlexItemOrder = 2;
    FlexItemGrow = @grow;
    FlexItemShrink = 0;
    FlexLayoutWrap = @wrap;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(2, styleGroup[0].groups[0].normal.FlexItemOrder);
        Assert.AreEqual(1, styleGroup[0].groups[0].normal.FlexItemGrow);
        Assert.AreEqual(0, styleGroup[0].groups[0].normal.FlexItemShrink);
        Assert.AreEqual(LayoutWrap.WrapReverse, styleGroup[0].groups[0].normal.FlexLayoutWrap);
    }

    [Test]
    public void CompileBorder() {
        var nodes = StyleParser.Parse(@"
export const brtl = 1px;
export const brtr = 2%;
export const brbr = 3vw;
export const brbl = 4em;

style border1 {
    Border = @brtl @brtr @brbr @brbl;
}

style border2 {
    Border = @brtl 20px @brbr;
}

style border3 {
    Border = @brtl @brtr;
}

style border4 {
    Border = 5px;
}
style border5 {
    BorderTop = 1px;
    BorderRight = 20vh;
    BorderBottom = 2em;
    BorderLeft = 4px;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(5, styleGroup.Length);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[0].groups[0].normal.BorderTop);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[0].groups[0].normal.BorderRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[0].groups[0].normal.BorderBottom);
        Assert.AreEqual(new UIFixedLength(4, UIFixedUnit.Em), styleGroup[0].groups[0].normal.BorderLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[1].groups[0].normal.BorderTop);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.BorderRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[1].groups[0].normal.BorderBottom);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.BorderLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.BorderTop);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.BorderRight);
        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.BorderBottom);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.BorderLeft);

        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderTop);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderRight);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderBottom);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[4].groups[0].normal.BorderTop);
        Assert.AreEqual(new UIFixedLength(20, UIFixedUnit.ViewportHeight), styleGroup[4].groups[0].normal.BorderRight);
        Assert.AreEqual(new UIFixedLength(2, UIFixedUnit.Em), styleGroup[4].groups[0].normal.BorderBottom);
        Assert.AreEqual(new UIFixedLength(4), styleGroup[4].groups[0].normal.BorderLeft);
    }

    [Test]
    public void CompilBorderRadius() {
        var nodes = StyleParser.Parse(@"
export const brtl = 1px;
export const brtr = 2%;
export const brbr = 3vw;
export const brbl = 4em;

style border1 {
    BorderRadius = @brtl @brtr @brbr @brbl;
}

style border2 {
    BorderRadius = @brtl 20px @brbr;
}

style border3 {
    BorderRadius = @brtl @brtr;
}

style border4 {
    BorderRadius = 5px;
}
style border5 {
    BorderRadiusTopLeft = 1px;
    BorderRadiusTopRight = 20vh;
    BorderRadiusBottomRight = 2em;
    BorderRadiusBottomLeft = 4px;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(5, styleGroup.Length);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[0].groups[0].normal.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[0].groups[0].normal.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[0].groups[0].normal.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(4, UIFixedUnit.Em), styleGroup[0].groups[0].normal.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[1].groups[0].normal.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[1].groups[0].normal.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[4].groups[0].normal.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(20, UIFixedUnit.ViewportHeight), styleGroup[4].groups[0].normal.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(2, UIFixedUnit.Em), styleGroup[4].groups[0].normal.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(4), styleGroup[4].groups[0].normal.BorderRadiusBottomLeft);
    }

    [Test]
    public void CompileTransformPosition() {
        var nodes = StyleParser.Parse(@"
export const x = 20sw;
export const y = 10cah;

style trans1 { TransformPosition = @x @y; }
style trans2 { TransformPosition = @x; }
style trans3 { TransformPositionX = @x; }
style trans4 { TransformPositionY = 15ah; }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(4, styleGroup.Length);

        Assert.AreEqual(new TransformOffset(20, TransformUnit.ScreenWidth), styleGroup[0].groups[0].normal.TransformPositionX);
        Assert.AreEqual(new TransformOffset(10, TransformUnit.ContentAreaHeight), styleGroup[0].groups[0].normal.TransformPositionY);

        Assert.AreEqual(new TransformOffset(20, TransformUnit.ScreenWidth), styleGroup[1].groups[0].normal.TransformPositionX);
        Assert.AreEqual(new TransformOffset(20, TransformUnit.ScreenWidth), styleGroup[1].groups[0].normal.TransformPositionY);

        Assert.AreEqual(new TransformOffset(20, TransformUnit.ScreenWidth), styleGroup[2].groups[0].normal.TransformPositionX);
        Assert.AreEqual(TransformOffset.Unset, styleGroup[2].groups[0].normal.TransformPositionY);

        Assert.AreEqual(TransformOffset.Unset, styleGroup[3].groups[0].normal.TransformPositionX);
        Assert.AreEqual(new TransformOffset(15, TransformUnit.AnchorHeight), styleGroup[3].groups[0].normal.TransformPositionY);
    }

    [Test]
    public void CompileTransformProperties() {
        var nodes = StyleParser.Parse(@"
export const x = 1;
export const y = 2;

style trans1 { TransformScale = @x @y; }
style trans2 { TransformScaleX = 3; }
style trans3 { TransformScaleY = 4; }

const pivot = 10%;

style pivot1 { TransformPivot = @pivot 10px; }
style pivot2 { TransformPivotX = @pivot; }
style pivot3 { TransformPivotY = 20px; }

style rotate1 { TransformRotation = 90; }

const pivotOffset = PivotOffset;

style transBeh1 { TransformBehavior = LayoutOffset AnchorMinOffset; }
style transBeh2 { TransformBehavior = PivotOffset; }
style transBeh3 { TransformBehaviorX = @pivotOffset; }
style transBeh4 { TransformBehaviorY = AnchorMaxOffset; }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup[0].groups[0].normal.TransformScaleX);
        Assert.AreEqual(2, styleGroup[0].groups[0].normal.TransformScaleY);

        Assert.AreEqual(3, styleGroup[1].groups[0].normal.TransformScaleX);
        Assert.AreEqual(0, styleGroup[1].groups[0].normal.TransformScaleY);

        Assert.AreEqual(0, styleGroup[2].groups[0].normal.TransformScaleX);
        Assert.AreEqual(4, styleGroup[2].groups[0].normal.TransformScaleY);

        Assert.AreEqual(new UIFixedLength(0.1f, UIFixedUnit.Percent), styleGroup[3].groups[0].normal.TransformPivotX);
        Assert.AreEqual(new UIFixedLength(10), styleGroup[3].groups[0].normal.TransformPivotY);

        Assert.AreEqual(new UIFixedLength(0.1f, UIFixedUnit.Percent), styleGroup[4].groups[0].normal.TransformPivotX);
        Assert.AreEqual(UIFixedLength.Unset, styleGroup[4].groups[0].normal.TransformPivotY);

        Assert.AreEqual(UIFixedLength.Unset, styleGroup[5].groups[0].normal.TransformPivotX);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[5].groups[0].normal.TransformPivotY);

        Assert.AreEqual(90, styleGroup[6].groups[0].normal.TransformRotation);

        Assert.AreEqual(TransformBehavior.LayoutOffset, styleGroup[7].groups[0].normal.TransformBehaviorX);
        Assert.AreEqual(TransformBehavior.AnchorMinOffset, styleGroup[7].groups[0].normal.TransformBehaviorY);

        Assert.AreEqual(TransformBehavior.PivotOffset, styleGroup[8].groups[0].normal.TransformBehaviorX);
        Assert.AreEqual(TransformBehavior.PivotOffset, styleGroup[8].groups[0].normal.TransformBehaviorY);

        Assert.AreEqual(TransformBehavior.PivotOffset, styleGroup[9].groups[0].normal.TransformBehaviorX);
        Assert.AreEqual(TransformBehavior.Unset, styleGroup[9].groups[0].normal.TransformBehaviorY);

        Assert.AreEqual(TransformBehavior.Unset, styleGroup[10].groups[0].normal.TransformBehaviorX);
        Assert.AreEqual(TransformBehavior.AnchorMaxOffset, styleGroup[10].groups[0].normal.TransformBehaviorY);
    }

    [Test]
    public void CompileSizes() {
        var nodes = StyleParser.Parse(@"
export const x = 1pca;
export const y = 2;

style size1 { 
    MinWidth = @x;
    MinHeight = 300px;
    PreferredWidth = 20px;
    PreferredHeight = 1000px;
    MaxWidth = 400px;
    MaxHeight = @y;
}
style size2 { 
    PreferredSize = 1000px 1111px;
    MinSize = 200px;
    MaxSize = 1500px 1200px;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleContainer = styleSheet.styleGroupContainers;
        Assert.AreEqual(new UIMeasurement(1, UIMeasurementUnit.ParentContentArea), styleContainer[0].groups[0].normal.MinWidth);
        Assert.AreEqual(new UIMeasurement(300), styleContainer[0].groups[0].normal.MinHeight);
        Assert.AreEqual(new UIMeasurement(20), styleContainer[0].groups[0].normal.PreferredWidth);
        Assert.AreEqual(new UIMeasurement(1000), styleContainer[0].groups[0].normal.PreferredHeight);
        Assert.AreEqual(new UIMeasurement(400), styleContainer[0].groups[0].normal.MaxWidth);
        Assert.AreEqual(new UIMeasurement(2), styleContainer[0].groups[0].normal.MaxHeight);

        Assert.AreEqual(new UIMeasurement(1000), styleContainer[1].groups[0].normal.PreferredWidth);
        Assert.AreEqual(new UIMeasurement(1111), styleContainer[1].groups[0].normal.PreferredHeight);
        Assert.AreEqual(new UIMeasurement(200), styleContainer[1].groups[0].normal.MinWidth);
        Assert.AreEqual(new UIMeasurement(200), styleContainer[1].groups[0].normal.MinHeight);
        Assert.AreEqual(new UIMeasurement(1500), styleContainer[1].groups[0].normal.MaxWidth);
        Assert.AreEqual(new UIMeasurement(1200), styleContainer[1].groups[0].normal.MaxHeight);
    }

    [Test]
    public void CompileAnchoring() {
        var nodes = StyleParser.Parse(@"
export const layout = Fixed;
export const anchorRight = 20%;

style anchoring { 
    LayoutType = @layout;
    LayoutBehavior = Ignored;
    AnchorTarget = Viewport;
    AnchorTop = 10px;
    AnchorRight = @anchorRight;
    AnchorBottom = 400px;
    AnchorLeft = 90vh;
    ZIndex = 3;
    RenderLayer = Screen;
    RenderLayerOffset = 22;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(LayoutType.Fixed, styleGroup[0].groups[0].normal.LayoutType);
        Assert.AreEqual(LayoutBehavior.Ignored, styleGroup[0].groups[0].normal.LayoutBehavior);
        Assert.AreEqual(AnchorTarget.Viewport, styleGroup[0].groups[0].normal.AnchorTarget);
        Assert.AreEqual(new UIFixedLength(10), styleGroup[0].groups[0].normal.AnchorTop);
        Assert.AreEqual(new UIFixedLength(0.2f, UIFixedUnit.Percent), styleGroup[0].groups[0].normal.AnchorRight);
        Assert.AreEqual(new UIFixedLength(400), styleGroup[0].groups[0].normal.AnchorBottom);
        Assert.AreEqual(new UIFixedLength(90, UIFixedUnit.ViewportHeight), styleGroup[0].groups[0].normal.AnchorLeft);
        Assert.AreEqual(3, styleGroup[0].groups[0].normal.ZIndex);
        Assert.AreEqual(RenderLayer.Screen, styleGroup[0].groups[0].normal.RenderLayer);
        Assert.AreEqual(22, styleGroup[0].groups[0].normal.RenderLayerOffset);
    }

    [Test]
    public void CompileText() {
        // note: because of possible spaces in paths we have to support string values for urls
        var nodes = StyleParser.Parse(@"
export const red = red;

style teXt { 
    TextColor = @red;
    TextFontAsset = url(""Gotham-Medium SDF"");
    TextFontStyle = bold italic superscript underline highlight smallcaps;
    TextFontSize = 14;
    TextAlignment = Center;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(Color.red, styleGroup[0].groups[0].normal.TextColor);
        Assert.AreEqual("Gotham-Medium SDF", styleGroup[0].groups[0].normal.TextFontAsset.name);
        Assert.AreEqual(FontStyle.Normal
                        | FontStyle.Bold
                        | FontStyle.Italic
                        | FontStyle.Highlight
                        | FontStyle.Superscript
                        | FontStyle.Underline
                        | FontStyle.Highlight
                        | FontStyle.SmallCaps, styleGroup[0].groups[0].normal.TextFontStyle);
        Assert.AreEqual(TextAlignment.Center, styleGroup[0].groups[0].normal.TextAlignment);
        Assert.AreEqual(new UIFixedLength(14), styleGroup[0].groups[0].normal.TextFontSize);
    }

    [Test]
    public void CompileImport() {
        // note: because of possible spaces in paths we have to support string values for urls
        var nodes = StyleParser.Parse(@"
import ""Tests/Styles/ImportFromMe.style"" as importedThings;

style xyz {
    BackgroundColor = @importedThings.colorRed;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(Color.red, styleGroup[0].groups[0].normal.BackgroundColor);
    }

    [Test]
    public void StyleSheetContainers() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
          export const red = red;

          style styleRoot {
               
              TextColor = @red;
              TextFontAsset = url(""Gotham-Medium SDF"");
              TextFontStyle = bold italic superscript underline highlight smallcaps;
              TextFontSize = 14;
              TextAlignment = Center;
              [attr:attr0] {
              
                BackgroundColor = #ff0000aa;

              }

              [hover] {
                TextFontSize = 18;
              }
          }

        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.styleGroupContainers.Length);
        Assert.AreEqual(2, styleSheet.styleGroupContainers[0].groups.Count);
    }

    [Test]
    public void CompileAnimation() {
        var nodes = StyleParser.Parse(@"
            animation anim1 {
                [keyframes] {
                    0% { 
                        BackgroundColor = red; 
                        BackgroundColor = red; 
                    }
                    50% {
                        TextFontSize = 11;
                        BackgroundColor = green; 
                    }
                    60% {
                        PreferredSize = 40px, 30px;
                    }
                    100% {
                         BackgroundColor = green; 
                    }
                }
            }
        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.animations.Length);
        AnimationData animationData = styleSheet.animations[0];
        Assert.AreEqual("anim1", animationData.name);
        Assert.AreEqual(4, animationData.frames.Count);
        AnimationKeyFrame frame0 = animationData.frames[0];
        Assert.AreEqual(1, frame0.properties.Count);
        Assert.AreEqual(0, frame0.key);
        Assert.AreEqual(StylePropertyId.BackgroundColor, frame0.properties[0].propertyId);
        Assert.AreEqual(Color.red, frame0.properties[0].styleProperty.AsColor);

        AnimationKeyFrame frame1 = animationData.frames[1];
        Assert.AreEqual(2, frame1.properties.Count);
        Assert.AreEqual(0.5f, frame1.key);
        Assert.AreEqual(StylePropertyId.TextFontSize, frame1.properties[0].propertyId);
        Assert.AreEqual(StylePropertyId.BackgroundColor, frame1.properties[1].propertyId);

        AnimationKeyFrame frame2 = animationData.frames[2];
        Assert.AreEqual(2, frame2.properties.Count);
        Assert.AreEqual(0.6f, frame2.key);
        Assert.AreEqual(StylePropertyId.PreferredWidth, frame2.properties[0].propertyId);
        Assert.AreEqual(StylePropertyId.PreferredHeight, frame2.properties[1].propertyId);

        AnimationKeyFrame frame3 = animationData.frames[3];
        Assert.AreEqual(1, frame3.properties.Count);
        Assert.AreEqual(1, frame3.key);
        Assert.AreEqual(StylePropertyId.BackgroundColor, frame3.properties[0].propertyId);
    }

    [Test]
    public void CompileAnimationOptions() {
        var nodes = StyleParser.Parse(@"
            animation anim1 {

                [options] {
                    delay = 1000;
                    duration = 3000;
                    timingFunction = SineEaseOut;
                }

                [keyframes] {
                    0% { 
                        BackgroundColor = red; 
                        BackgroundColor = red; 
                    }
                    50% {
                        TextFontSize = 11;
                        BackgroundColor = green; 
                    }
                    60% {
                        PreferredSize = 40px, 30px;
                    }
                    100% {
                         BackgroundColor = green; 
                    }
                }
            }
        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.animations.Length);
        AnimationData animationData = styleSheet.animations[0];
        Assert.AreEqual(1000, animationData.options.delay);
        Assert.AreEqual(3000, animationData.options.duration);
        Assert.AreEqual(EasingFunction.SineEaseOut, animationData.options.timingFunction);
    }

    [Test]
    public void CompileBackgroundImageFromSpriteAtlas() {
        var nodes = StyleParser.Parse(@"
            style fromatlas {
                BackgroundImage = url(""/some/image"", ""spriteName1""); 
            }
        ".Trim());
        
        
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        
        var styleContainer = styleSheet.styleGroupContainers;
        Assert.IsInstanceOf<Sprite>(styleContainer[0].groups[0].normal.BackgroundImage);
    }
}
