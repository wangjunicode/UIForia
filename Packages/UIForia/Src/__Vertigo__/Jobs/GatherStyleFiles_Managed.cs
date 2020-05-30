using System.IO;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public struct GatherStyleFiles_Managed : IJob, IJobParallelFor, IVertigoParallelFor {

        public PerThreadList<StyleFile> perThread_styleFileList;
        public GCHandleArray<Module> moduleListHandle;
        public GCHandle<string> baseCachePath;

        [NativeSetThreadIndex] public int threadIndex;

        public ParallelParams parallel { get; set; }
        
        public void Run(Module[] moduleList, LightList<StyleFile> styleFileList, int start, int end) {

            string cachePathBase = baseCachePath.Get();

            for (int i = start; i < end; i++) {

                foreach (string file in Directory.EnumerateFiles(moduleList[i].location, "*.style", SearchOption.AllDirectories)) {

                    string cacheFilePath = file.Substring(moduleList[i].location.Length);
                    cacheFilePath = Path.Combine(cachePathBase, cacheFilePath);
                    styleFileList.Add(new StyleFile() {
                        module = moduleList[i],
                        filePath = file,
                        lastWriteTime = File.GetLastWriteTime(file),
                        parseCachePath = Path.ChangeExtension(cacheFilePath, ".parsecache"),
                        compileCachePath = Path.ChangeExtension(cacheFilePath, ".compilecache"),
                    });
                }

            }

        }

        public void Execute() {

            ref LightList<StyleFile> styleFileList = ref perThread_styleFileList.GetForThread(threadIndex);

            Module[] moduleList = moduleListHandle.Get();

            Run(moduleList, styleFileList, 0, moduleList.Length);

        }

        public void Execute(int index) {
            ref LightList<StyleFile> styleFileList = ref perThread_styleFileList.GetForThread(threadIndex);
            Module[] moduleList = moduleListHandle.Get();

            if (styleFileList == null) styleFileList = new LightList<StyleFile>();

            Run(moduleList, styleFileList, index, index + 1);

        }


    }

}