using NUnit.Framework;
using UIForia;

namespace Tests {

    public unsafe class StingInternSystemTest {

        [Test]
        public void InternsAConstantString() {
            using (StringInternSystem system = new StringInternSystem()) {
                int idx0 = system.AddConstant("hello");
                int idx1 = system.AddConstant("how");
                int idx2 = system.AddConstant("are");
                int idx3 = system.AddConstant("you");
                int idx4 = system.AddConstant("today");
                int idx5 = system.AddConstant("?");

                Assert.AreEqual(0, idx0);
                Assert.AreEqual(1, idx1);
                Assert.AreEqual(2, idx2);
                Assert.AreEqual(3, idx3);
                Assert.AreEqual(4, idx4);
                Assert.AreEqual(5, idx5);

                Assert.AreEqual(int.MinValue, system.GetRefCount("hello"));
                Assert.AreEqual(int.MinValue, system.GetRefCount("how"));
                Assert.AreEqual(int.MinValue, system.GetRefCount("are"));
                Assert.AreEqual(int.MinValue, system.GetRefCount("you"));
                Assert.AreEqual(int.MinValue, system.GetRefCount("today"));
                Assert.AreEqual(int.MinValue, system.GetRefCount("?"));
                Assert.AreEqual(0, system.GetRefCount("not-here"));
            }
        }

        [Test]
        public void InternsANonConstantString() {
            using (StringInternSystem system = new StringInternSystem()) {
                int idx0 = system.Add("hello");
                int idx1 = system.Add("how");
                int idx2 = system.Add("are");
                int idx3 = system.Add("you");
                int idx4 = system.Add("today");
                int idx5 = system.Add("?");

                Assert.AreEqual(0, idx0);
                Assert.AreEqual(1, idx1);
                Assert.AreEqual(2, idx2);
                Assert.AreEqual(3, idx3);
                Assert.AreEqual(4, idx4);
                Assert.AreEqual(5, idx5);

                Assert.AreEqual(1, system.GetRefCount("hello"));
                Assert.AreEqual(1, system.GetRefCount("how"));
                Assert.AreEqual(1, system.GetRefCount("are"));
                Assert.AreEqual(1, system.GetRefCount("you"));
                Assert.AreEqual(1, system.GetRefCount("today"));
                Assert.AreEqual(1, system.GetRefCount("?"));
                Assert.AreEqual(0, system.GetRefCount("not-here"));
            }
        }

        [Test]
        public void DoNotRemoveConstantString() {
            using (StringInternSystem system = new StringInternSystem()) {
                int idx0 = system.AddConstant("hello");
                system.Remove("hello");
                Assert.AreEqual(int.MinValue, system.GetRefCount("hello"));
                StringHandle handle0 = system.GetStringHandle(idx0);
                Assert.IsTrue(BurstableStringUtil.Compare(handle0.data, handle0.length, "hello"));
            }
        }
        
        [Test]
        public void RemoveNonConstantString() {
            using (StringInternSystem system = new StringInternSystem()) {
                int idx0 = system.Add("hello");
                StringHandle handle0 = system.GetStringHandle(idx0);
                Assert.AreEqual(1, system.GetRefCount("hello"));
                Assert.IsTrue(BurstableStringUtil.Compare(handle0.data, handle0.length, "hello"));
                system.Remove("hello");
                Assert.AreEqual(0, system.GetRefCount("hello"));

                StringHandle handle1 = system.GetStringHandle(idx0);
                Assert.IsFalse(BurstableStringUtil.Compare(handle1.data, handle1.length, "hello"));
            }
        }

        [Test]
        public void ReferenceCountANonConstantString() {
            using (StringInternSystem system = new StringInternSystem()) {
                system.Add("hello");
                system.Add("how");
                system.Add("are");
                system.Add("you");
                system.Add("today");
                system.Add("?");

                system.Remove("today");

                Assert.AreEqual(1, system.GetRefCount("hello"));
                Assert.AreEqual(1, system.GetRefCount("how"));
                Assert.AreEqual(1, system.GetRefCount("are"));
                Assert.AreEqual(1, system.GetRefCount("you"));
                Assert.AreEqual(0, system.GetRefCount("today"));
                Assert.AreEqual(1, system.GetRefCount("?"));
                Assert.AreEqual(0, system.GetRefCount("not-here"));

                int idx0 = system.Add("today");
                int idx1 = system.Add("today");
                Assert.AreEqual(idx0, idx1);

                Assert.AreEqual(2, system.GetRefCount("today"));

                Assert.AreEqual("today", system.GetString(idx0));
                Assert.AreEqual("today", system.GetString(idx1));

                StringHandle handle0 = system.GetStringHandle(idx0);
                StringHandle handle1 = system.GetStringHandle(idx1);

                Assert.IsTrue(BurstableStringUtil.Compare(handle0.data, handle0.length, "today"));
                Assert.IsTrue(BurstableStringUtil.Compare(handle1.data, handle1.length, "today"));
            }
        }

    }

}