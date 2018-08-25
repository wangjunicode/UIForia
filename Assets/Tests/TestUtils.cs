using System;
using NUnit.Framework;
using Src.Compilers.AliasSource;

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
        
    }

}