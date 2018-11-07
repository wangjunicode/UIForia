using System.Collections.Generic;
using NUnit.Framework;
using UIForia.Rendering;
using UIForia;
using UIForia.Parsing.StyleParser;
using UnityEngine;

[TestFixture]
public class StyleParserTests {

    [Test]
    public void ParseBasicStyleProperty() {
        const string input = @"
            style style1 {
                BackgroundColor = red;
            }
        ";

        ParsedStyleSheet sheet = StyleParser.ParseFromString(input);
        Assert.AreEqual(1, sheet.styles.Length);
        Assert.AreEqual(sheet.GetStyleGroup("style1").normal.BackgroundColor, Color.red);
    }
    
    [Test]
    public void ParseHoverStyleProperty() {
        const string input = @"
            style style1 {
                BackgroundColor = red;
                [hover] {
                    BackgroundColor = blue;
                }
            }
        ";

        ParsedStyleSheet sheet = StyleParser.ParseFromString(input);
        Assert.AreEqual(1, sheet.styles.Length);
        Assert.AreEqual(sheet.GetStyleGroup("style1").normal.BackgroundColor, Color.red);
        Assert.AreEqual(sheet.GetStyleGroup("style1").hover.BackgroundColor, Color.blue);
    }

    [Test]
    public void ParseLocalColorVariable() {
        const string input = @"
            var thing : color = blue;
            style style1 {
                BackgroundColor = @thing;
            }
        ";

        ParsedStyleSheet sheet = StyleParser.ParseFromString(input);
        Assert.AreEqual(1, sheet.styles.Length);
        Assert.AreEqual(sheet.GetStyleGroup("style1").normal.BackgroundColor, Color.blue);
    }

    [Test]
    public void ParseFloat() {
        List<StyleVariable> vars = new List<StyleVariable>();
        Assert.AreEqual(1.5f, ParseUtil.ParseFloat(vars, "1.5"));
        Assert.AreEqual(1.5f, ParseUtil.ParseFloat(vars, "1.5f"));
        Assert.AreEqual(-1.5f, ParseUtil.ParseFloat(vars, "-1.5f"));
        Assert.AreEqual(.5f, ParseUtil.ParseFloat(vars, ".5f"));
        Assert.AreEqual(-.5f, ParseUtil.ParseFloat(vars, "-.5f"));
        vars.Add(new StyleVariable() {
            name = "@thing",
            type = typeof(float),
            value = 10.8f
        });
        Assert.AreEqual(10.8f, ParseUtil.ParseFloat(vars, "@thing"));
    }

    [Test]
    public void ParseInt() {
        List<StyleVariable> vars = new List<StyleVariable>();
        Assert.AreEqual(1, ParseUtil.ParseInt(vars, "1"));
        Assert.AreEqual(-1, ParseUtil.ParseInt(vars, "-1"));
        vars.Add(new StyleVariable() {
            name = "@thing",
            type = typeof(int),
            value = 10
        });
        Assert.AreEqual(10, ParseUtil.ParseInt(vars, "@thing"));
    }

