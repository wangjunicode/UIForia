using Unity.Jobs;

namespace UIForia {

    public unsafe struct BuildFinalStyles : IJobParallelForBatch, IJob {

        public StyleResultTable sharedTable;
        public StyleResultTable instanceTable;
        public StyleResultTable selectorTable;
        
        public void Execute(int startIndex, int count) {
            
        }

        public void Execute() {
            
        }

        private void Run(int start, int count) {

            int end = start + count;
            
            for (int buildIndex = start; buildIndex < end; buildIndex++) {
                
                
                
            }
            
        }

    }

}