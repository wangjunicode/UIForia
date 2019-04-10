using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

#pragma warning disable 0649

[TestFixture]
public class PropertyWriterTests {

    public class ThingThatWrites : UIContainerElement {

        public string value;

    }

    [Template(TemplateType.String, @"

        <UITemplate>
            <Contents>
                <ThingThatWrites value.write='writeTarget'/>
            </Contents>
        </UITemplate>

    ")]
    private class ThingWrittenTo : UIElement {

        public string writeTarget = "initial";

    }
    
    [Template(TemplateType.String, @"

        <UITemplate>
            <Contents>
                <ThingThatWrites value.write='WriteTarget'/>
            </Contents>
        </UITemplate>

    ")]
    private class ThingWrittenTo_Property : UIElement {

        public string WriteTarget { get; set; }

    }

    private class NestedWriteTarget {

        public string writeToMe;
        public string WriteToMe { get; set; }

    }

    [Template(TemplateType.String, @"

        <UITemplate>
            <Contents>
                <ThingThatWrites value.write='writeTarget.writeToMe'/>
            </Contents>
        </UITemplate>

    ")]
    private class ThingWrittenToNested : UIElement {

        public NestedWriteTarget writeTarget = new NestedWriteTarget();

    }
    
    [Template(TemplateType.String, @"

        <UITemplate>
            <Contents>
                <ThingThatWrites value.write='writeTarget.WriteToMe'/>
            </Contents>
        </UITemplate>

    ")]
    private class ThingWrittenTo_NestedProperty : UIElement {

        public NestedWriteTarget writeTarget = new NestedWriteTarget();

    }

    [Test]
    public void CompilesWriterAttrModifier_Field() {
        MockApplication app = new MockApplication(typeof(ThingWrittenTo));
        ThingWrittenTo root = (ThingWrittenTo) app.RootElement;
        ThingThatWrites writer = root.FindFirstByType<ThingThatWrites>();
        writer.value = "this is a value";

        app.Update();

        Assert.AreEqual(writer.value, "this is a value");
        Assert.AreEqual(writer.value, root.writeTarget);
    }

    [Test]
    public void CompilesWriterAttrModifierNested_Field() {
        MockApplication app = new MockApplication(typeof(ThingWrittenToNested));
        ThingWrittenToNested root = (ThingWrittenToNested) app.RootElement;
        ThingThatWrites writer = root.FindFirstByType<ThingThatWrites>();
        
        writer.value = "this is a value";
        app.Update();

        Assert.AreEqual(writer.value, "this is a value");
        Assert.AreEqual(writer.value, root.writeTarget.writeToMe);
    }
    
    [Test]
    public void CompilesWriterAttrModifier_Property() {
        MockApplication app = new MockApplication(typeof(ThingWrittenTo_Property));
        ThingWrittenTo_Property root = (ThingWrittenTo_Property) app.RootElement;
        ThingThatWrites writer = root.FindFirstByType<ThingThatWrites>();
        writer.value = "this is a value";

        app.Update();

        Assert.AreEqual(writer.value, "this is a value");
        Assert.AreEqual(writer.value, root.WriteTarget);
    }
    
    [Test]
    public void CompilesWriterAttrModifierNested_Property() {
        MockApplication app = new MockApplication(typeof(ThingWrittenTo_NestedProperty));
        ThingWrittenTo_NestedProperty root = (ThingWrittenTo_NestedProperty) app.RootElement;
        ThingThatWrites writer = root.FindFirstByType<ThingThatWrites>();
        writer.value = "this is a value";

        app.Update();

        Assert.AreEqual(writer.value, "this is a value");
        Assert.AreEqual(writer.value, root.writeTarget.WriteToMe);
    }

}