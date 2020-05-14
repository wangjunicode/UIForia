using NUnit.Framework;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace Tests.DataStructures {

    public class TestUnmanagedPagedList {

        [Test]
        public unsafe void AddRange() {
            PagedList<Vector4> list = new PagedList<Vector4>(4, Allocator.TempJob);

            Vector4* v = stackalloc Vector4[13];
            
            v[0] = new Vector4(0, 0, 0, 0);
            v[1] = new Vector4(0, 0, 0, 1);
            v[2] = new Vector4(0, 0, 0, 2);
            v[3] = new Vector4(0, 0, 0, 3);
            v[4] = new Vector4(0, 0, 0, 4);
            v[5] = new Vector4(0, 0, 0, 5);
            v[6] = new Vector4(0, 0, 0, 6);
            v[7] = new Vector4(0, 0, 0, 7);
            v[8] = new Vector4(0, 0, 0, 8);
            v[9] = new Vector4(0, 0, 0, 9);
            v[10] = new Vector4(0, 0, 0, 10);
            v[11] = new Vector4(0, 0, 0, 11);
            v[12] = new Vector4(0, 0, 0, 12);
            
            RangeInt range = list.AddRange(v, 4);
            
            Assert.AreEqual(1, list.pageCount);
            Assert.AreEqual(4, list.pageSize);
            
            Assert.AreEqual(0, range.start);
            Assert.AreEqual(4, range.end);
            
            Assert.AreEqual(v[0], list[0]);
            Assert.AreEqual(v[1], list[1]);
            Assert.AreEqual(v[2], list[2]);
            Assert.AreEqual(v[3], list[3]);
            
            range = list.AddRange(&v[4], 4);
            Assert.AreEqual(4, range.start);
            Assert.AreEqual(8, range.end);
            
            Assert.AreEqual(2, list.pageCount);

            for (int i = 0; i < 8; i++) {
                Assert.AreEqual(v[i], list[i]);
            }
            
            range = list.AddRange(&v[8], 2);
            Assert.AreEqual(8, range.start);
            Assert.AreEqual(10, range.end);
            
            Assert.AreEqual(3, list.pageCount);
            
            // add new page since we don't fit on the same one
            range = list.AddRange(&v[10], 3);
            Assert.AreEqual(12, range.start);
            Assert.AreEqual(15, range.end);
            
            Assert.AreEqual(4, list.pageCount);
            
            Assert.AreEqual(v[10], list[12]);
            Assert.AreEqual(v[11], list[13]);
            Assert.AreEqual(v[12], list[14]);
            
            list.Dispose();
        }
        
           [Test]
        public unsafe void AddRangeCompact() {
            PagedList<Vector4> list = new PagedList<Vector4>(4, Allocator.TempJob);

            Assert.AreEqual(4, list.pageSize);

            Vector4* v = stackalloc Vector4[13];
            
            v[0] = new Vector4(0, 0, 0, 0);
            v[1] = new Vector4(0, 0, 0, 1);
            v[2] = new Vector4(0, 0, 0, 2);
            v[3] = new Vector4(0, 0, 0, 3);
            v[4] = new Vector4(0, 0, 0, 4);
            v[5] = new Vector4(0, 0, 0, 5);
            v[6] = new Vector4(0, 0, 0, 6);
            v[7] = new Vector4(0, 0, 0, 7);
            v[8] = new Vector4(0, 0, 0, 8);
            v[9] = new Vector4(0, 0, 0, 9);
            v[10] = new Vector4(0, 0, 0, 10);
            v[11] = new Vector4(0, 0, 0, 11);
            v[12] = new Vector4(0, 0, 0, 12);
            
            RangeInt range = list.AddRangeCompact(v, 2);
            
            Assert.AreEqual(1, list.pageCount);
            
            Assert.AreEqual(0, range.start);
            Assert.AreEqual(2, range.end);
            
            Assert.AreEqual(v[0], list[0]);
            Assert.AreEqual(v[1], list[1]);
            
            range = list.AddRangeCompact(&v[2], 2);
            Assert.AreEqual(2, range.start);
            Assert.AreEqual(4, range.end);
            
            Assert.AreEqual(1, list.pageCount);

            for (int i = 0; i < 4; i++) {
                Assert.AreEqual(v[i], list[i]);
            }

            range = list.AddRangeCompact(&v[4], 3);
            Assert.AreEqual(4, range.start);
            Assert.AreEqual(7, range.end);
            
            Assert.AreEqual(2, list.pageCount);
            
            range = list.AddRangeCompact(&v[8], 2);
            Assert.AreEqual(8, range.start);
            Assert.AreEqual(10, range.end);
            Assert.AreEqual(3, list.pageCount);
            
            range = list.AddRangeCompact(&v[12], 1);
            Assert.AreEqual(7, range.start);
            Assert.AreEqual(8, range.end);
            Assert.AreEqual(3, list.pageCount);
            
            list.Dispose();
        }

    }

}