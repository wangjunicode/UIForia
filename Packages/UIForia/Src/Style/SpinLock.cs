using System.Threading;

// ReSharper disable RedundantJumpStatement

namespace UIForia {

    internal struct SpinLock {

        private const int k_LockFree = 0;
        private const int k_LockTaken = 1;

        public static void Lock(ref int lockVal) {
            int spinCount = 0;
            while (Interlocked.CompareExchange(ref lockVal, k_LockTaken, k_LockFree) != k_LockFree) {
// #if UNITY_BURST_EXPERIMENTAL_PAUSE_INTRINSIC
                //if (spinCount >= 10) {
                //    spinCount = 0;
                // Unity.Burst.Intrinsics.Common.Pause();
                //}

// #endif
                continue; // Spin while lock is occupied
            }

            UnityEngine.Assertions.Assert.IsTrue(lockVal == k_LockTaken);
        }

        public static void Unlock(ref int lockVal) {
            lockVal = k_LockFree;
        }

    }

}