using System;
using System.Collections.Generic;
using UIForia.Parsing;

namespace UIForia {

    internal struct TemplateCache {

        internal struct FileInfo {

            public string path;
            public string contents;
            public TemplateShell templateShell;
            public DateTime lastWriteTime;
            public Module module;

        }

        internal readonly FileInfo[] cache;

        public TemplateCache(HashSet<ResolvedTemplateLocation> resolvedLocations, TemplateCache oldCache) {
            cache = new FileInfo[resolvedLocations.Count];

            int idx = 0;
            if (oldCache.cache == null) {
                foreach (ResolvedTemplateLocation location in resolvedLocations) {
                    cache[idx++] = new FileInfo() {
                        path = location.filePath,
                        module = location.module,
                        lastWriteTime = default
                    };
                }
            }
            else {
                foreach (ResolvedTemplateLocation location in resolvedLocations) {

                    if (oldCache.TryGetFileInfo(location.filePath, out FileInfo fileInfo)) {
                        cache[idx++] = fileInfo;
                    }
                    else {
                        cache[idx++] = new FileInfo() {
                            path = location.filePath
                        };
                    }
                }
            }

            Array.Sort(cache, (a, b) => string.CompareOrdinal(a.path, b.path));
        }

        private int BinarySearchForPath(string path) {
            int start = 0;
            int end = cache.Length - 1;

            while (start <= end) {
                int index1 = start + (end - start >> 1);

                int cmp = string.CompareOrdinal(cache[index1].path, path);

                if (cmp == 0) {
                    return index1;
                }

                if (cmp < 0) {
                    start = index1 + 1;
                }
                else {
                    end = index1 - 1;
                }
            }

            return ~start;
        }

        internal bool TryGetFileInfo(string path, out FileInfo fileInfo) {
            int idx = BinarySearchForPath(path);
            if (idx >= 0) {
                fileInfo = cache[idx];
                return true;
            }

            fileInfo = default;
            return false;
        }

        private static readonly FileInfoComparer s_Searcher = new FileInfoComparer();

        private class FileInfoComparer : IComparer<FileInfo> {

            public int Compare(FileInfo x, FileInfo y) {
                return string.CompareOrdinal(x.path, y.path);
            }

        }

        public bool ContentsChanged(string path) {
            // if (cache.TryGetValue(path, out FileInfo info)) {
            //     return info.lastWriteTime == File.GetLastWriteTime(path);
            // }

            return true;
        }

        // public T GetTemplateSource<T>() {
        //     
        // }

        public void SetFileSource(string filePath, object data) {
            // cache[filePath] = new FileInfo() {
            //     lastWriteTime = File.GetLastWriteTime(filePath),
            //     data = data
            // };
        }

    }

}