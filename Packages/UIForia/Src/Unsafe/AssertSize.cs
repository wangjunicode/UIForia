using System;
using System.Collections.Generic;
using UIForia.Extensions;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public class AssertSize : Attribute {

        public readonly int assertedSize;

        public AssertSize(int size) {
            this.assertedSize = size;
        }

        [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
        public class SizeOfDependentType : Attribute {

            public Type type;
            public int expectedSize;

            public SizeOfDependentType(Type type, int expectedSize) {
                this.type = type;
                this.expectedSize = expectedSize;
            }

        }

        private static bool assertedSizes;


        public static void AssertSizes() {
#if UNITY_EDITOR
            return; // todo -- re-enable
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

            list = UnityEditor.TypeCache.GetTypesWithAttribute<SizeOfDependentType>();
            for (int i = 0; i < list.Count; i++) {
                Type type = list[i];
                IEnumerable<SizeOfDependentType> attrs = System.Reflection.CustomAttributeExtensions.GetCustomAttributes<SizeOfDependentType>(type);
                foreach (SizeOfDependentType attr in attrs) {
                    if (attr.expectedSize != UnsafeUtility.SizeOf(attr.type)) {
                        UnityEngine.Debug.LogError(type.GetTypeName() + $" expected {attr.type.GetTypeName()} to have a size of {attr.expectedSize} bytes but was actually {UnsafeUtility.SizeOf(attr.type)} bytes.");
                    }
                }
            }
#endif
        }

    }


}