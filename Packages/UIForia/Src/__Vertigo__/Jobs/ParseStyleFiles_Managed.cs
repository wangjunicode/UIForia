using System;
using System.IO;
using UIForia.NewStyleParsing;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    public struct ParseStyleFiles_Managed : IJob, IVertigoParallelDeferred {

        public GCHandleList<StyleFile> styleFileListHandle;
        public PerThreadObject<Diagnostics> perThread_DiagnosticsHandle;

        [NativeSetThreadIndex] public int threadIndex;

        public ParallelParams.Deferred defer { get; set; }

        private void Run(int start, int end) {

            LightList<StyleFile> styleFileList = styleFileListHandle.Get();
            Diagnostics diagnostics = perThread_DiagnosticsHandle.GetForThread(threadIndex);
            
            StyleSheetParser3 parser = new StyleSheetParser3();

            ManagedByteBuffer byteBuffer = new ManagedByteBuffer(new byte[8 * 1024]);

            for (int i = start; i < end; i++) {

                ref StyleFile file = ref styleFileList.array[i];

                bool hasCacheFile = !string.IsNullOrEmpty(file.parseCachePath) && File.Exists(file.parseCachePath);

                if (hasCacheFile) {

                    DateTime writeTime = File.GetLastWriteTime(file.parseCachePath);

                    // todo -- might have an in memory copy also, no need to deserialize in that case

                    // is it enough to check that we last wrote after the last parse? I think it might be
                    if (writeTime > file.lastWriteTime) {
                        try {
                            file.parseResult = ParsedStyleFile.Deserialize(File.ReadAllBytes(file.parseCachePath));
                            continue;
                        }
                        catch (Exception e) {
                            Debug.Log(e);
                        }
                    }

                }

                file.contents = File.ReadAllText(file.filePath);
                ParseResult result = parser.TryParseFile(file, diagnostics, out file.parseResult);
                
                if (result == ParseResult.Success) {

                    try {
                        if (!VertigoLoader.AllowCaching) {
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(file.parseCachePath));
                        
                        using (FileStream fileStream = new FileStream(file.parseCachePath, FileMode.OpenOrCreate, FileAccess.Write)) {
                            byteBuffer.ptr = 0;
                            file.parseResult.Serialize(ref byteBuffer);
                            fileStream.Write(byteBuffer.array, 0, byteBuffer.ptr);
                        }
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                    }

                }
                else {
                    // delete it
                    if (hasCacheFile) {
                        File.Delete(file.parseCachePath);
                    }
                }

            }

        }

        public void Execute() {
            Run(0, styleFileListHandle.Get().size);
        }

        public void Execute(int start, int end) {
            Run(start, end);
        }

    }

}