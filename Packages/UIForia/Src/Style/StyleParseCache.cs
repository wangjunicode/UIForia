using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia.Compilers {

    internal class StyleParseCache {

        private string cacheFilePath;
        public Dictionary<string, StyleFileShell> styleMap;
        private StyleFileShell[] fileList;
        private LightList<StyleFileShell> files;

        internal StyleParseCache() {
            this.styleMap = new Dictionary<string, StyleFileShell>(128);
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

        public bool TryGetStyle(string filePath, out StyleFileShell shell) {
            if (styleMap.TryGetValue(filePath, out shell)) {
                if (shell.checkedTimestamp) {
                    return true;
                }

                if (!File.Exists(shell.filePath)) {
                    shell = null;
                    styleMap.Remove(filePath);
                    return false;
                }

                DateTime lastWrite = File.GetLastWriteTime(shell.filePath);
                if (shell.lastWriteTime == lastWrite) {
                    shell.checkedTimestamp = true;
                }
                else {
                    shell = null;
                    styleMap.Remove(filePath);
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

            if (version != UIForiaStyleParser.Version) {
                BuildCache();
                return;
            }

            for (int i = 0; i < templateCount; i++) {
                // TemplateFileShell shell = new TemplateFileShell();

                // must always deserialize so the buffer pointer knows how much data to skip
                // shell.Deserialize(ref buffer);

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
            styleMap.Clear();
            Diagnostics diagnostics = new Diagnostics();

            IEnumerable<string> itr = Directory.EnumerateFiles(Path.GetFullPath(UnityEngine.Application.dataPath), "*.style", SearchOption.AllDirectories);

            UIForiaStyleParser parser = new UIForiaStyleParser(diagnostics);

            LightList<string> list = new LightList<string>(128);
            LightList<string> contentList = new LightList<string>(128);
            LightList<DateTime> timestamps = new LightList<DateTime>(128);

            foreach (string file in itr) {
                string contents = File.ReadAllText(file);
                DateTime timestamp = File.GetLastWriteTime(file);
                list.Add(file);
                contentList.Add(contents);
                timestamps.Add(timestamp);
            }

            IEnumerable<string> itr2 = Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages")), "*.style", SearchOption.AllDirectories);

            foreach (string file in itr2) {
                string contents = File.ReadAllText(file);
                DateTime timestamp = File.GetLastWriteTime(file);
                list.Add(file);
                contentList.Add(contents);
                timestamps.Add(timestamp);

            }

            files = new LightList<StyleFileShell>(list.size);
            for (int i = 0; i < list.size; i++) {
                DateTime timestamp = timestamps.array[i];

                if (parser.TryParse(list.array[i], contentList.array[i], out StyleFileShell result)) {
                    result.lastWriteTime = timestamp;
                    files.Add(result);
                    result.checkedTimestamp = true; // rebuilt -- assume we're fine
                }
                else {
                    result.isValid = false;
                    files.Add(result);
                }

            }
            diagnostics.Dump();
            // diagnostics.Clear();

            // todo -- serialize style file & read them back

            // ManagedByteBuffer buffer = new ManagedByteBuffer(new byte[TypedUnsafe.Kilobytes(512)]);
            // buffer.Write(files.size);
            // buffer.Write(UIForiaMLParser.Version);

            for (int i = 0; i < files.size; i++) {
                styleMap[files[i].filePath] = files[i];
                // files.array[i].Serialize(ref buffer);
            }

            // WriteCacheResult since it didnt exist before or we are initializing
            // using (FileStream fStream = new FileStream(cacheFilePath, FileMode.OpenOrCreate)) {
            //     fStream.Seek(0, SeekOrigin.Begin);
            //     fStream.Write(buffer.array, 0, buffer.ptr);
            // }
        }

        private string GetCacheFilePath() {
            return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Temp", "__UIForiaStyleCache__.bytes"));
        }

        public void Set(string locationFilePath, StyleFileShell shell) {
            styleMap[locationFilePath] = shell;
        }

        
        public void GetFilesInModule(string path, LightList<StyleFileShell> output) {

            for (int i = 0; i < files.size; i++) {
                if (files.array[i].filePath.StartsWith(path, StringComparison.Ordinal)) {
                    output.Add(files.array[i]);
                }
            }

        }

    }

}