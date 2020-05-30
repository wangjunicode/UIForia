using System;
using System.IO;
using UIForia.NewStyleParsing;
using UIForia.Rendering;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {
    
    public struct CompileStyleFiles_Managed : IJob, IVertigoParallelDeferred {

        public PerThreadObject<Diagnostics> perThread_DiagnosticsHandle;
        public GCHandleList<StyleFile> styleFileListHandle;

        [NativeSetThreadIndex] public int threadIndex;

        public ParallelParams.Deferred defer { get; set; }

        private void Run(int start, int end) {
            LightList<StyleFile> styleFileList = styleFileListHandle.Get();
            Diagnostics diagnostics = perThread_DiagnosticsHandle.GetForThread(threadIndex);

            StyleFileCompiler compiler = new StyleFileCompiler();

            for (int i = start; i < end; i++) {

                ref StyleFile file = ref styleFileList.array[i];

                if (file.parseResult == null) {
                    continue;
                }
                
                ManagedByteBuffer writeBuffer = default;

                bool hasCacheFile = !string.IsNullOrEmpty(file.compileCachePath) && File.Exists(file.compileCachePath);

                if (VertigoLoader.AllowCaching && hasCacheFile) {

                    DateTime writeTime = File.GetLastWriteTime(file.compileCachePath);

                    // todo -- might have an in memory copy also, no need to deserialize in that case

                    // is it enough to check that we last wrote after the last parse? I think it might be
                    if (writeTime > file.lastWriteTime) {
                        try {
                            file.compileResult = CompiledStyleFile.Deserialize(new ManagedByteBuffer(File.ReadAllBytes(file.compileCachePath)));
                            continue;
                        }
                        catch (Exception e) {
                            Debug.Log(e);
                            File.Delete(file.compileCachePath);
                        }
                    }

                }

                if (compiler.TryCompile(file.parseResult, diagnostics, out file.compileResult) && VertigoLoader.AllowCaching) {
                    try {
                        // todo -- consider doing this later async
                        using (FileStream fileStream = new FileStream(file.compileCachePath, FileMode.OpenOrCreate, FileAccess.Write)) {

                            writeBuffer.ptr = 0;

                            if (writeBuffer.array == null) {
                                writeBuffer.array = new byte[1024 * 8];
                            }

                            file.compileResult.Serialize(ref writeBuffer);
                            fileStream.Write(writeBuffer.array, 0, writeBuffer.ptr);
                        }

                        continue;
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                    }
                }

                if (hasCacheFile) {
                    File.Delete(file.compileCachePath);
                }

            }

            compiler.Dispose();

        }

        public void Execute() {
            Run(0, styleFileListHandle.Get().size);
        }

        public void Execute(int start, int end) {
            Run(start, end);
        }

    }

}