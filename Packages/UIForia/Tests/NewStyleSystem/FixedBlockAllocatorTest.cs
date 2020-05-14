using NUnit.Framework;
using UIForia.Util.Unsafe;

namespace Tests {

    public unsafe class FixedBlockAllocatorTest {

        public struct SixteenBytes {

            private fixed byte bytes[16];

        }

        [Test]
        public void Allocates() {
            int blockSize = sizeof(SixteenBytes);
            int blocksPerPage = 4;

            FixedBlockAllocator alloc = new FixedBlockAllocator(blockSize, blocksPerPage, 1);

            SixteenBytes* a = alloc.Allocate<SixteenBytes>();
            SixteenBytes* b = alloc.Allocate<SixteenBytes>();
            SixteenBytes* c = alloc.Allocate<SixteenBytes>();
            SixteenBytes* d = alloc.Allocate<SixteenBytes>();

            Assert.IsTrue(a + 1 == b);
            Assert.IsTrue(b + 1 == c);
            Assert.IsTrue(c + 1 == d);

            Assert.IsTrue(alloc.freeList == null);

            alloc.Free(b);
            alloc.Free(c);
            alloc.Free(d);

            Assert.IsTrue(alloc.freeList != null);

            SixteenBytes* b2 = alloc.Allocate<SixteenBytes>();
            SixteenBytes* c2 = alloc.Allocate<SixteenBytes>();
            SixteenBytes* d2 = alloc.Allocate<SixteenBytes>();

            Assert.IsTrue(a + 1 == d2);
            Assert.IsTrue(d2 + 1 == c2);
            Assert.IsTrue(c2 + 1 == b2);

            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();
            alloc.Allocate<SixteenBytes>();

            Assert.AreEqual(alloc.pageListSize, 3);

            alloc.Dispose();

        }

    }

}