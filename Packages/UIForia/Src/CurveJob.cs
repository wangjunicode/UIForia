using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct CurveJob : IJob {

        public DataList<AnimationOptions> optionTable;
        public DataList<DataList<KeyFrameResult>> resultBuffer;

        public void Execute() {

            for (int i = 0; i < resultBuffer.size; i++) {
                for (int j = 0; j < resultBuffer[i].size; j++) {
                    ref KeyFrameResult result = ref resultBuffer.Get(i).array[j];
                    ref AnimationOptions option = ref optionTable.array[result.optionId.id];
                    result.t = Easing.Interpolate(result.t, option.easingFunction, ref option.bezier);
                }
            }

        }

    }

}