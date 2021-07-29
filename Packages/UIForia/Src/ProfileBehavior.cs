using System;
using System.Threading;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    public unsafe class ProfileBehavior : MonoBehaviour {

        public int elementCount = 10000;
        private TestTypeLarge* list;

        public void Start() {
            list = TypedUnsafe.Malloc<TestTypeLarge>(elementCount, Allocator.Persistent);
        }

        ~ProfileBehavior() {
            TypedUnsafe.Dispose(list, Allocator.Persistent);
        }

        public void Update() {

            float result = 0;
            new Job() {
                output =  &result,
                list = list,
                elementCount = elementCount
            }.Run();

        }

        internal struct TestTypeSmall {

            public int value;

        }

        internal struct TestTypeLarge {

            public int value;

        }


        [BurstCompile(DisableSafetyChecks = false)]
        internal unsafe struct JobNativeArray : IJob {

            public NativeArray<int> array;
            [NativeDisableUnsafePtrRestriction] public int* retn;
            public void Execute() {
                int size = array.Length;
                for (int i = 0; i < size; i++) {
                    size -= array[i];
                }

                *retn = size;
            }

        }
        
        [BurstCompile(DisableSafetyChecks = false)]
        internal unsafe struct JobPointerArray : IJob {

            public int length;
            [NativeDisableUnsafePtrRestriction, NoAlias] public int* array;
            [NativeDisableUnsafePtrRestriction, NoAlias] public int* retn;
            public void Execute() {
                int size = length;
                for (int i = 0; i < size; i++) {
                    size -= array[i];
                }

                *retn = size;
            }

        }

        [BurstCompile]
        internal unsafe struct Job : IJob {

            public int elementCount;
            public int totalValue;

            [NativeDisableUnsafePtrRestriction] public float* output;

            [NativeDisableUnsafePtrRestriction] public TestTypeLarge* list;

            public void Execute() {
                float acc = 0;
                
                
                for (int i = 0; i < elementCount; i++) {
                    int val = (int) (math.sin((float) list[i].value) * math.cos((float) list[i].value));
                    
                    int initialValue, computedValue;
                    do {
                        // Save the current running total in a local variable.
                        initialValue = totalValue;

                        // Add the new value to the running total.
                        computedValue = initialValue + val;

                        // CompareExchange compares totalValue to initialValue. If
                        // they are not equal, then another thread has updated the
                        // running total since this loop started. CompareExchange
                        // does not update totalValue. CompareExchange returns the
                        // contents of totalValue, which do not equal initialValue,
                        // so the loop executes again.
                    } while (initialValue != Interlocked.CompareExchange(ref totalValue, computedValue, initialValue));
                }

                *output = acc;
            }

        }

    }

}