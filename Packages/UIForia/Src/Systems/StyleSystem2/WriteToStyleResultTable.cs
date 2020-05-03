using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct WriteToStyleResultTable : IJob {

        public StyleResultTable targetTable;
        public StyleRebuildResultList writeList;

        public void Execute() {
            // this could eventually be multithreaded if the allocator can handle it
        }

    }

}