using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using DemoExternal;
using Mono.Linq.Expressions;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UnityEngine;

public class DemoMethod : Attribute { }

[Template("CompilerDemo/Tmp.xml")]
public class Tmp : UIElement {

    public string output;
    public string compiledCode;

    public List<ISelectOption<MethodInfo>> options;
    public MethodInfo selected;
    public string expression;

    public void OnValueChanged() {
        selected.Invoke(this, null);
    }
    
    public override void OnCreate() {
        options = new List<ISelectOption<MethodInfo>>();
        
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
        
        for (int i = 0; i < methods.Length; i++) {
            if (methods[i].GetCustomAttribute(typeof(DemoMethod)) != null) {
                options.Add(new SelectOption<MethodInfo>(Regex.Replace(methods[i].Name, "([A-Z])", " $1", RegexOptions.Compiled), methods[i]));
            }
        }
    }

    [DemoMethod]
    private void VectorDotProduct() {
        LinqCompiler compiler = new LinqCompiler();

        compiler.SetSignature<float>(new Parameter<Vector3>("x"), new Parameter<Vector3>("y"));
        expression = "UnityEngine.Vector3.Dot(x, y)";
        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<Vector3, Vector3, float> fn = compiler.Compile<Func<Vector3, Vector3, float>>();

        output = fn(new Vector3(4, 5, 6), new Vector3(-4, 5, 1)).ToString();
        
    }
    
    [DemoMethod]
    private void VectorDotProductWithReferenceType() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "UnityEngine.Vector3.Dot(refThing.v, y)"; 
        compiler.SetSignature<float>(new Parameter<VectorHolder>("refThing"), new Parameter<Vector3>("y"));

        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<VectorHolder, Vector3, float> fn = compiler.Compile<Func<VectorHolder, Vector3, float>>();
        
