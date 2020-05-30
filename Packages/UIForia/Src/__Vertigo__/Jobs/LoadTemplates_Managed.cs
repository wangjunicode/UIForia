using System;
using System.IO;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia {

    public struct LoadTemplates_Managed : IVertigoParallel {

        public GCHandleArray<TemplateFileShell> fileShells;
        public PerThreadObject<Diagnostics> perThread_diagnosticsList;
        public GCHandle<string> cachePathBase;

        [NativeSetThreadIndex] public int threadIndex;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, fileShells.Get().Length);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            string cacheRoot = cachePathBase.Get();
            TemplateFileShell[] templateFiles = fileShells.Get();
            Diagnostics diagnostics = perThread_diagnosticsList.GetForThread(threadIndex);
            
            TemplateParser parser = TemplateParser.GetParserForFileType("xml");
            TemplateFileShellBuilder builder = new TemplateFileShellBuilder();

            for (int i = start; i < end; i++) {
                TemplateFileShell templateFile = templateFiles[i];

                string filePath = templateFiles[i].filePath;
                string cacheFilePath = Path.Combine(cacheRoot, filePath.Substring(templateFile.module.location.Length));

                if (!File.Exists(filePath)) {
                    diagnostics.LogError("Unable to find template file at path: " + filePath);
                    continue;
                }

                DateTime writeTime = File.GetLastWriteTime(filePath);

                if (VertigoLoader.AllowCaching && File.Exists(cacheFilePath)) {

                    DateTime cacheWriteTime = File.GetLastWriteTime(cacheFilePath);

                    if (cacheWriteTime > writeTime) {

                        try {
                            byte[] bytes = File.ReadAllBytes(cacheFilePath);
                            ManagedByteBuffer buffer = new ManagedByteBuffer(bytes);
                            templateFile.Deserialize(ref buffer);
                            continue;
                        }
                        catch (Exception e) {
                            Debug.Log(e);
                        }

                    }

                }

                string contents = File.ReadAllText(filePath);

                parser.Setup(filePath, diagnostics);

                // todo -- if we have inline style node that needs to be parsed here as well!!!!
                // but i probably can't compile it until later, maybe leave it for the template compiler?
                
                templateFile.successfullyParsed = parser.TryParse(contents, builder);

                if (templateFile.successfullyParsed) {
                    builder.Build(templateFile);
                }

                parser.Reset();
            }

        }

    }

}