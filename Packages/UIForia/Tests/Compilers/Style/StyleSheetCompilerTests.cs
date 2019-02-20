using NUnit.Framework;
using UIForia;
using UIForia.Compilers.Style;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class StyleSheetCompilerTests {

    [Test]
    public void CompileEmptyStyle() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        Assert.IsNotNull(styleSheet);
        Assert.AreEqual(0, styleSheet.styleGroups.Count);
    }

    [Test]
    public void CreateStyleGroupWithReferencedValue() {
        var nodes = StyleParser2.Parse(@"
            
const alpha = 255;
const redChannel = 255.000;
export const color0 = rgba(@redChannel, 0, 0, @alpha);
            
style myStyle {
    BackgroundColor = @color0;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(Color.red, styleGroup[0].normal.BackgroundColor);
    }

    [Test]
    public void CreateAttributeGroupsWithMeasurements() {
        var nodes = StyleParser2.Parse(@"
            
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

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(2, styleGroup.Count);

        Assert.AreEqual(10, styleGroup[0].normal.MarginTop.value);
        Assert.AreEqual(UIMeasurementUnit.Pixel, styleGroup[0].normal.MarginTop.unit);
        Assert.AreEqual(20, styleGroup[0].hover.MarginLeft.value);
        Assert.AreEqual(20, styleGroup[1].normal.MarginTop.value);
    }

    [Test]
    public void UseMarginPropertyShorthand() {
        var nodes = StyleParser2.Parse(@"
            
export const m1 = 10pca;
export const m2 = @m3;
export const m3 = 10pca;
export const m4 = @m2;
            
style myStyle {
    Margin = @m1 @m2 10px @m4;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(10, styleGroup[0].normal.MarginTop.value);
        Assert.AreEqual(10, styleGroup[0].normal.MarginRight.value);
        Assert.AreEqual(10, styleGroup[0].normal.MarginBottom.value);
        Assert.AreEqual(10, styleGroup[0].normal.MarginLeft.value);
        Assert.AreEqual(UIMeasurementUnit.ParentContentArea, styleGroup[0].normal.MarginTop.unit);
        Assert.AreEqual(UIMeasurementUnit.ParentContentArea, styleGroup[0].normal.MarginRight.unit);
        Assert.AreEqual(UIMeasurementUnit.Pixel, styleGroup[0].normal.MarginBottom.unit);
        Assert.AreEqual(UIMeasurementUnit.ParentContentArea, styleGroup[0].normal.MarginLeft.unit);
    }

    [Test]
    public void UsePaddingPropertyShorthand() {
        var nodes = StyleParser2.Parse(@"

export const p1 = 10%;
export const p2 = @p3;
export const p3 = 10%;
export const p4 = @p2;

style myStyle {
    Padding = @p1 @p2 20px @p4;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(10, styleGroup[0].normal.PaddingTop.value);
        Assert.AreEqual(10, styleGroup[0].normal.PaddingRight.value);
        Assert.AreEqual(20, styleGroup[0].normal.PaddingBottom.value);
        Assert.AreEqual(10, styleGroup[0].normal.PaddingLeft.value);
        Assert.AreEqual(UIFixedUnit.Percent, styleGroup[0].normal.PaddingTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, styleGroup[0].normal.PaddingRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, styleGroup[0].normal.PaddingBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, styleGroup[0].normal.PaddingLeft.unit);
    }

    [Test]
    public void UseBorderPropertyShorthand() {
        var nodes = StyleParser2.Parse(@"

export const b1 = 10%;
export const b2 = @b3;
export const b3 = 10%;
export const b4 = @b2;

style myStyle {
    Border = @b1 @b2 20px @b4;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(10, styleGroup[0].normal.BorderTop.value);
        Assert.AreEqual(10, styleGroup[0].normal.BorderRight.value);
        Assert.AreEqual(20, styleGroup[0].normal.BorderBottom.value);
        Assert.AreEqual(10, styleGroup[0].normal.BorderLeft.value);
        Assert.AreEqual(UIFixedUnit.Percent, styleGroup[0].normal.BorderTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, styleGroup[0].normal.BorderRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, styleGroup[0].normal.BorderBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, styleGroup[0].normal.BorderLeft.unit);
    }

    [Test]
    public void CompileVisibilty() {
        var nodes = StyleParser2.Parse(@"

const v = hidden;

style myStyle {
    Visibility = visible;
    [attr:disabled=""disabled""] {
        Visibility = @v;
    }
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(2, styleGroup.Count);

        Assert.AreEqual(Visibility.Visible, styleGroup[0].normal.Visibility);
        Assert.AreEqual(Visibility.Hidden, styleGroup[1].normal.Visibility);
        Assert.AreEqual("disabled", styleGroup[1].rule.attributeName);
        Assert.AreEqual("disabled", styleGroup[1].rule.attributeValue);
        Assert.AreEqual(false, styleGroup[1].rule.invert);
    }

    [Test]
    public void CompileGridItemColAndRowProperties() {
        var nodes = StyleParser2.Parse(@"

const rowStart = 2;

style myStyle {
    GridItemColStart = 0;
    GridItemColSpan = 4;
    GridItemRowStart = @rowStart;
    GridItemRowSpan = 5;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(0, styleGroup[0].normal.GridItemColStart);
        Assert.AreEqual(4, styleGroup[0].normal.GridItemColSpan);
        Assert.AreEqual(2, styleGroup[0].normal.GridItemRowStart);
        Assert.AreEqual(5, styleGroup[0].normal.GridItemRowSpan);
    }

    [Test]
    public void CompileGridAxisAlignmentProperties() {
        var nodes = StyleParser2.Parse(@"

const colSelfAlignment = Center;

style myStyle {
    GridItemColSelfAlignment = @colSelfAlignment;
    GridItemRowSelfAlignment = End;
    GridLayoutColAlignment = Shrink;
    GridLayoutRowAlignment = fit;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(GridAxisAlignment.Center, styleGroup[0].normal.GridItemColSelfAlignment);
        Assert.AreEqual(GridAxisAlignment.End, styleGroup[0].normal.GridItemRowSelfAlignment);
        Assert.AreEqual(GridAxisAlignment.Shrink, styleGroup[0].normal.GridLayoutColAlignment);
        Assert.AreEqual(GridAxisAlignment.Fit, styleGroup[0].normal.GridLayoutRowAlignment);
    }

    [Test]
    public void CompileGridLayoutDensity() {
        var nodes = StyleParser2.Parse(@"

const density = dense;

style myStyle {
    GridLayoutDensity = @density;
    [hover] { GridLayoutDensity = sparse; }
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(GridLayoutDensity.Dense, styleGroup[0].normal.GridLayoutDensity);
        Assert.AreEqual(GridLayoutDensity.Sparse, styleGroup[0].hover.GridLayoutDensity);
    }

    [Test]
    public void CompileGridLayoutDirection() {
        var nodes = StyleParser2.Parse(@"

const dir = Horizontal;

style myStyle {
    GridLayoutDirection = @dir;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);
        Assert.AreEqual(LayoutDirection.Horizontal, styleGroup[0].normal.GridLayoutDirection);
    }

    [Test]
    public void CompileFlexLayoutDirection() {
        var nodes = StyleParser2.Parse(@"

const dir = Vertical;

style myStyle {
    FlexLayoutDirection = @dir;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);
        Assert.AreEqual(LayoutDirection.Vertical, styleGroup[0].normal.FlexLayoutDirection);
    }

    [Test]
    public void CompileGridLayoutColTemplate() {
        var nodes = StyleParser2.Parse(@"

const colOne = 1mx;

style myStyle {
    GridLayoutColTemplate = @colOne 1mx 2fr 480px;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(4, styleGroup[0].normal.GridLayoutColTemplate.Count);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].normal.GridLayoutColTemplate[0]);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].normal.GridLayoutColTemplate[1]);
        Assert.AreEqual(new GridTrackSize(2, GridTemplateUnit.FractionalRemaining), styleGroup[0].normal.GridLayoutColTemplate[2]);
        Assert.AreEqual(new GridTrackSize(480, GridTemplateUnit.Pixel), styleGroup[0].normal.GridLayoutColTemplate[3]);
    }

    [Test]
    public void CompileGridLayoutRowTemplate() {
        var nodes = StyleParser2.Parse(@"

const colOne = 1mx;

style myStyle {
    GridLayoutRowTemplate = @colOne 1mx 2fr 480px;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(4, styleGroup[0].normal.GridLayoutRowTemplate.Count);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].normal.GridLayoutRowTemplate[0]);
        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].normal.GridLayoutRowTemplate[1]);
        Assert.AreEqual(new GridTrackSize(2, GridTemplateUnit.FractionalRemaining), styleGroup[0].normal.GridLayoutRowTemplate[2]);
        Assert.AreEqual(new GridTrackSize(480, GridTemplateUnit.Pixel), styleGroup[0].normal.GridLayoutRowTemplate[3]);
    }

}
