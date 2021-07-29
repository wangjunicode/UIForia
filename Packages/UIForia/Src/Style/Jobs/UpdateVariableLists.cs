using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal struct UpdateVariableLists : IJob {

        public CheckedArray<TraversalInfo> traversalInfo;
        public CheckedArray<ValueVariable> valueVariables;
        public CheckedArray<ColorVariable> colorVariables;
        public CheckedArray<TextureVariable> textureVariables;

        public void Execute() {

            HandleVariables(valueVariables);
            HandleVariables(colorVariables);
            HandleVariables(textureVariables);

        }

        private unsafe void HandleVariables<T>(CheckedArray<T> variables) where T : unmanaged, IStyleVariable, IComparable<T> {

            for (int i = 0; i < variables.size; i++) {

                ref T variable = ref variables.Get(i);

                variable.TraversalInfo = traversalInfo[variable.ElementId.index];

            }

            NativeSortExtension.Sort(variables.GetArrayPointer(), variables.size);
        }

    }

}