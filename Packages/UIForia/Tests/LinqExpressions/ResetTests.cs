using System;
using NUnit.Framework;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.LinqExpressions;
using UIForia.Parsing;
using UnityEngine;

[TestFixture]
public class ResetTests {

    private class ResetThing : UIElement {

        public int autoInt { get; set; }
        private Action callback;
        public event Action evt;
        public readonly int readonlyValue = 12;
        public int someOtherValue = 12;

        private float x;

        public void SetCallback(Action action) {
            this.callback = action;
        }

        public Action GetCallback() {
            return callback;
        }

        public bool EvtIsNull() {
            return evt == null;
        }

        public event Action propertyEvent {
            add { }
            remove { }
        }

        public void Clear() {
            this.callback = null;
            this.autoInt = 0;
            this.someOtherValue = 0;
            this.evt = null;
            this.x = 0;
            this.flags = 0;
        }

    }

    
    public class TestThing123 : UIElement {

        public float value = 12.3f;
        private float value2 = -12.3f;

    }


    [Test]
    public void MakeConstructor() {
        Type type = typeof(TestThing123);
        
        ProcessedType processedType = new ProcessedType(type, new TemplateAttribute(TemplateType.String, ""));
        UIElement element = processedType.CreateInstance();
        Debug.Log("Element");
//        compiler.SetSignature(new Parameter<ProcessedType>("processedType", ParameterFlags.NeverNull), typeof(UIElement));
//        ParameterExpression typeParam = compiler.GetParameter("processedType");
//        MethodCallExpression getElement = Expression.Call(typeParam, typeof(ProcessedType).GetMethod("GetPooledInstance"));
//        MemberExpression constructorFunctionAccess = Expression.MakeMemberAccess(typeParam, typeof(ProcessedType).GetField("constructorFn"));
//
//        MethodCallExpression createFn = Expression.Call(typeParam, typeof(ProcessedType).GetMethod("CreateCtor"));
//
//        ParameterExpression retnParam = compiler.AddVariable(typeof(UIElement), "retn");
//        ParameterExpression p = compiler.AddVariable(typeof(object), "x");
//        compiler.Assign(p, constructorFunctionAccess);
//        compiler.Assign(retnParam, getElement);
//
//        compiler.IfEqual(p, Expression.Default(typeof(object)), () => {
//            compiler.RawExpression(createFn);
//            compiler.Assign(p, constructorFunctionAccess);
//        });
//
//        Type genericType = ReflectionUtil.CreateGenericType(typeof(Action<>), type);
//        ParameterExpression z = compiler.AddVariable(genericType, "z");
//
//        UnaryExpression converted = Expression.Convert(p, genericType);
//
//        compiler.Assign(z, converted);
//        compiler.RawExpression(Expression.Invoke(z, Expression.Convert(retnParam, type)));
//        compiler.RawExpression(retnParam);
//
//        Debug.Log(compiler.BuildLambda().ToCSharpCode());
//        Func<ProcessedType, UIElement> l = (Func<ProcessedType, UIElement>) compiler.BuildLambda().Compile();
//
//        UIElement result = l(processedType);
//        Debug.Log(result);
    }

    // todo -- use this & profile! https://stackoverflow.com/questions/16363838/how-do-you-call-a-constructor-via-an-expression-tree-on-an-existing-object
    [Test]
    public void Creates() {
        ResetThing thing = new ResetThing();
//        ResetThing thing2  = (ResetThing)LinqExpressions.CreateInstance(thing, typeof(ResetThing));
//        Assert.AreEqual(thing2.someOtherValue, 12);
//        thing2.someOtherValue = 15;
//        ResetThing thing3 = (ResetThing) LinqExpressions.CreateInstance(thing2, typeof(ResetThing));
//        Assert.AreEqual(thing2, thing3);
//        Assert.AreEqual(thing2.someOtherValue, 12);

//        int count = 1000000;
//        var ctro = typeof(ResetThing).GetConstructor(Type.EmptyTypes);
//        var stopwatch = new Stopwatch();
//        Action<object> clear = LinqExpressions.CompileClear(typeof(ResetThing));
//
//        stopwatch.Start();
//        for (int i = 0; i < count; i++) {
//            thing = new ResetThing();
////            LinqExpressions.ResetInstance(thing, constructor, typeof(ResetThing), clear);
//        }
//
//        stopwatch.Stop();
//        UnityEngine.Debug.Log("New");
//        UnityEngine.Debug.Log(stopwatch.ElapsedTicks);
//        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
//        stopwatch.Reset();
//        stopwatch.Start();
//
//        // in development after creating a type for the first time, ensure that it is empty. if not the user added some field initializers that should not be there
//        // in the far future maybe we can make a util using rosyln to remap field initializers into a create function.
//        for (int i = 0; i < count; i++) {
//            //thing = (ResetThing) FormatterServices.GetUninitializedObject(typeof(ResetThing));
//            LinqExpressions.ResetInstance(thing, ctro, clear);
//        }
//
//        stopwatch.Stop();
//        UnityEngine.Debug.Log("Clear");
//
//        UnityEngine.Debug.Log(stopwatch.ElapsedTicks);
//        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    public void Clears() {
        ResetThing element = new ResetThing();
        element.flags |= UIElementFlags.TextElement;

        element.autoInt = 1414;
        element.SetCallback(() => { });
        element.evt += () => { };

        Assert.NotNull(element.GetCallback());
        Assert.NotNull(element.layoutResult);
        Action<object> clear = LinqExpressions.CompileClear(typeof(ResetThing));
        clear(element);
        Assert.AreEqual(default(int), element.autoInt);
        Assert.AreEqual(default(Action), element.GetCallback());
        Assert.AreEqual(default(UIElementFlags), element.flags);
        Assert.AreEqual(default(LayoutResult), element.layoutResult);
        Assert.AreEqual(default(int), element.readonlyValue);
        Assert.IsTrue(element.EvtIsNull());
    }

}