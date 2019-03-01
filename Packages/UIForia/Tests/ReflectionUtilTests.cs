using System;
using System.Reflection;
using NUnit.Framework;
using UIForia.Util;

[TestFixture]
public class ReflectionUtilTests {

    private class RefUtilClass1 : IDoSomething {

        public string value;

        public RefUtilClass1(string value) {
            this.value = value;
        }

        public string GetValue() {
            return value;
        }

        public string GetValueWithArg(string arg) {
            return arg + value;
        }

        public virtual string CallVirtMethod() {
            return "base";
        }
    }

    private class RefUtilClass1Extends : RefUtilClass1 {

        public RefUtilClass1Extends(string value) : base(value) { }

        public override string CallVirtMethod() {
            return "extends";
        }
        
    }
    
    private class RefUtilClass1ExtendsAgain : RefUtilClass1Extends {

        public RefUtilClass1ExtendsAgain(string value) : base(value) { }

    }

    private class RefUtilClass2 : IDoSomething {

        public string value;

        public RefUtilClass2(string value) {
            this.value = value;
        }

        public string GetValue() {
            return value;
        }

    }


    private interface IDoSomething {

        string GetValue();

    }
    
    private interface IDoSomething2 {

        string GetValue();

    }

    [Test]
    public void TestOpenDelegate() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1 two = new RefUtilClass1("two");
        MethodInfo info = typeof(RefUtilClass1).GetMethod("GetValue");
        Func<RefUtilClass1, string> openDelegate = ReflectionUtil.CreateOpenDelegate<Func<RefUtilClass1, string>>(info);
        Assert.AreEqual("one", openDelegate(one));
        Assert.AreEqual("two", openDelegate(two));
    }

    [Test]
    public void GetOpenDelegateFromType() {
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass1).GetMethod("GetValueWithArg");
        Type openDelegateType1 = ReflectionUtil.GetOpenDelegateType(info1);
        Type openDelegateType2 = ReflectionUtil.GetOpenDelegateType(info2);
        Assert.AreEqual(typeof(Func<RefUtilClass1, string>), openDelegateType1);
        Assert.AreEqual(typeof(Func<RefUtilClass1, string, string>), openDelegateType2);
    }

    [Test]
    public void TestOpenDelegateChild() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1 two = new RefUtilClass1Extends("two");
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass1Extends).GetMethod("GetValue");
        Func<RefUtilClass1, string> d1 = (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info1);
        Func<RefUtilClass1, string> d2 =  (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info2);
        Assert.AreEqual(d1, d2);
        
        Assert.AreEqual("one", d1(one));
        Assert.AreEqual("two", d1(two));
        
    }
    
    [Test]
    public void TestOpenDelegateAncestor() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1Extends two = new RefUtilClass1Extends("two");
        RefUtilClass1ExtendsAgain three = new RefUtilClass1ExtendsAgain("three");
        
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass1Extends).GetMethod("GetValue");
        MethodInfo info3 = typeof(RefUtilClass1ExtendsAgain).GetMethod("GetValue");
        
        Func<RefUtilClass1, string> d1 = (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info1);
        Func<RefUtilClass1, string> d2 =  (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info2);
        Func<RefUtilClass1, string> d3 =  (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info3);
        
        Assert.AreEqual(d1, d2);
        Assert.AreEqual(d2, d3);
        
        Assert.AreEqual("one", d1(one));
        Assert.AreEqual("two", d1(two));
        Assert.AreEqual("three", d1(three));
        
    }
    
    [Test]
    public void TestOpenDelegateWorksWithInterface() {
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass2).GetMethod("GetValue");
        Delegate d1 = ReflectionUtil.GetDelegate(info1);
        Delegate d2 = ReflectionUtil.GetDelegate(info2);
        Assert.AreEqual(d1, d2);
    }

    [Test]
    public void TestCallsVirtualMethod() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1Extends two = new RefUtilClass1Extends("two");
        MethodInfo info1 = typeof(RefUtilClass1Extends).GetMethod("CallVirtMethod");
        Func<RefUtilClass1, string> d1 = (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info1);
        
        Assert.AreEqual("base", d1(one));
        Assert.AreEqual("extends", d1(two));

    }
}