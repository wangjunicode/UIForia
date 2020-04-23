using System;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public class AssertSize : Attribute {

        public readonly int assertedSize;

        public AssertSize(int size) {
            this.assertedSize = size;
        }

        private static bool assertedSizes;

#if UNITY_EDITOR

        public static void AssertSizes() {
            if (assertedSizes) return;
            assertedSizes = true;
            UnityEditor.TypeCache.TypeCollection list = UnityEditor.TypeCache.GetTypesWithAttribute<AssertSize>();
            for (int i = 0; i < list.Count; i++) {
                Type type = list[i];
                AssertSize attr = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<AssertSize>(type);
                if (attr.assertedSize != UnsafeUtility.SizeOf(type)) {
                    UnityEngine.Debug.LogError(type.GetTypeName() + $" was supposed to have a size of {attr.assertedSize} bytes but was actually {UnsafeUtility.SizeOf(type)} bytes.");
                }
            }
        }

#endif

    }

}