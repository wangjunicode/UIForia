using System.Diagnostics;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public static class BurstAssert {

        // based on: https://jacksondunstan.com/articles/5292
        [Conditional("UNITY_ASSERTIONS")]
        public static void IsTrue(bool condition, in FixedString128 message) {
#if UNITY_ASSERTIONS
            if (!condition) {
                Debug.LogError(message);
            }
#endif
        }

    }

}