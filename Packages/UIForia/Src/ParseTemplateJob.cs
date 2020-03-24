using System;
using System.IO;
using System.Runtime.InteropServices;
using UIForia.Parsing;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Src {

    internal struct ParseTemplateJob : IJobParallelFor {

        public GCHandle handle;

        public ParseTemplateJob(object data) {
            handle = GCHandle.Alloc(data);
        }
            
        public void Execute(int index) {
            
            TemplateCache.FileInfo[] cache = (TemplateCache.FileInfo[]) handle.Target;
            
            ref TemplateCache.FileInfo fileInfo = ref cache[index];

            string templateFilePath = fileInfo.path;

            if (!File.Exists(templateFilePath)) {
                Debug.LogError($"Unable to find template file at {templateFilePath}");
                return;
            }

            DateTime lastWriteTime = File.GetLastWriteTime(templateFilePath);

            if (lastWriteTime == fileInfo.lastWriteTime) {
                return;
            }

            TemplateParser parser = TemplateParser.GetParserForFileType("xml");

            string contents = File.ReadAllText(templateFilePath);

            TemplateShell templateShell = new TemplateShell(fileInfo.module, templateFilePath);

            parser.OnSetup();
            
            if (parser.TryParse(contents, templateShell)) {
                fileInfo.templateShell = templateShell;
                fileInfo.lastWriteTime = lastWriteTime;
                fileInfo.contents = contents;
            }
            
            parser.OnReset();
        }

    }

}