using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UIForia.Util;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    public struct GatherStyleFiles_Managed : IJob {

        private GCHandle styleFileListHandle;
        private GCHandle moduleArrayHandle;
        private GCHandle appDataPathHandle;
        
        public GatherStyleFiles_Managed(Module[] moduleArray, StructList<StyleFile> styleFileList, string appDataPath) {
            styleFileListHandle = GCHandle.Alloc(styleFileList);
            moduleArrayHandle = GCHandle.Alloc(moduleArray);
            appDataPathHandle = GCHandle.Alloc(appDataPath);
        }

        public void Execute() {

            Module[] moduleList = (Module[]) moduleArrayHandle.Target;
            StructList<StyleFile> styleFileList = (StructList<StyleFile>) styleFileListHandle.Target;
            string appDataPath = (string)appDataPathHandle.Target;
            
            for (int i = 0; i < moduleList.Length; i++) {

                foreach (string file in Directory.EnumerateFiles(moduleList[i].location, "*.style", SearchOption.AllDirectories)) {
                    styleFileList.Add(new StyleFile() {
                        module = moduleList[i],
                        filePath = file,
                        lastWriteTime = File.GetLastWriteTime(file)
                    });
                }

            }

            string cachePathBase = Path.Combine(appDataPath, "UIForiaCache");
            
            Dictionary<string, CachedStyleSheet> cachedStyleSheets = new Dictionary<string, CachedStyleSheet>(styleFileList.size);
            
            for (int i = 0; i < styleFileList.size; i++) {
                string cacheFilePath = styleFileList.array[i].filePath.Substring(styleFileList.array[i].module.location.Length);
                cacheFilePath = Path.ChangeExtension(Path.Combine(cachePathBase, cacheFilePath), ".cachestyle");
                
                if (File.Exists(cacheFilePath)) {
                    
                    if (TryLoadJson(cacheFilePath, out CachedStyleSheet cachedStyleSheet)) {
                        
                        if (cachedStyleSheet.lastWriteTime == styleFileList.array[i].lastWriteTime) {
                            cachedStyleSheets.Add(cacheFilePath, cachedStyleSheet);
                            continue;
                        }
                        
                    }
                    
                }

                styleFileList.array[i].contents = File.ReadAllText(styleFileList.array[i].filePath);

            }

            for (int i = 0; i < styleFileList.size; i++) {
                
                // if not in cache we need to reparse
                // if in cache we need to validate all of its dependencies are also in cache
                
            }


            styleFileListHandle.Free();
            moduleArrayHandle.Free();
            appDataPathHandle.Free();
        }

        private bool TryLoadJson(string path, out CachedStyleSheet cachedStyleSheet) {
            try {
                cachedStyleSheet = JsonUtility.FromJson<CachedStyleSheet>(path);
                return true;
            }
            catch (Exception e) {
                Debug.Log(e);
                cachedStyleSheet = default;
                return false;
            }
        }

    }

}