using System;
using NUnit.Framework;
using UIForia.Compilers;
using UIForia.Compilers.AliasSource;

namespace Tests {

    public static class TestUtils {

        [Flags]
        public enum TestEnum {

            One = 1 << 0,
            Two = 1 << 1,
            Three = 1 << 2

        }

        public static T As<T>(object thing) {
            return (T) thing;
        }

        public static T AssertInstanceOfAndReturn<T>(object target) {
            Assert.IsInstanceOf<T>(target);
            return (T) target;
        }

        public class TestUIElementType : UIElement {

            public int intValue;

        }

        public class TestAliasSource : IAliasSource {

            public object value;

            public TestAliasSource(object value) {
                this.value = value;
            }

            public object ResolveAlias(string alias, object data = null) {
                return value;
            }

        }

        public class FakeRootElement : UIElement {

            public int arg0CallCount;

            public string[] arg1Params;
            public string[] arg2Params;
            public string[] arg3Params;
            public string[] arg4Params;

            public void HandleSomeEventArg0() {
                arg0CallCount++;
            }

            public void HandleSomeEventArg1(string val) {
                arg1Params = new[] {val};
            }


            public void HandleSomeEventArg2(string arg0, string arg1) {
                arg2Params = new[] {arg0, arg1};
            }


            public void HandleSomeEventArg3(string arg0, string arg1, string arg2) {
                arg3Params = new[] {arg0, arg1, arg2};
            }


            public void HandleSomeEventArg4(string arg0, string arg1, string arg2, string arg3) {
                arg4Params = new[] {arg0, arg1, arg2, arg3};
            }

        }

        public class FakeElement : UIElement {

            public delegate void SomeDelegateArg0();

            public delegate void SomeDelegateArg1(string arg0);

            public delegate void SomeDelegateArg2(string arg0, string arg1);

            public delegate void SomeDelegateArg3(string arg0, string arg1, string arg2);

            public delegate void SomeDelegateArg4(string arg0, string arg1, string arg2, string arg3);

            public event SomeDelegateArg0 onSomeEventArg0;
            public event SomeDelegateArg1 onSomeEventArg1;
            public event SomeDelegateArg2 onSomeEventArg2;
            public event SomeDelegateArg3 onSomeEventArg3;
            public event SomeDelegateArg4 onSomeEventArg4;


            public void InvokeEvtArg0() {
                onSomeEventArg0?.Invoke();
            }

            public void InvokeEvtArg1(string arg0) {
                onSomeEventArg1?.Invoke(arg0);
            }

            public void InvokeEvtArg2(string arg0, string arg1) {
                onSomeEventArg2?.Invoke(arg0, arg1);
            }

            public void InvokeEvtArg3(string arg0, string arg1, string arg2) {
                onSomeEventArg3?.Invoke(arg0, arg1, arg2);
            }

            public void InvokeEvtArg4(string arg0, string arg1, string arg2, string arg3) {
                onSomeEventArg4?.Invoke(arg0, arg1, arg2, arg3);
            }

        }

    }

}