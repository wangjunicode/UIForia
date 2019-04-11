using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UIForia.Templates;

[TestFixture]
public class ParsedTemplateTests {

    [Template("Templates/ParsedTemplateTests/TestType0.xml")]
    private class TestType0 : UIElement { }

    [Template("Templates/ParsedTemplateTests/TestType1.xml")]
    private class TestType1 : UIElement { }

    [Template("Templates/ParsedTemplateTests/TestType2.xml")]
    private class TestType2 : UIElement { }

    [Test]
    public void ParsedTemplateCompilesStyles() {
        MockApplication app = new MockApplication(typeof(TestType0));
        TemplateParser parser = new TemplateParser(app);
        ParsedTemplate template = parser.GetParsedTemplate(typeof(TestType0), true);
        template.Compile();
        Assert.AreEqual(3, template.sharedStyleMap.Count);
        Assert.AreEqual("style0", template.sharedStyleMap.GetOrDefault("style0").container.name);
        Assert.AreEqual("style1", template.sharedStyleMap.GetOrDefault("style1").container.name);
        Assert.AreEqual("style2", template.sharedStyleMap.GetOrDefault("style2").container.name);
    }

    [Test]
    public void ParsedTemplateCompilesMultipleAliasedStyles() {
        MockApplication app = new MockApplication(typeof(TestType1));
        TemplateParser parser = new TemplateParser(app);
        ParsedTemplate template = parser.GetParsedTemplate(typeof(TestType1), true);
        template.Compile();

        Assert.AreEqual(9, template.sharedStyleMap.Count);

        Assert.AreEqual(template.sharedStyleMap.GetOrDefault("t0.style0").container, template.sharedStyleMap.GetOrDefault("style0").container);
        Assert.AreEqual(template.sharedStyleMap.GetOrDefault("t0.style1").container, template.sharedStyleMap.GetOrDefault("style1").container);
        Assert.AreEqual(template.sharedStyleMap.GetOrDefault("t0.style2").container, template.sharedStyleMap.GetOrDefault("style2").container);

        Assert.AreEqual("style0", template.sharedStyleMap.GetOrDefault("t0.style0").container.name);
        Assert.AreEqual("style1", template.sharedStyleMap.GetOrDefault("t0.style1").container.name);
        Assert.AreEqual("style2", template.sharedStyleMap.GetOrDefault("t0.style2").container.name);

        Assert.AreEqual("style0", template.sharedStyleMap.GetOrDefault("t1.style0").container.name);
        Assert.AreEqual("style1", template.sharedStyleMap.GetOrDefault("t1.style1").container.name);
        Assert.AreEqual("style2", template.sharedStyleMap.GetOrDefault("t1.style2").container.name);
    }

    [Test]
    public void ParsedTemplateCompilesImplicitStyles() {
        MockApplication app = new MockApplication(typeof(TestType2));
        TemplateParser parser = new TemplateParser(app);
        ParsedTemplate template = parser.GetParsedTemplate(typeof(TestType2), true);
        template.Compile();

        Assert.AreEqual(3, template.implicitStyleMap.Count);

        Assert.AreEqual("Div", template.implicitStyleMap.GetOrDefault("Div").name);
        Assert.AreEqual("Group", template.implicitStyleMap.GetOrDefault("Group").name);
        Assert.AreEqual("Other", template.implicitStyleMap.GetOrDefault("Other").name);

        Assert.AreEqual(new UIFixedLength(1), template.implicitStyleMap.GetOrDefault("Div").groups[0].normal.TextFontSize);
        Assert.AreEqual(new UIFixedLength(2), template.implicitStyleMap.GetOrDefault("Group").groups[0].normal.TextFontSize);
        Assert.AreEqual(new UIFixedLength(3), template.implicitStyleMap.GetOrDefault("Other").groups[0].normal.TextFontSize);
    }

}