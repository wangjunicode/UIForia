using NUnit.Framework;
using Src.Rendering;
using Src;
using Src.Systems;
using Tests.Mocks;
using static Tests.TestUtils;

[TestFixture]
public class TemplateTests {

    public ParsedTemplate dummyTemplate;

    class TestTarget : IExpressionContextProvider {

        public string stringValue;

        public int UniqueId => 0;
        public IExpressionContextProvider ExpressionParent => null;

    }

    [SetUp]
    public void Setup() {
        dummyTemplate = new ParsedTemplate(new UIElementTemplate(typeof(TestTarget), null));
    }

    [Test]
    public void TextElement_CompileSimpleConstantBinding() {
        UITextTemplate template = new UITextTemplate("'hello'");
        template.Compile(dummyTemplate);
        MetaData data = template.GetCreationData(new UITextElement(), new UITemplateContext(null));
        Assert.IsNotEmpty(data.constantBindings);
        Assert.AreEqual(1, data.constantBindings.Length);
        Assert.IsInstanceOf<TextBinding_Single>(data.constantBindings[0]);
    }

    [Test]
    public void TextElement_CompileMultipartConstantBinding() {
        UITextTemplate template = new UITextTemplate("'hello {'there'}'");
        template.Compile(dummyTemplate);
        MetaData data = template.GetCreationData(new UITextElement(), new UITemplateContext(null));
        Assert.IsNotEmpty(data.constantBindings);
        Assert.AreEqual(1, data.constantBindings.Length);
        Assert.IsInstanceOf<TextBinding_Multiple>(data.constantBindings[0]);
        data.element.style = new UIStyleSet(data.element, new StyleSystem());
        data.constantBindings[0].Execute(data.element, data.context);
        Assert.AreEqual("hello there", As<UITextElement>(data.element).GetText());
    }

    [Test]
    public void TextElement_CompileMultipartDynamicBinding() {
        TestTarget target = new TestTarget();
        UITemplateContext ctx = new UITemplateContext(null);
        ctx.rootContext = target;

        target.stringValue = "world";
        UITextTemplate template = new UITextTemplate("'hello {stringValue}!'");
        template.Compile(dummyTemplate);
        MetaData data = template.GetCreationData(new UITextElement(), ctx);
        data.element.style = new UIStyleSet(data.element, new StyleSystem());
        Assert.IsNotEmpty(data.bindings);
        Assert.AreEqual(1, data.bindings.Length);
        Assert.IsInstanceOf<TextBinding_Multiple>(data.bindings[0]);
        data.bindings[0].Execute(data.element, data.context);
        Assert.AreEqual("hello world!", As<UITextElement>(data.element).GetText());
    }

    [Test]
    public void TextElement_EventOnChange() {
        TestTarget target = new TestTarget();
        UITemplateContext ctx = new UITemplateContext(null);
        ctx.rootContext = target;

        target.stringValue = "world";
        UITextTemplate template = new UITextTemplate("'hello {stringValue}!'");
        template.Compile(dummyTemplate);
        MetaData data = template.GetCreationData(new UITextElement(), ctx);
        data.element.style = new UIStyleSet(data.element, new StyleSystem());
        int callCount = 0;
        As<UITextElement>(data.element).onTextChanged += (element, text) => callCount++;
        data.bindings[0].Execute(data.element, data.context);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void TextElement_NoEventWithoutChange() {
        TestTarget target = new TestTarget();
        UITemplateContext ctx = new UITemplateContext(null);
        ctx.rootContext = target;

        target.stringValue = "world";
        UITextTemplate template = new UITextTemplate("'hello {stringValue}!'");
        template.Compile(dummyTemplate);
        MetaData data = template.GetCreationData(new UITextElement(), ctx);
        data.element.style = new UIStyleSet(data.element, new StyleSystem());
        int callCount = 0;
        As<UITextElement>(data.element).onTextChanged += (element, text) => callCount++;
        data.bindings[0].Execute(data.element, data.context);
        data.bindings[0].Execute(data.element, data.context);
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