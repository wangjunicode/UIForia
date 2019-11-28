using System;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UnityEngine;

[TestFixture]
public class TestTemplateCompiler_Bindings {

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <Div/>
            </Contents>
        </UITemplate>
    ")]
    public class BindingTestFloatThing : UIElement {

        public float value;

    }

    [Template(TemplateType.String, @"
        <UITemplate>
            <Contents>
                <BindingTestFloatThing value=""floatVal""/>
            </Contents>
        </UITemplate>
    ")]
    public class BindingTest_Simple : UIElement {

        public float floatVal;

        public event Action evt;

        public void Handler() {
            Debug.Log("handled");
        }

        public void Trigger() {
            evt?.Invoke();
        }

    }

    static Action<object, object> MakeFunc(EventInfo sourceEvent, MethodInfo targetMethod)
    {
        // setting up objects involved
        var sourceParam = Expression.Parameter(sourceEvent.EventHandlerType, "source");
        var targetParam = Expression.Parameter(typeof(object), "target");
        var sourceParamCast = Expression.Convert(sourceParam, sourceEvent.DeclaringType);
        var targetParamCast = Expression.Convert(targetParam, targetMethod.DeclaringType);
        var createDelegate = typeof(Delegate).GetMethod(nameof(Delegate.CreateDelegate), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Type), typeof(object), typeof(MethodInfo) }, null);

        // Create a delegate of type sourceEvent.EventHandlerType
        var createDelegateCall = Expression.Call(createDelegate, Expression.Constant(sourceEvent.EventHandlerType), targetParam, Expression.Constant(targetMethod));

        // Cast the Delegate to its real type
        var delegateCast = Expression.Convert(createDelegateCall, sourceEvent.EventHandlerType);

        // Subscribe to the event
        var addMethodCall = Expression.Call(sourceParamCast, sourceEvent.AddMethod, delegateCast);

        var lambda = Expression.Lambda<Action<object, object>>(addMethodCall, sourceParam, targetParam);
        var subscriptionAction = lambda.Compile();
        Debug.Log(lambda.ToCSharpCode());

        return subscriptionAction;
    }
    
//    private void Do(object source, object target) {
//        (TestTemplateCompiler_Bindings.BindingTest_Simple)source.add_evt((System.Action)System.Delegate.CreateDelegate(typeof(System.Action), target, Void Handler()));
//    }

    public static void Subscribe(object target, EventInfo info, object del) {
        
    }
    
    // <Thing onSelected="Selected"/>
     
    // this.onSelected += (root | context | global).method;
    
    [Test]
    public void TestSimpleBinding() {
        
        LinqCompiler compiler = new LinqCompiler();
        compiler.SetSignature(new Parameter<BindingTest_Simple>("obj", ParameterFlags.NeverNull));

        ParameterExpression root = compiler.GetParameter("obj");
        EventInfo evtInfo = typeof(BindingTest_Simple).GetEvent(nameof(BindingTest_Simple.evt));
        MethodInfo m = typeof(BindingTest_Simple).GetMethod("Handler");
        
        BindingTest_Simple x = new BindingTest_Simple();


        ParameterExpression castDelegate = compiler.AddVariable(evtInfo.EventHandlerType, "castDelegate");
        MemberExpression memberAccess = Expression.MakeMemberAccess(root, m);
        compiler.Assign(castDelegate, memberAccess);
        // (obj) => 
        // var evt = typeof(X).GetEvent("Y");
        // return (target) => evt.AddEventHandler(target, 
//        fn(x);
        
        x.Trigger();

        //MockApplication app = new MockApplication(typeof(BindingTest_Simple));
        //BindingTest_Simple root = app.RootElement.GetChild(0) as BindingTest_Simple;
        //root.floatVal = 123f;
        //app.Update();
        //BindingTestFloatThing thing = app.RootElement.GetChild(0).FindFirstByType<BindingTestFloatThing>();
        //Assert.AreEqual(123, thing.value);
    }

}