using NUnit.Framework;
using UIForia.Util.Unsafe;
using Unity.Collections;

using UnityEngine;

namespace Tests.DataStructures {

    public class TestUnmanagedList {

        [Test]
        public unsafe void Add() {
            BufferList<Vector4> list = new BufferList<Vector4>(4, Allocator.TempJob);
            list.Add(new Vector4(0, 0, 0, 0));
            list.Add(new Vector4(0, 0, 0, 1));
            list.Add(new Vector4(0, 0, 0, 2));
            list.Add(new Vector4(0, 0, 0, 3));
            list.Add(new Vector4(0, 0, 0, 4));
            list.Add(new Vector4(0, 0, 0, 5));
            list.Add(new Vector4(0, 0, 0, 6));
            list.Add(new Vector4(0, 0, 0, 7));
            list.Add(new Vector4(0, 0, 0, 8));
            list.Add(new Vector4(0, 0, 0, 9));

            Assert.AreEqual(10, list.size);
            for (int i = 0; i < list.size; i++) {
                Assert.AreEqual(new Vector4(0, 0, 0, i), list.array[i]);
            }
            
            list.SwapRemoveAt(0);
            
            Assert.AreEqual(new Vector4(0, 0, 0, 9), list[0]);
            
            for (int i = 1; i < list.size; i++) {
                Assert.AreEqual(new Vector4(0, 0, 0, i), list.array[i]);
            }
            
            list.Dispose();
        }

    }

}