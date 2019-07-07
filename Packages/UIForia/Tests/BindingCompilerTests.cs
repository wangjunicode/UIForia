using NUnit.Framework;
using UIForia.Attributes;
using UIForia.Bindings;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;
using static Tests.TestUtils;

#pragma warning disable 0649

[TestFixture]
public class BindingCompilerTests {

    private class TestThing {
        
    }
    
    [Test]
    public void CreatesBinding_FieldSetter() {
        PropertyBindingCompiler compiler = new PropertyBindingCompiler();
        var list = new LightList<Binding>();
        compiler.CompileAttribute(
            typeof(TestUIElementType),
            typeof(TestUIElementType),
            new AttributeDefinition("intValue", "{1 + 1}"),
            list
        );
        Assert.IsNotNull(list[0]);
    }

    [Test]
    public void CreatesBinding_ActionEvent_1Args() {
        // <RootElement>
        //     <FakeElement onSomething="{HandleSomething($event)}"/>
        // </RootElement>

        PropertyBindingCompiler compiler = new PropertyBindingCompiler();
        FakeRootElement rootElement = new FakeRootElement();
        FakeElement childElement = new FakeElement();
        ExpressionContext ctx = new ExpressionContext(rootElement);

        AttributeDefinition attrDef = new AttributeDefinition("onEvt1", "evt1Handler");

        var list = new LightList<Binding>();

        compiler.CompileAttribute(typeof(FakeRootElement), typeof(FakeElement), attrDef, list);
        Binding binding = list[0];
        binding.Execute(childElement, ctx);
        Assert.IsNull(rootElement.arg1Params);

        childElement.InvokeEvtArg1("str");
        Assert.AreEqual(new [] {"str"}, rootElement.arg1Params);
    }
    
    [Test]
    public void CreatesBinding_FuncEvent_1Args() {
        // <RootElement>
        //     <FakeElement onSomething="{HandleSomething($event)}"/>
        // </RootElement>

        PropertyBindingCompiler compiler = new PropertyBindingCompiler();
        FakeRootElement rootElement = new FakeRootElement();
        FakeElement childElement = new FakeElement();
        ExpressionContext ctx = new ExpressionContext(rootElement);

        AttributeDefinition attrDef = new AttributeDefinition("onFuncEvt1", "evt1Handler_Func");

        var list = new LightList<Binding>();

        compiler.CompileAttribute(typeof(FakeRootElement), typeof(FakeElement), attrDef, list);
        Binding binding = list[0];
        binding.Execute(childElement, ctx);
        Assert.IsNull(rootElement.arg1Params);

        childElement.InvokeFuncEvtArg1("str");
        Assert.AreEqual(new [] {"str"}, rootElement.arg1Params);
    }
    
    [Test]
    public void CreatesBinding_FuncEvent_Prop_1Args() {
        // <RootElement>
        //     <FakeElement onSomething="{HandleSomething($event)}"/>
        // </RootElement>

        PropertyBindingCompiler compiler = new PropertyBindingCompiler();
        FakeRootElement rootElement = new FakeRootElement();
        FakeElement childElement = new FakeElement();
        ExpressionContext ctx = new ExpressionContext(rootElement);

        AttributeDefinition attrDef = new AttributeDefinition("onFuncEvt1", "evt1Handler_Func");

        var list = new LightList<Binding>();

        compiler.CompileAttribute(typeof(FakeRootElement), typeof(FakeElement), attrDef, list);
        Binding binding = list[0];
        binding.Execute(childElement, ctx);
        Assert.IsNull(rootElement.arg1Params);

        childElement.InvokeFuncEvtArg1("str");
        Assert.AreEqual(new [] {"str"}, rootElement.arg1Params);
    }
 
    private class TestedThing1 : UIElement {

        public string prop0;
        public bool didProp0Change;

        [OnPropertyChanged(nameof(prop0))]
        public void OnProp0Changed(string prop) {
            didProp0Change = true;
        }

    }


    [Test]
    public void OnPropertyChanged() {
        AttributeDefinition attrDef = new AttributeDefinition("prop0", "'some-string'");
        PropertyBindingCompiler c = new PropertyBindingCompiler();
        var list = new LightList<Binding>();
        c.CompileAttribute(typeof(TestedThing1), typeof(TestedThing1), attrDef, list);
        Binding b = list[0];
        Assert.IsInstanceOf<FieldSetterBinding_WithCallbacks<TestedThing1, string>>(b);
        TestedThing1 t = new TestedThing1();
        Assert.IsFalse(t.didProp0Change);
        Assert.AreEqual(t.prop0, null);
        b.Execute(t, new ExpressionContext(null));
        Assert.AreEqual(t.prop0, "some-string");
        Assert.IsTrue(t.didProp0Change);
        t.didProp0Change = false;
        b.Execute(t, new ExpressionContext(null));
        Assert.AreEqual(t.prop0, "some-string");
        Assert.IsFalse(t.didProp0Change);
    }

   
}