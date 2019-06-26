using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UIForia.Bindings;
using UIForia.Compilers;
using UIForia.Expressions;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UnityEngine;
using Expression = System.Linq.Expressions.Expression;

[TestFixture]
public class TestLinqCompiler {

    private class LinqThing {

        public float floatValue;

        public ValueHolder<Vector3> refValueHolderVec3 = new ValueHolder<Vector3>();
        public ValueHolder<float> valueHolderFloat = new ValueHolder<float>();
        public StructValueHolder<Vector3> svHolderVec3;
        public Vector3[] vec3Array;
        public List<Vector3> vec3List;
        public Dictionary<string, Vector3> vec3Dic;

    }

    public struct StructValueHolder<T> {

        public T value;

        public StructValueHolder(T value) {
            this.value = value;
        }

    }

    public class ValueHolder<T> {

        public T value;

        public ValueHolder(T value = default) {
            this.value = value;
        }

    }

    private static readonly Type LinqType = typeof(LinqThing);

    public abstract class LinqBinding {

        public abstract void Execute(ExpressionContext ctx);

    }

    public class ReadBinding : LinqBinding {

        public override void Execute(ExpressionContext ctx) { }

    }

    public class BindingCompiler : LinqCompiler {

        public LambdaExpression BuildMemberReadBinding(Type root, Type elementType, AttributeDefinition attributeDefinition) {
            LinqCompiler compiler = new LinqCompiler();

            MethodInfo[] changedHandlers = GetPropertyChangedHandlers(elementType, "fieldName");

            compiler.AddParameter(root, "root");
            compiler.AddParameter(elementType, "element");

            LHSStatementChain left = compiler.CreateLHSStatementChain("element", attributeDefinition.key);
            RHSStatementChain right = compiler.CreateRHSStatementChain("root", left.targetExpression.Type, attributeDefinition.value);

            // if no listeners and field or auto prop then just assign, no need to check
                //compiler.Assign(left, Expression.Constant(34f));
            
            compiler.IfNotEqual(left, right, () => {
                compiler.Assign(left, right);
//
                if (changedHandlers != null) {
                    for (int i = 0; i < changedHandlers.Length; i++) {
                        //compiler.Invoke(rootParameter, changedHandlers[i], compiler.GetVariable("previousValue"));
                    }
                }

                if (elementType.Implements(typeof(IPropertyChangedHandler))) {
                    //compiler.Invoke("element", "OnPropertyChanged", compiler.GetVariable("currentValue"));
                }
            });

            //Debug.Log(PrintCode(compiler.BuildLambda()));

            return compiler.BuildLambda();
        }

        public LinqBinding CompileMemberReadBinding(Type root, Type elementType, AttributeDefinition attributeDefinition) {
            return null; //BuildMemberReadBinding(root, elementType, attributeDefinition).Compile();
        }

        private MethodInfo[] GetPropertyChangedHandlers(Type targetType, string fieldname) {
            return null;
        }

    }

