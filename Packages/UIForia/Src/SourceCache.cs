using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UIForia.Util;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace UIForia.Src {

    internal class SourceCache {

        private readonly HashSet<string> sourceCacheSet;
        private readonly StructList<FileInfo> sourceCache;
        private readonly LightList<string> pending;

        public SourceCache() {
            this.sourceCacheSet = new HashSet<string>();
            this.pending = new LightList<string>();
            this.sourceCache = new StructList<FileInfo>();
        }
            
        public void Ensure(string path) {
            if (sourceCacheSet.Add(path)) {
                pending.Add(path);
            }
        }

        public FileInfo Get(string path) {
            int start = 0;
            int end = sourceCache.size - 1;

            while (start <= end) {
                int index1 = start + (end - start >> 1);

                int cmp = String.CompareOrdinal(sourceCache.array[index1].path, path);

                if (cmp == 0) {
                    return sourceCache.array[index1];
                }

                if (cmp < 0) {
                    start = index1 + 1;
                }
                else {
                    end = index1 - 1;
                }
            }

            return new FileInfo() {
                missing = true,
                path = path,
            };
        }

        public void FlushPendingUpdates() {
                
            sourceCache.EnsureAdditionalCapacity(pending.size);
            for (int i = 0; i < pending.size; i++) {
                sourceCache.AddUnsafe(new FileInfo() {
                    path = pending.array[i]
                });
            }

            sourceCache.Sort((a, b) => string.CompareOrdinal(a.path, b.path));
                
            pending.Clear();
                
            SourceCacheJob job = new SourceCacheJob() {
                handle = GCHandle.Alloc(sourceCache)
            };
            
            job.Schedule(sourceCache.size, 1).Complete();
            job.handle.Free();
            
        }
            
        private struct SourceCacheJob : IJobParallelFor {

            public GCHandle handle;
            
            public void Execute(int index) {
                
                StructList<FileInfo> fileInfos = (StructList<FileInfo>) handle.Target;
                
                ref FileInfo fileInfo = ref fileInfos.array[index];

                if (!File.Exists(fileInfo.path)) {
                    Debug.LogError($"Unable to find template file at {fileInfo.path}");
                    fileInfo.missing = true;
                    return;
                }
                
                DateTime lastWriteTime = File.GetLastWriteTime(fileInfo.path);

                if (lastWriteTime == fileInfo.lastWriteTime) {
                    return;
                }
                
                string contents = File.ReadAllText(fileInfo.path);

                fileInfo.contents = contents;
                fileInfo.missing = false;
                fileInfo.lastWriteTime = lastWriteTime;

            }

        }

    }

}