    [Test]
    public void ParseFixedLength() {
        List<StyleVariable> vars = new List<StyleVariable>();
        Assert.AreEqual(new UIFixedLength(100, UIFixedUnit.Pixel), ParseUtil.ParseFixedLength(vars, "100"));
        Assert.AreEqual(new UIFixedLength(-100, UIFixedUnit.Pixel), ParseUtil.ParseFixedLength(vars, "-100"));
        Assert.AreEqual(new UIFixedLength(100, UIFixedUnit.Pixel), ParseUtil.ParseFixedLength(vars, "100px"));
        Assert.AreEqual(new UIFixedLength(100, UIFixedUnit.Pixel), ParseUtil.ParseFixedLength(vars, "100 px"));
        Assert.AreEqual(new UIFixedLength(-100, UIFixedUnit.Pixel), ParseUtil.ParseFixedLength(vars, "-100px"));
        Assert.AreEqual(new UIFixedLength(-100, UIFixedUnit.Pixel), ParseUtil.ParseFixedLength(vars, "-100 px"));

        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.Percent), ParseUtil.ParseFixedLength(vars, "80%"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.Percent), ParseUtil.ParseFixedLength(vars, "-80%"));
        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.Percent), ParseUtil.ParseFixedLength(vars, "80 %"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.Percent), ParseUtil.ParseFixedLength(vars, "-80 %"));

        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.ViewportHeight), ParseUtil.ParseFixedLength(vars, "0.8vh"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.ViewportHeight), ParseUtil.ParseFixedLength(vars, "-0.8vh"));
        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.ViewportHeight), ParseUtil.ParseFixedLength(vars, "0.8 vh"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.ViewportHeight), ParseUtil.ParseFixedLength(vars, "-0.8 vh"));

        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.ViewportWidth), ParseUtil.ParseFixedLength(vars, "0.8vw"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.ViewportWidth), ParseUtil.ParseFixedLength(vars, "-0.8vw"));
        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.ViewportWidth), ParseUtil.ParseFixedLength(vars, "0.8 vw"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.ViewportWidth), ParseUtil.ParseFixedLength(vars, "-0.8 vw"));

        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.Em), ParseUtil.ParseFixedLength(vars, "0.8em"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.Em), ParseUtil.ParseFixedLength(vars, "-0.8em"));
        Assert.AreEqual(new UIFixedLength(0.8f, UIFixedUnit.Em), ParseUtil.ParseFixedLength(vars, "0.8 em"));
        Assert.AreEqual(new UIFixedLength(-0.8f, UIFixedUnit.Em), ParseUtil.ParseFixedLength(vars, "-0.8 em"));

        vars.Add(new StyleVariable() {
            name = "@thing",
            type = typeof(UIFixedLength),
            value = new UIFixedLength(0.2f, UIFixedUnit.Percent)
        });

        Assert.AreEqual(new UIFixedLength(0.2f, UIFixedUnit.Percent), ParseUtil.ParseFixedLength(vars, "@thing"));
    }

    [Test]
    public void ParseMeasurement() {
        List<StyleVariable> vars = new List<StyleVariable>();
        Assert.AreEqual(new UIMeasurement(100, UIMeasurementUnit.Pixel), ParseUtil.ParseMeasurement(vars, "100"));
        Assert.AreEqual(new UIMeasurement(-100, UIMeasurementUnit.Pixel), ParseUtil.ParseMeasurement(vars, "-100"));
        Assert.AreEqual(new UIMeasurement(100, UIMeasurementUnit.Pixel), ParseUtil.ParseMeasurement(vars, "100px"));
        Assert.AreEqual(new UIMeasurement(100, UIMeasurementUnit.Pixel), ParseUtil.ParseMeasurement(vars, "100 px"));
        Assert.AreEqual(new UIMeasurement(-100, UIMeasurementUnit.Pixel), ParseUtil.ParseMeasurement(vars, "-100px"));
        Assert.AreEqual(new UIMeasurement(-100, UIMeasurementUnit.Pixel), ParseUtil.ParseMeasurement(vars, "-100 px"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ParentSize), ParseUtil.ParseMeasurement(vars, "0.8psz"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ParentSize), ParseUtil.ParseMeasurement(vars, "-0.8psz"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ParentSize), ParseUtil.ParseMeasurement(vars, "0.8 psz"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ParentSize), ParseUtil.ParseMeasurement(vars, "-0.8 psz"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ParentContentArea), ParseUtil.ParseMeasurement(vars, "0.8pca"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ParentContentArea), ParseUtil.ParseMeasurement(vars, "-0.8pca"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ParentContentArea), ParseUtil.ParseMeasurement(vars, "0.8 pca"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ParentContentArea), ParseUtil.ParseMeasurement(vars, "-0.8 pca"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ViewportHeight), ParseUtil.ParseMeasurement(vars, "0.8vh"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ViewportHeight), ParseUtil.ParseMeasurement(vars, "-0.8vh"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ViewportHeight), ParseUtil.ParseMeasurement(vars, "0.8 vh"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ViewportHeight), ParseUtil.ParseMeasurement(vars, "-0.8 vh"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ViewportWidth), ParseUtil.ParseMeasurement(vars, "0.8vw"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ViewportWidth), ParseUtil.ParseMeasurement(vars, "-0.8vw"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.ViewportWidth), ParseUtil.ParseMeasurement(vars, "0.8 vw"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.ViewportWidth), ParseUtil.ParseMeasurement(vars, "-0.8 vw"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.Content), ParseUtil.ParseMeasurement(vars, "0.8cnt"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.Content), ParseUtil.ParseMeasurement(vars, "-0.8cnt"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.Content), ParseUtil.ParseMeasurement(vars, "0.8 cnt"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.Content), ParseUtil.ParseMeasurement(vars, "-0.8 cnt"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.AnchorWidth), ParseUtil.ParseMeasurement(vars, "0.8aw"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.AnchorWidth), ParseUtil.ParseMeasurement(vars, "-0.8aw"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.AnchorWidth), ParseUtil.ParseMeasurement(vars, "0.8 aw"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.AnchorWidth), ParseUtil.ParseMeasurement(vars, "-0.8 aw"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.AnchorHeight), ParseUtil.ParseMeasurement(vars, "0.8ah"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.AnchorHeight), ParseUtil.ParseMeasurement(vars, "-0.8ah"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.AnchorHeight), ParseUtil.ParseMeasurement(vars, "0.8 ah"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.AnchorHeight), ParseUtil.ParseMeasurement(vars, "-0.8 ah"));

        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.Em), ParseUtil.ParseMeasurement(vars, "0.8em"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.Em), ParseUtil.ParseMeasurement(vars, "-0.8em"));
        Assert.AreEqual(new UIMeasurement(0.8f, UIMeasurementUnit.Em), ParseUtil.ParseMeasurement(vars, "0.8 em"));
        Assert.AreEqual(new UIMeasurement(-0.8f, UIMeasurementUnit.Em), ParseUtil.ParseMeasurement(vars, "-0.8 em"));
    }

    [Test]
    public void ParseColor() {
        List<StyleVariable> vars = new List<StyleVariable>();
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "red"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "#ff0000"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "#f00"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "#ff0000ff"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "rgb(255,0,0)"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "rgb (  255 ,  0  ,  0 )"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "rgba(255,0,0,255)"));
        Assert.AreEqual(Color.red, ParseUtil.ParseColor(vars, "rgba (255,  0, 0,       255)"));
    }

    [Test]
    public void ParseFixedLengthRect() {
        List<StyleVariable> vars = new List<StyleVariable>();
        FixedLengthRect rect = ParseUtil.ParseFixedLengthRect(vars, "10px, 20%, 12em, 7vh");
        Assert.AreEqual(new UIFixedLength(10, UIFixedUnit.Pixel), rect.top);
        Assert.AreEqual(new UIFixedLength(0.2f, UIFixedUnit.Percent), rect.right);
        Assert.AreEqual(new UIFixedLength(12, UIFixedUnit.Em), rect.bottom);
        Assert.AreEqual(new UIFixedLength(7, UIFixedUnit.ViewportHeight), rect.left);
    }
}