        output = fn(
            new VectorHolder() {
                v = new Vector3(4, 5, 6)
            },
            new Vector3(-4, 5, 1)
        ).ToString();
    }
    
    [DemoMethod]
    private void VectorDotProductWithReferenceTypeWithoutNullChecking() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "UnityEngine.Vector3.Dot(refThing.v, y)";
        compiler.SetSignature<float>(new Parameter<VectorHolder>("refThing", ParameterFlags.NeverNull), new Parameter<Vector3>("y"));

        compiler.Return("UnityEngine.Vector3.Dot(refThing.v, y)");

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<VectorHolder, Vector3, float> fn = compiler.Compile<Func<VectorHolder, Vector3, float>>();
        
        output = fn(
            new VectorHolder() {
                v = new Vector3(4, 5, 6)
            },
            new Vector3(-4, 5, 1)
        ).ToString();
    }
    
    [DemoMethod]
    private void VectorDotProductWithStructType() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "UnityEngine.Vector3.Dot(refThing.v, y)";
        compiler.SetSignature<float>(new Parameter<VectorHolderStruct>("refThing", ParameterFlags.NeverNull), new Parameter<Vector3>("y"));

        compiler.Return("UnityEngine.Vector3.Dot(refThing.v, y)");

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<VectorHolderStruct, Vector3, float> fn = compiler.Compile<Func<VectorHolderStruct, Vector3, float>>();
        
        output = fn(
            new VectorHolderStruct() {
                v = new Vector3(4, 5, 6)
            },
            new Vector3(-4, 5, 1)
        ).ToString();
    }

    [DemoMethod]
    private void Assignment() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "refThing.v.x *= 3f\nUnityEngine.Vector3.Dot(refThing.v, y)";
        compiler.SetSignature<float>(new Parameter<VectorHolderStruct>("refThing", ParameterFlags.NeverNull), new Parameter<Vector3>("y"));

        compiler.Statement("refThing.v.x *= 3f");
        compiler.Return("UnityEngine.Vector3.Dot(refThing.v, y)");

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<VectorHolderStruct, Vector3, float> fn = compiler.Compile<Func<VectorHolderStruct, Vector3, float>>();
        
        output = fn(
            new VectorHolderStruct() {
                v = new Vector3(4, 5, 6)
            },
            new Vector3(-4, 5, 1)
        ).ToString();
    }
    
    [DemoMethod]
    private void NestedNew() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "new ThingWithOptionals(new ThingWithOptionals(12f), 2)";

        compiler.SetSignature<ThingWithOptionals>();
        compiler.SetNamespaces(new string[] { "DemoExternal" });
        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<ThingWithOptionals> fn = compiler.Compile<Func<ThingWithOptionals>>();
        
        output = fn().ToString();
    }
    
    [DemoMethod]
    private void OperatorOverloads() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "opOverload.v0 + opOverload.v1";
        compiler.SetSignature<Vector3>(new Parameter<OperatorOverloadTest>("opOverload"));
        compiler.SetNamespaces(new string[] { "UnityEngine" });
        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();
        OperatorOverloadTest overloadTest = new OperatorOverloadTest();

        overloadTest.v0 = new Vector3(1124, 522, 241);
        overloadTest.v1 = new Vector3(1124, 522, 241);
        Func<OperatorOverloadTest, Vector3> fn = compiler.Compile<Func<OperatorOverloadTest, Vector3>>();
        
        output = fn(overloadTest).ToString();
    }
    
    [DemoMethod]
    private void PartialTernary() {
        LinqCompiler compiler = new LinqCompiler();
        expression = "vecHolder ? vecHolder.v.x";
        compiler.SetSignature<float>(new Parameter<VectorHolder>("vecHolder"));
        compiler.SetNamespaces(new string[] { "UnityEngine" });
        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<VectorHolder, float> fn = compiler.Compile<Func<VectorHolder, float>>();
        
        output = fn(new VectorHolder() { v = Vector3.left}).ToString();
    }
    
    [DemoMethod]
    private void SafeAccess() {
        expression = "thing.vec3Array?[arg0 + thing.intVal - arg1] ?? Vector3.one";
        
        LinqCompiler compiler = new LinqCompiler();

        compiler.SetSignature<Vector3>(
            new Parameter<SafeAccessThing>("thing"),
            new Parameter<int>("arg0"),
            new Parameter<int>("arg1")
        );
        
        SafeAccessThing thing = new SafeAccessThing();

        thing.intVal = 3;
        thing.vec3Array = new[] {
            new Vector3(2, 2, 2),
            new Vector3(4, 5, 6),
            new Vector3(7, 8, 9)
        };
        
        compiler.AddNamespace("UnityEngine");
        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<SafeAccessThing, int, int, Vector3> fn = compiler.Compile<Func<SafeAccessThing, int, int, Vector3>>();
        
        output = fn(thing, 1, 2).ToString();
    }
    
    [DemoMethod]
    private void IsOperator() {
        expression = "thing is Vector3";
        
        LinqCompiler compiler = new LinqCompiler();

        compiler.SetSignature<bool>(
            new Parameter<object>("thing")
        );
        compiler.AddNamespace("UnityEngine");

        compiler.Return(expression);

        compiledCode = compiler.BuildLambda().ToCSharpCode();

        Func<object, bool> fn = compiler.Compile<Func<object, bool>>();
        
        output = fn("str").ToString();
    }

    private class SafeAccessThing {

        public Vector3[] vec3Array;
        public int intVal;


    }
    
    private class OperatorOverloadTest {

        public Vector3 v0;
        public Vector3 v1;

    }
    
    public struct VectorHolderStruct {

        public Vector3 v;

    }
    
    public class VectorHolder {

        public Vector3 v;

    }
    
}

namespace DemoExternal {
    public class ThingWithOptionals {

        public readonly int x;
        public readonly int y;
        public readonly float f;

        public ThingWithOptionals(ThingWithOptionals other, int y = 2) {
            this.f = other.f;
            this.x = other.x;
            this.y = y;
        }

        public ThingWithOptionals(float f, int y = 2) {
            this.f = f;
            this.y = y;
        }

        public ThingWithOptionals(int x, int y = 2) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return $@"x = {x}, y = {y}, f = {f}";
        }

    }

}