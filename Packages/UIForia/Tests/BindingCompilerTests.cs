using System;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UIForia.Attributes;
using UIForia.Bindings;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;
using static Tests.TestUtils;

#pragma warning disable 0649

[TestFixture]
public class BindingCompilerTests {

    private class TestThing {
        
    }
    
    
    [StructLayout(LayoutKind.Explicit)]
    public struct Union {

        [FieldOffset(0)] public float asFloat;
        [FieldOffset(0)] public uint asInt;

    }

    [Test]
    public void BitTest() {
        uint color = 0xff808080;
        // double asFloat = 0b11111111100000001000000001000000;
        // bytes = BitConverter.GetBytes(asFloat);
        // ToBits(bytes, new char[64]);
        //
        // float asFloatf = (float) asFloat;
        // bytes = BitConverter.GetBytes(asFloatf);
        // ToBits(bytes, result);
        //
        // uint uInt32 = BitConverter.ToUInt32(bytes, 0);
        // Debug.Log(uInt32);
        // Debug.Log(uInt32 & 0xFF);
        // Debug.Log((uInt32 >> 8) & 0xFF);
        // Debug.Log((uInt32 >> 16) & 0xFF);
        // Debug.Log((uInt32 >> 24) & 0xFF);

        Union u = default;
        u.asFloat = 0;
        u.asInt = color;
        float asFloat = u.asFloat;
        ToBits(BitConverter.GetBytes(u.asInt));
        ToBits(BitConverter.GetBytes(u.asFloat));
        ToBits(BitConverter.GetBytes(u.asInt));
        u.asFloat = asFloat;
        ToBits(BitConverter.GetBytes(asFloat));
        uint uInt32 = u.asInt;
        Debug.Log(uInt32);
        Debug.Log(uInt32 & 0xFF);
        Debug.Log((uInt32 >> 8) & 0xFF);
        Debug.Log((uInt32 >> 16) & 0xFF);
        Debug.Log((uInt32 >> 24) & 0xFF);
    }

    private static void ToBits(byte[] bytes) {
        char[] result = new char[bytes.Length * 8];
        int bitPos = 0;

        while (bitPos < 8 * bytes.Length) {
            int byteIndex = bitPos / 8;
            int offset = bitPos % 8;
            bool isSet = (bytes[byteIndex] & (1 << offset)) != 0;

            // isSet = [True] if the bit at bitPos is set, false otherwise

            result[result.Length - 1 - bitPos] = isSet ? '1' : '0';
            bitPos++;
        }

        Debug.Log(new string(result));
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