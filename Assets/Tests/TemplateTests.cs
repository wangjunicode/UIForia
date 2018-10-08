
using NUnit.Framework;
using Rendering;
using Src;
using Src.Systems;
using static Tests.TestUtils;

[TestFixture]
public class TemplateTests {

    public ParsedTemplate dummyTemplate;

    class TestTarget {

        public string stringValue;

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

}