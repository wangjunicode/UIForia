using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal struct RemoveDeadVariables : IJob {

        public CheckedArray<RuntimeTraversalInfo> metaInfo;

        public DataList<ValueVariable>.Shared valueVariables;
        public DataList<ColorVariable>.Shared colorVariables;
        public DataList<TextureVariable>.Shared textureVariables;

        public void Execute() {

            HandleVariables(valueVariables);
            HandleVariables(colorVariables);
            HandleVariables(textureVariables);

        }

        private void HandleVariables<T>(DataList<T>.Shared variables) where T : unmanaged, IStyleVariable {

            for (int i = 0; i < variables.size; i++) {
                ElementId elementId = variables[i].ElementId;
                if (!ElementSystem.IsAlive(elementId, metaInfo[elementId.index])) {
                    variables[i--] = variables[variables.size - 1];
                    variables.size--;
                }
            }

        }

    }

}