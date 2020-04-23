using NUnit.Framework;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace Tests {

    public class UnsafePagedListTests {

        [Test]
        public unsafe void FitAlignedRangesToListPage() {

            UnsafePagedList<int> list = new UnsafePagedList<int>(16, Allocator.TempJob);

            int val = 0;
            int* ints = stackalloc int[8];
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < 8; j++) {
                    ints[j] = val++;
                }

                list.AddRangeToSamePage(ints, 8);

            }

            Assert.AreEqual(5, list.pageCount);
            for (int i = 0; i < 80; i++) {
                Assert.AreEqual(i, list[i]);
            }

            list[36] = 1290;
            Assert.AreEqual(1290, list[36]);
            list.Dispose();
        }
        
        [Test]
        public unsafe void FitUnalignedRangesToListPage() {

            UnsafePagedList<long> list = new UnsafePagedList<long>(16, Allocator.TempJob);

            long val = 0;
            long* longs = stackalloc long[5];
            for (long i = 0; i < 10; i++) {
                for (long j = 0; j < 5; j++) {
                    longs[j] = val++;
                }

                list.AddRangeToSamePage(longs, 5);

            }
            Assert.AreEqual(15, list.GetPage(0).size);
            Assert.AreEqual(15, list.GetPage(1).size);
            Assert.AreEqual(15, list.GetPage(2).size);
            Assert.AreEqual(5, list.GetPage(3).size);
            
            Assert.AreEqual(15, list[16]);
            
            list.Dispose();
        }

    }

}