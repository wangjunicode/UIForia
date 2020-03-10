using NUnit.Framework;

namespace Tests {

    public static class TestUtils {

        public static T As<T>(object thing) {
            return (T) thing;
        }

        public static T AssertInstanceOfAndReturn<T>(object target) {
            Assert.IsInstanceOf<T>(target);
            return (T) target;
        }

    }

}