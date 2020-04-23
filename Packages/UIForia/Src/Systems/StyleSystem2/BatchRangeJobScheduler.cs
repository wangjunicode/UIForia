using System;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    public unsafe static class BatchRangeJobScheduler {

        private static readonly int s_ProcessorCount = SystemInfo.processorCount;

        public static void Schedule<T>(int minBatchSize, T job, JobHandle dependencies = default) where T : struct, IJob {
            Schedule(s_ProcessorCount * 2, minBatchSize, job, dependencies);
        }

        public static void Schedule<T>(int maxJobCount, int minBatchSize, T job, JobHandle dependencies = default) where T : struct, IJob {
            int fieldOffset = OffsetCache<T>.GetOffset();

            if (fieldOffset != 0) {
                throw new Exception($"Tried to schedule BatchRangeJob but 'rangeJobData' was missing or was not at offset 0. It must be defined as {typeof(BatchRangeJobData)} and must be at offset 0");
            }

            for (int i = 0; i < maxJobCount; i++) {
                T data = job; // copy the input struct
                BatchRangeJobData* ptr = (BatchRangeJobData*) UnsafeUtility.AddressOf(ref data);
                BatchRangeJobData jobData = new BatchRangeJobData(i, minBatchSize, maxJobCount);
                *ptr = jobData;
                data.Schedule(dependencies);
            }
        }
        
        private struct OffsetCache<T> {

            // ReSharper disable once StaticMemberInGenericType
            private static int offset = -1;

            public static int GetOffset() {
#if UNITY_EDITOR
                if (offset < 0) {
                    FieldInfo fieldInfo = typeof(T).GetField("rangeJobData", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fieldInfo == null) {
                        offset = -1;
                        return -1;
                    }

                    offset = UnsafeUtility.GetFieldOffset(fieldInfo);
                }

                return offset;
#else
                    return 0;
#endif
            }

        }

  
    }

}