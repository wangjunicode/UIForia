using System;
using System.Collections.Generic;
using System.IO;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia {

    internal class TemplateParseCache {

        private string cacheFilePath;

        private Dictionary<string, TemplateFileShell> templateMap;

        internal TemplateParseCache() {
            templateMap = new Dictionary<string, TemplateFileShell>(128);
        }

        public void Initialize(bool forceReparse = false) {
            if (cacheFilePath != null) {
                return;
            }

            cacheFilePath = GetCacheFilePath();

            // todo -- fix hydrate bug
            if (true || forceReparse || !File.Exists(cacheFilePath)) {
                BuildCache();
            }
            else {
                HydrateCache();
            }
        }

        public bool TryGetTemplateFile(string filePath, out TemplateFileShell shell) {
            if (templateMap.TryGetValue(filePath, out shell)) {
                if (shell.checkedTimestamp) {
                    return true;
                }

                if (!File.Exists(shell.filePath)) {
                    shell = null;
                    templateMap.Remove(filePath);
                    return false;
                }

                DateTime lastWrite = File.GetLastWriteTime(shell.filePath);
                if (shell.lastWriteTime == lastWrite) {
                    shell.checkedTimestamp = true;
                }
                else {
                    shell = null;
                    templateMap.Remove(filePath);
                    return false;
                }

                return true;
            }

            return false;
        }

        private void HydrateCache() {
            byte[] bytes = File.ReadAllBytes(cacheFilePath);
            ManagedByteBuffer buffer = new ManagedByteBuffer(bytes);
            buffer.Read(out int templateCount);
            buffer.Read(out int version);

            if (version != UIForiaMLParser.Version) {
                BuildCache();
                return;
            }

            for (int i = 0; i < templateCount; i++) {
                TemplateFileShell shell = new TemplateFileShell();

                // must always deserialize so the buffer pointer knows how much data to skip
                shell.Deserialize(ref buffer);

                // could consider doing these validation checks when resolving the template instead of here
                // that reduces (maybe) the number of times we have to run the checks 
                // we should also check per compilation run that the file hasn't changed since we started compiling
            }

            if (buffer.ptr != buffer.array.Length) {
                Debug.LogWarning("Encountered an issue when loading UIForia templates from cache, reparsing files and rebuilding cache");
                BuildCache();
            }
        }

        private void BuildCache() {
            templateMap.Clear();
            // todo -- could improve search time by only finding module locations and looking only at those xml files

            Diagnostics diagnostics = new Diagnostics();
            IEnumerable<string> itr = Directory.EnumerateFiles(Path.GetFullPath(UnityEngine.Application.dataPath), "*.xml", SearchOption.AllDirectories);

            UIForiaMLParser parser = new UIForiaMLParser(diagnostics);

            LightList<string> list = new LightList<string>(128);
            LightList<string> contentList = new LightList<string>(128);
            LightList<DateTime> timestamps = new LightList<DateTime>(128);

            foreach (string file in itr) {
                string contents = File.ReadAllText(file);
                DateTime timestamp = File.GetLastWriteTime(file);

                if (UIForiaMLParser.IsProbablyTemplate(contents)) {
                    list.Add(file);
                    contentList.Add(contents);
                    timestamps.Add(timestamp);
                }
            }

            IEnumerable<string> itr2 = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages")), "*.xml", SearchOption.AllDirectories);

            foreach (string file in itr2) {
                string contents = File.ReadAllText(file);
                DateTime timestamp = File.GetLastWriteTime(file);
                if (UIForiaMLParser.IsProbablyTemplate(contents)) {
                    list.Add(file);
                    contentList.Add(contents);
                    timestamps.Add(timestamp);
                }
            }

            ManagedByteBuffer buffer = new ManagedByteBuffer(new byte[TypedUnsafe.Kilobytes(512)]);

            LightList<TemplateFileShell> files = new LightList<TemplateFileShell>(list.size);

            for (int i = 0; i < list.size; i++) {
                DateTime timestamp = timestamps.array[i];

                if (parser.TryParse(list.array[i], contentList.array[i], out TemplateFileShell result)) {
                    result.lastWriteTime = timestamp;
                    files.Add(result);
                    result.checkedTimestamp = true; // rebuilt -- assume we're fine
                }

                diagnostics.Clear();
            }

            buffer.Write(files.size);
            buffer.Write(UIForiaMLParser.Version);

            for (int i = 0; i < files.size; i++) {
                templateMap[files[i].filePath] = files[i];
                files.array[i].Serialize(ref buffer);
            }

            // WriteCacheResult since it didnt exist before or we are initializing
            using (FileStream fStream = new FileStream(cacheFilePath, FileMode.OpenOrCreate)) {
                fStream.Seek(0, SeekOrigin.Begin);
                fStream.Write(buffer.array, 0, buffer.ptr);
            }
        }

        private string GetCacheFilePath() {
            return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Temp", "__UIForiaTemplateCache__.bytes"));
        }

        public void SetTemplateFile(string filePath, TemplateFileShell shell) {
            templateMap[filePath] = shell;
        }

    }

}