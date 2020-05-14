using NUnit.Framework;
using UIForia;

namespace Tests {

    public class StringTableTest {

        [Test]
        public void GetStringRef() {
            using (StaticStringTable stringTable = StaticStringTable.Create()) {

                int key0 = stringTable.GetOrCreateReference("hello");
                int key1 = stringTable.GetOrCreateReference("hello");
                int key2 = stringTable.GetOrCreateReference("world");
                int prefixKey = stringTable.GetOrCreateReference("hel");
                int postFixKey = stringTable.GetOrCreateReference("ld");
                int containsKey = stringTable.GetOrCreateReference("ll");
                
                Assert.AreEqual(key0, key1);
                Assert.AreNotEqual(key0, key2);

                BurstableString hello1 = stringTable.Get(key0);
                BurstableString hello2 = stringTable.Get(key1);
                
                Assert.AreEqual(hello1.ToString(), "hello");

                BurstableString world = stringTable.Get(key2);
                BurstableString prefix = stringTable.Get(prefixKey);
                BurstableString postFix = stringTable.Get(postFixKey);
                BurstableString contains = stringTable.Get(containsKey);
                
                Assert.IsTrue(hello1 == hello2);
                Assert.IsFalse(hello1 == world);
                
                Assert.IsTrue(hello1.StartsWith(prefix));
                Assert.IsTrue(world.EndsWith(postFix));
                Assert.IsFalse(hello1.EndsWith(postFix));
                
                Assert.IsTrue(hello1.Contains(prefix));
                Assert.IsFalse(hello1.Contains(postFix));
                Assert.IsTrue(hello1.Contains(contains));
            }
        }

    }

}