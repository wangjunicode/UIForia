using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Templates;
using UIForia.Util;
using static Tests.TestUtils;

[TestFixture]
public class TemplateTests {

    public ParsedTemplate dummyTemplate;

    class TestTarget {

        public string stringValue;

        public int UniqueId => 0;

    }

    [SetUp]
    public void Setup() {
        dummyTemplate = new ParsedTemplate(null, typeof(TestTarget), null, null, null, null, null, null);
    }

    [Test]
    public void TextElement_CompileSimpleConstantBinding() {
        UITextTemplate template = new UITextTemplate(null, "'hello'");
        template.Compile(dummyTemplate);
        template.CreateScoped(new TemplateScope());
        Assert.IsNotEmpty(template.triggeredBindings);
        Assert.AreEqual(1, template.triggeredBindings.Length);
        Assert.IsInstanceOf<TextBinding_Single>(template.triggeredBindings[0]);
    }

    [Test]
    public void TextElement_CompileMultipartConstantBinding() {
        UITextTemplate template = new UITextTemplate(null, "hello {'there'}");
        template.Compile(dummyTemplate);
        UIElement element = template.CreateScoped(new TemplateScope());
        Assert.IsNotEmpty(template.triggeredBindings);
        Assert.AreEqual(1, template.triggeredBindings.Length);
        Assert.IsInstanceOf<TextBinding_Multiple>(template.triggeredBindings[0]);
        template.triggeredBindings[0].Execute(element, new ExpressionContext(null));
        Assert.AreEqual("hello there", As<UITextElement>(element).GetText());
    }

    [Test]
    public void TextElement_CompileMultipartDynamicBinding() {
        TestTarget target = new TestTarget();
        ExpressionContext ctx = new ExpressionContext(target);

        target.stringValue = "world";
        UITextTemplate template = new UITextTemplate(null, "hello {stringValue}!");
        template.Compile(dummyTemplate);
        UIElement element = template.CreateScoped(new TemplateScope());
        Assert.IsNotEmpty(template.perFrameBindings);
        Assert.AreEqual(1, template.perFrameBindings.Length);
        Assert.IsInstanceOf<TextBinding_Multiple>(template.perFrameBindings[0]);
        template.perFrameBindings[0].Execute(element, ctx);
        Assert.AreEqual("hello world!", As<UITextElement>(element).GetText());
    }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Repeat list=""values"">
                <Text>The value is</Text>
                <Text>{$item}</Text>
            </Repeat>
        </Contents>
    </UITemplate>
    ")]
    private class MultipleRepeatThing : UIElement {

        public RepeatableList<int> values;

    }

    [Test]
    public void RepeatElementCanHaveMultipleChildren() {
        MockApplication app = new MockApplication(typeof(MultipleRepeatThing));
        MultipleRepeatThing target = app.GetView(0).RootElement.GetChild(0) as MultipleRepeatThing;

        target.values = new RepeatableList<int>(new[] {
            1, 2, 3
        });

        app.Update();
        UIRepeatElement<int> repeatElement = target.FindFirstByType<UIRepeatElement<int>>();
        Assert.NotNull(repeatElement);

        void AssertChildren(UIElement child, int i) {
            Assert.IsInstanceOf<RepeatMultiChildContainerElement>(child);
            Assert.AreEqual(2, child.ChildCount);
            Assert.IsInstanceOf<UITextElement>(child.GetChild(0));
            UITextElement text0 = child.GetChild(0) as UITextElement;
            UITextElement text1 = child.GetChild(1) as UITextElement;
            Assert.AreEqual("The value is", text0.text);
            Assert.AreEqual(i.ToString(), text1.text);
        }

        AssertChildren(repeatElement.GetChild(0), 1);
        AssertChildren(repeatElement.GetChild(1), 2);
        AssertChildren(repeatElement.GetChild(2), 3);
    }

}