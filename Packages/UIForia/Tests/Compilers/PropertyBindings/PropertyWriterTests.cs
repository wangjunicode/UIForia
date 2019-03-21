using System;
using System.Reflection;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Bindings;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

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
    private class WriterThing1 : UIElement {

        public string writeTarget = "initial";

    }
    
    [Test]
    public void CompilesWriterAttrModifier() {
        
        ExpressionCompiler compiler = new ExpressionCompiler();
        
        MockApplication app = new MockApplication(typeof(WriterThing1));
        WriterThing1 root = (WriterThing1)app.RootElement;
        ThingThatWrites writer = root.FindFirstByType<ThingThatWrites>();
        writer.value = "this is a value";
        
        string attrKey = "value";
        string attrValue = "writeTarget";
        
        FieldInfo fieldInfo = ReflectionUtil.GetFieldInfo(typeof(ThingThatWrites), attrKey);
        Type elementType = typeof(ThingThatWrites);
        
        ReflectionUtil.LinqAccessor accessor = ReflectionUtil.GetLinqFieldAccessors(elementType, fieldInfo.FieldType, attrKey);

        WriteTargetExpression expression = compiler.CompileWriteTarget(typeof(WriterThing1), fieldInfo.FieldType, attrValue);

        Binding writeBinding = (Binding)ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(WriteBinding<,>),
            new GenericArguments(typeof(ThingThatWrites), typeof(string)),
            new ConstructorArguments(attrKey, expression, accessor.getter)
        );
        writeBinding.Execute(writer, new ExpressionContext(root, writer));
//        Assert.AreEqual(root.writeTarget, "initial");
//        app.Update();
        Assert.AreEqual(writer.value, "this is a value");
        Assert.AreEqual(writer.value, root.writeTarget);
    }

}