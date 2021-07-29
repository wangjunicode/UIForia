using System;
using System.Collections.Generic;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UIForia.Style;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using IntBoolMap = UIForia.Util.IntBoolMap;

namespace UIForia {

    public unsafe struct TestThing : IComparable<TestThing> {

        public ElementId elementId;
        public int pid;

        public TestThing(PropertyId propertyId, int state, int src) : this() { }

        public int CompareTo(TestThing other) {
            return elementId.index - other.elementId.index;
        }

    }

    public struct ColorUpdate : IComparable<ColorUpdate> {

        public int index;
        public Color color;

        public int CompareTo(ColorUpdate other) {
            return index.CompareTo(other.index);
        }

    }

    public struct UpdateValue : IComparable<UpdateValue> {

        public ElementId elementId;
        public int dbLocation;

        public int CompareTo(UpdateValue other) {
            return elementId.CompareTo(other.elementId);
        }

    }

    public unsafe struct UpdateList {

        public int size;
        public int capacity;
        public UpdateValue* values;
        public ulong* elementMap;

    }

    [BurstCompile(CompileSynchronously = true)]
    public unsafe struct BenchmarkJob2 : IJob {

        public NativeArray<TestThing> entries;
        [NativeDisableUnsafePtrRestriction] public ulong* changeMap;
        public int mapSize;
        public int iterations;
        public NativeArray<TestThing> output;
        public NativeArray<UpdateList> updates;

        public void Execute() { }

    }

    [BurstCompile(CompileSynchronously = true)]
    public unsafe struct BenchmarkJob : IJob {

        public NativeArray<TestThing> entries;
        public NativeArray<bool> changeBools;
        [NativeDisableUnsafePtrRestriction] public ulong* changeMap;
        [NativeDisableUnsafePtrRestriction] public ulong* vMap;
        public int mapSize;
        public int iterations;
        public NativeArray<TestThing> output;
        public NativeArray<UpdateList> updates;

        public void Execute() {

            for (int i = 0; i < updates.Length; i++) {

                UpdateValue* values = updates[i].values;
                int length = updates[i].size;

                for (int x = 0; x < length; x++) {
                    if (changeBools[values[x].elementId.index]) {
                        values[x--] = values[--length];
                    }
                }

                //for (int x = 0; x < length; x++) {
                //    int idx = values[x].elementId.id;
                //    int mapIdx = idx >> 6; // divide by 64
                //    int shift = (idx - (mapIdx << 6)); // multiply by 64
                //    if ((changeMap[mapIdx] & ((1ul << shift))) != 0) {
                //        values[x--] = values[--length];
                //    }
                //}

            }
            // for (int i = 0; i < 150; i++) {
            //     int len = entries.Length / 150;
            //     TestThing* elementPtr = ptr + (i * len);
            //     for (int x = 0; x < len; x++) {
            //         if (elementMap.Get(elementPtr[x].elementId)) {
            //             elementPtr[x--] = elementPtr[--len];
            //         }
            //     }
            // }

            // for (int x = 0; x < iterations; x++) {
            // int length = entries.Length;
            // for (int i = 0; i < length; i++) {
            //
            //     if (elementMap.Get(ptr[i].elementId)) {
            //         // output[addIdx++] = ptr[i];
            //         ptr[i--] = ptr[--length];
            //     }
            //
            // }
            // }

            // insertion sort vs native sort threshold is ~30 properties 
            // for (int i = 0; i < length; i++) {
            //     
            //     if (ptr[i].elementIndex == 8) {
            //         ptr[i--] = ptr[--length];
            //     }
            //
            //     *removedCount = entries.Length - length;
            //
            // }

            // 60% faster than NativeSort for this this use case when property count <= 35
            // private static void InsertionSort(TestThing* values, int n) {
            //
            //     for (int i = 1; i < n; i++) {
            //
            //         long key = values[i].value;
            //
            //         int j = i - 1;
            //
            //         while (j >= 0 && values[j].value > key) {
            //             values[j + 1] = values[j];
            //             j--;
            //         }
            //
            //         values[j + 1].value = key;
            //     }
            // }

        }

    }

}