    [Test]
    public void CompileSimpleMemberRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "4f"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        Assert.AreEqual(0, element.floatValue);
        fn.Invoke(root, element);
        Assert.AreEqual(4, element.floatValue);
    }

    [Test]
    public void CompileDotAccessRefMemberRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        // todo handle implicit conversion casting
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "valueHolderFloat.value"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.valueHolderFloat = new ValueHolder<float>(42);
        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(42, root.valueHolderFloat.value);

        fn.Invoke(root, element);

        Assert.AreEqual(42, element.floatValue);

        AssertStringsEqual(@"
            (LinqThing root, LinqThing element) =>
            {
                float rhsOutput;
                ValueHolder<float> part;
                float part0;

                part = root.valueHolderFloat;
                if (part == null)
                {
                    rhsOutput = default(float);
                    goto retn;
                }
                part0 = part.value;
                rhsOutput = part0;
            retn:
                if (element.floatValue != rhsOutput)
                {
                    element.floatValue = rhsOutput;
                }
            }
          ", PrintCode(expr));
    }

    [Test]
    public void CompileDotAccessStructMemberRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "svHolderVec3.value.z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.svHolderVec3 = new StructValueHolder<Vector3>(new Vector3(0, 0, 42));
        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(42, root.svHolderVec3.value.z);

        fn.Invoke(root, element);

        Assert.AreEqual(42, element.floatValue);
        AssertStringsEqual(@"
               (LinqThing root, LinqThing element) =>
        {
            float rhsOutput;
            StructValueHolder<Vector3> part;
            Vector3 part0;
            float part1;

            part = root.svHolderVec3;
            part0 = part.value;
            part1 = part0.z;
            rhsOutput = part1;
            if (element.floatValue != rhsOutput)
            {
                element.floatValue = rhsOutput;
            }
        }
        ", PrintCode(expr));
    }

    [Test]
    public void CompileDotAccessMixedStructRefMemberRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "refValueHolderVec3.value.z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.refValueHolderVec3 = new ValueHolder<Vector3>(new Vector3(0, 0, 42));
        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(42, root.refValueHolderVec3.value.z);

        fn.Invoke(root, element);

        Assert.AreEqual(42, element.floatValue);
        AssertStringsEqual(@"
        (LinqThing root, LinqThing element) =>
        {
                float rhsOutput;
                ValueHolder<Vector3> part;
                Vector3 part0;
                float part1;

                part = root.refValueHolderVec3;
                if (part == null)
                {
                    rhsOutput = default(float);
                    goto retn;
                }
                part0 = part.value;
                part1 = part0.z;
                rhsOutput = part1;
            retn:
                if (element.floatValue != rhsOutput)
                {
                    element.floatValue = rhsOutput;
                }
        }
        ", PrintCode(expr));
    }

    [Test]
    public void CompileIndexAccess_ConstIndex_StructRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "vec3Array[3].z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.vec3Array = new[] {
            new Vector3(),
            new Vector3(),
            new Vector3(),
            new Vector3(0, 0, 42)
        };

        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(42, root.vec3Array[3].z);
        Debug.Log(PrintCode(expr));

        fn.Invoke(root, element);

        Assert.AreEqual(42, element.floatValue);
        AssertStringsEqual(@"
        (LinqThing root, LinqThing element) =>
        {
            float rhsOutput;
            Vector3[] part;
            int indexer;
            Vector3 arrayVal;
            float part1;

            part = root.vec3Array;
            indexer = 3;
            if ((part == null) || ((indexer < 0) || (indexer >= part.Length)))
            {
                rhsOutput = default(float);
                goto retn;
            }
            arrayVal = part[indexer];
            part1 = arrayVal.z;
            rhsOutput = part1;
        retn:
            if (element.floatValue != rhsOutput)
            {
                element.floatValue = rhsOutput;
            }
        }
        ", PrintCode(expr));
    }

    [Test]
    public void CompileIndexAccess_NonArray_ConstIndex_StructRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "vec3List[3].z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.vec3List = new List<Vector3>();
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3(0, 0, 42));


        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(42, root.vec3List[3].z);
        Debug.Log(PrintCode(expr));

        fn.Invoke(root, element);

        Assert.AreEqual(42, element.floatValue);
        AssertStringsEqual(@"
        (LinqThing root, LinqThing element) =>
        {
            float rhsOutput;
            List<Vector3> part;
            int indexer;
            Vector3 indexVal;
            float part1;

            part = root.vec3List;
            indexer = 3;
            if ((part == null) || ((indexer < 0) || (indexer >= part.Count)))
            {
                rhsOutput = default(float);
                goto retn;
            }
            indexVal = part[indexer];
            part1 = indexVal.z;
            rhsOutput = part1;
        retn:
            if (element.floatValue != rhsOutput)
            {
                element.floatValue = rhsOutput;
            }
        }
        ", PrintCode(expr));
    }

    [Test]
    public void CompileIndexAccess_StringDictionary_StructRead() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "vec3Dic['two'].z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.vec3Dic = new Dictionary<string, Vector3>();
        root.vec3Dic["one"] = new Vector3(1, 1, 1);
        root.vec3Dic["two"] = new Vector3(2, 2, 2);
        root.vec3Dic["three"] = new Vector3(3, 3, 3);

        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(2, root.vec3Dic["two"].z);
        Debug.Log(PrintCode(expr));

        fn.Invoke(root, element);

        Assert.AreEqual(2, element.floatValue);
        AssertStringsEqual(@"
          (LinqThing root, LinqThing element) =>
          {
            float rhsOutput;
            Dictionary<string, Vector3> part;
            string indexer;
            Vector3 indexVal;
            float part1;

            part = root.vec3Dic;
            indexer = ""two"";
                if (part == null)
                {
                    rhsOutput = default(float);
                    goto retn;
                }
                indexVal = part[indexer];
                part1 = indexVal.z;
                rhsOutput = part1;
                retn:
                if (element.floatValue != rhsOutput)
                {
                    element.floatValue = rhsOutput;
                }
            }
        ", PrintCode(expr));
    }
    
    [Test]
    public void CompileIndexAccess_AttemptTypeCast() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        // using a float to index the list but list is indexed by int, should cast float to int
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("floatValue", "vec3List[3f].z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.vec3List = new List<Vector3>();
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3(0, 0, 42));


        Assert.AreEqual(0, element.floatValue);
        Assert.AreEqual(42, root.vec3List[3].z);
        Debug.Log(PrintCode(expr));
            
        fn.Invoke(root, element);

        Assert.AreEqual(42, element.floatValue);
        AssertStringsEqual(@"
        (LinqThing root, LinqThing element) =>
        {
            float rhsOutput;
            List<Vector3> part;
            int indexer;
            Vector3 indexVal;
            float part1;

            part = root.vec3List;
            indexer = 3;
            if ((part == null) || ((indexer < 0) || (indexer >= part.Count)))
            {
                rhsOutput = default(float);
                goto retn;
            }
            indexVal = part[indexer];
            part1 = indexVal.z;
            rhsOutput = part1;
        retn:
            if (element.floatValue != rhsOutput)
            {
                element.floatValue = rhsOutput;
            }
        }
        ", PrintCode(expr));
    }
    
    [Test]
    public void CompileStructFieldAssignment_Constant() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        // using a float to index the list but list is indexed by int, should cast float to int
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("svHolderVec3.value.x", "34"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
       
        Assert.AreEqual(0, element.svHolderVec3.value.x);
        Debug.Log(PrintCode(expr));
            
        fn.Invoke(root, element);

        Assert.AreEqual(34, element.svHolderVec3.value.x);
        
        AssertStringsEqual(@"
       (LinqThing root, LinqThing element) =>
        {
            StructValueHolder<Vector3> svHolderVec3;
            Vector3 value;
            float x;

            svHolderVec3 = element.svHolderVec3;
            value = svHolderVec3.value;
            x = value.x;
            if (x != 34)
            {
                value.x = 34;
                svHolderVec3.value = value;
                element.svHolderVec3 = svHolderVec3;
            }
        }
        ", PrintCode(expr));
    }
    
    [Test]
    public void CompileStructFieldAssignment_Variable() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        // using a float to index the list but list is indexed by int, should cast float to int
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("svHolderVec3.value.x", "floatValue"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.floatValue = 35;
        Assert.AreEqual(0, element.svHolderVec3.value.x);
            
        fn.Invoke(root, element);

        Assert.AreEqual(35, element.svHolderVec3.value.x);
        
        AssertStringsEqual(@"
               (LinqThing root, LinqThing element) =>
        {
            StructValueHolder<Vector3> svHolderVec3;
            Vector3 value;
            float x;
            float floatValue;

            svHolderVec3 = element.svHolderVec3;
            value = svHolderVec3.value;
            x = value.x;
            floatValue = root.floatValue;
            if (x != floatValue)
            {
                value.x = floatValue;
                svHolderVec3.value = value;
                element.svHolderVec3 = svHolderVec3;
            }
        }
        ", PrintCode(expr));
    }
    
    [Test]
    public void CompileStructFieldAssignment_Accessor() {
        BindingCompiler bindingCompiler = new BindingCompiler();
        // using a float to index the list but list is indexed by int, should cast float to int
        LambdaExpression expr = bindingCompiler.BuildMemberReadBinding(LinqType, LinqType, new AttributeDefinition("svHolderVec3.value.x", "vec3List[3].z"));
        Action<LinqThing, LinqThing> fn = (Action<LinqThing, LinqThing>) expr.Compile();
        LinqThing root = new LinqThing();
        LinqThing element = new LinqThing();
        root.vec3List = new List<Vector3>();
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3());
        root.vec3List.Add(new Vector3(0, 0, 42));

        Assert.AreEqual(0, element.svHolderVec3.value.x);
        Assert.AreEqual(42, root.vec3List[3].z);
        Debug.Log(PrintCode(expr));

        fn.Invoke(root, element);

        Assert.AreEqual(42, element.svHolderVec3.value.x);
        
        AssertStringsEqual(@"
        (LinqThing root, LinqThing element) =>
        {
            StructValueHolder<Vector3> svHolderVec3;
            Vector3 value;
            float x;
            float rhsOutput;
            List<Vector3> part;
            int indexer;
            Vector3 indexVal;
            float part1;

            svHolderVec3 = element.svHolderVec3;
            value = svHolderVec3.value;
            x = value.x;
            part = root.vec3List;
            indexer = 3;
            if ((part == null) || ((indexer < 0) || (indexer >= part.Count)))
            {
                rhsOutput = default(float);
                goto retn;
            }
            indexVal = part[indexer];
            part1 = indexVal.z;
            rhsOutput = part1;
        retn:
            if (x != rhsOutput)
            {
                value.x = rhsOutput;
                svHolderVec3.value = value;
                element.svHolderVec3 = svHolderVec3;
            }
        }
        ", PrintCode(expr));
    }

    public void AssertStringsEqual(string a, string b) {
        string[] splitA = a.Trim().Split('\n');
        string[] splitB = b.Trim().Split('\n');

        Assert.AreEqual(splitA.Length, splitB.Length);

        for (int i = 0; i < splitA.Length; i++) {
            Assert.AreEqual(splitA[i].Trim(), splitB[i].Trim());
        }
    }


    private static string PrintCode(IList<Expression> expressions) {
        string retn = "";
        for (int i = 0; i < expressions.Count; i++) {
            retn += Mono.Linq.Expressions.CSharp.ToCSharpCode(expressions[i]);
            if (i != expressions.Count - 1) {
                retn += "\n";
            }
        }

        return retn;
    }

    private static string PrintCode(Expression expression) {
        return Mono.Linq.Expressions.CSharp.ToCSharpCode(expression);
    }

}