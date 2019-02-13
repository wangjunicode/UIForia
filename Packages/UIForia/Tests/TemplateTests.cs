using NUnit.Framework;
using UIForia;
using static Tests.TestUtils;

[TestFixture]
public class TemplateTests {

    public ParsedTemplate dummyTemplate;

    class TestTarget  {

        public string stringValue;

        public int UniqueId => 0;

    }

    [SetUp]
    public void Setup() {
        dummyTemplate = new ParsedTemplate(typeof(TestTarget), null, null, null, null, null);
    }

    [Test]
    public void TextElement_CompileSimpleConstantBinding() {
        UITextTemplate template = new UITextTemplate("'hello'");
        template.Compile(dummyTemplate);
        template.CreateScoped(new TemplateScope());
        Assert.IsNotEmpty(template.triggeredBindings);
        Assert.AreEqual(1, template.triggeredBindings.Length);
        Assert.IsInstanceOf<TextBinding_Single>(template.triggeredBindings[0]);
    }

    [Test]
    public void TextElement_CompileMultipartConstantBinding() {
        UITextTemplate template = new UITextTemplate("'hello {'there'}'");
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
        UITextTemplate template = new UITextTemplate("'hello {stringValue}!'");
        template.Compile(dummyTemplate);
        UIElement element = template.CreateScoped(new TemplateScope());
//        MetaData data = template.GetCreationData(new UITextElement(), ctx);
        Assert.IsNotEmpty(template.perFrameBindings);
        Assert.AreEqual(1, template.perFrameBindings.Length);
        Assert.IsInstanceOf<TextBinding_Multiple>(template.perFrameBindings[0]);
        template.perFrameBindings[0].Execute(element, ctx);
        Assert.AreEqual("hello world!", As<UITextElement>(element).GetText());
    }

    [Test]
    public void TextElement_EventOnChange() {
        TestTarget target = new TestTarget();
        ExpressionContext ctx = new ExpressionContext(target);
        target.stringValue = "world";
        UITextTemplate template = new UITextTemplate("'hello {stringValue}!'");
        template.Compile(dummyTemplate);
        UIElement el = template.CreateScoped(new TemplateScope());
        int callCount = 0;
        As<UITextElement>(el).onTextChanged += (element, text) => callCount++;
        template.perFrameBindings[0].Execute(el, ctx);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void TextElement_NoEventWithoutChange() {
        TestTarget target = new TestTarget();
        ExpressionContext ctx = new ExpressionContext(target);

        target.stringValue = "world";
        UITextTemplate template = new UITextTemplate("'hello {stringValue}!'");
        template.Compile(dummyTemplate);
        UIElement el = template.CreateScoped(new TemplateScope());
        int callCount = 0;
        As<UITextElement>(el).onTextChanged += (element, text) => callCount++;
        template.perFrameBindings[0].Execute(el, ctx);
        template.perFrameBindings[0].Execute(el, ctx);
        Assert.AreEqual(1, callCount);
    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Style>
                style x {}
                style y {}
            </Style>
            <Contents>
              <Div style='x' x-id='child0'/>
              <Div x-id='child1'>
                <Div style='y' x-id='child2'/>
              </Div>
              <ThingBlerg2 x-id='child3'/>
            </Contents>
        </UITemplate>
    ")]
    public class Thing : UIElement {

    }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Div x-id='child4'/>
            <Div x-id='child5'>
                <Div x-id='child6'/>
            </Div>
        </Contents>
    </UITemplate>
    ")]
    public class ThingBlerg2 : UIElement { }

//    [Test]
//    public void General_AssignTemplateRef() {
//        MockView view = new MockView(typeof(Thing));
//        view.Initialize();
//        ParsedTemplate template = view.RootElement.GetTemplate();
//        Assert.IsNotNull(template);
//        Assert.AreEqual(template, view.RootElement.FindById("child0").GetTemplate());
//        Assert.AreEqual(template, view.RootElement.FindById("child1").GetTemplate());
//        Assert.AreEqual(template, view.RootElement.FindById("child2").GetTemplate());
//    }
//
//    [Test]
//    public void General_ScopedTemplateRef() {
//        MockView view = new MockView(typeof(Thing));
//        view.Initialize();
//        ParsedTemplate template = view.RootElement.GetTemplate();
//        Assert.IsNotNull(template);
//        Assert.AreEqual(template, view.RootElement.FindById("child0").GetTemplate());
//        Assert.AreEqual(template, view.RootElement.FindById("child1").GetTemplate());
//        Assert.AreEqual(template, view.RootElement.FindById("child2").GetTemplate());
//        Assert.AreEqual(template, view.RootElement.FindById("child3").GetTemplate());
//        UIElement c3 = view.RootElement.FindById("child3");
//        UIElement c4 = c3.FindById("child4");
//        Assert.IsNotNull(c4.GetTemplate());
//        Assert.AreNotEqual(template, c4.GetTemplate());
//    }
//
//    [Test]
//    public void General_GetStyleFromTemplate() {
//        ParsedTemplate.Reset();
//        MockView view = new MockView(typeof(Thing));
//        view.Initialize();
//        UITemplate t = view.RootElement.FindById("child0").GetTemplateData();
//        UITemplate t2 = view.RootElement.FindById("child2").GetTemplateData();
//        Assert.AreEqual(t.baseStyles[0].name, "x");
//        Assert.AreEqual(t2.baseStyles[0].name, "y");
//    }

}