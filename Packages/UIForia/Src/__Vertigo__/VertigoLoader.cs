using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Util;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace UIForia {

    public struct CompileResult : IDisposable {

        public Diagnostics diagnostics;
        public StyleDatabase styleDatabase;
        public StringInternSystem internSystem;
        public Dictionary<Type, TemplateData> templateDataMap;

        public void Dispose() {
            styleDatabase?.Dispose();
            internSystem?.Dispose();
        }

    }

    public static unsafe class VertigoLoader {

        private struct DisposedCompileData : IDisposable {

            public GCHandleArray<Module> moduleArrayHandle;
            public GCHandleArray<TemplateFileShell> templateFiles;
            
            public GCHandleList<StyleFile> mergedStyleFileList;
            public GCHandleList<TemplateData> templateDataHandle;
            public GCHandleList<TemplateExpressionSet> compiledTemplateHandle;
            
            public PerThreadList<StyleFile> perThread_StyleFiles;
            public PerThreadObject<Diagnostics> perThread_Diagnostics;
            
            public GCHandle<string> baseCachePath;
            public GCHandle<Diagnostics> gatheredDiagnostics;
   
            public HeapAllocated<int> parseCount;

            public void Dispose() {
                moduleArrayHandle.Dispose();
                templateFiles.Dispose();
                mergedStyleFileList.Dispose();
                perThread_StyleFiles.Dispose();
                perThread_Diagnostics.Dispose();
                baseCachePath.Dispose();
                gatheredDiagnostics.Dispose();
                templateDataHandle.Dispose();
                compiledTemplateHandle.Dispose();
            }

        }

        public static bool AllowCaching {
            get => false; // module types changed, module conditions changed, styles regenerated
        }

        public static bool Compile(Type entryType, CompilationType compileType, out CompileResult compileResult) {
            ModuleSystem.Initialize();

            Diagnostics diagnostics = new Diagnostics();
            if (!entryType.IsSubclassOf(typeof(UIElement))) {
                diagnostics.LogError($"Cannot compile entry type {entryType.GetTypeName()} because it is not a subclass of {typeof(UIElement).GetTypeName()}");
                compileResult = new CompileResult() {
                    diagnostics = diagnostics,
                };
                return false;
            }

            // todo -- ensure has [EntryPoint] 

            ProcessedType processedEntryType = TypeProcessor.GetProcessedType(entryType);

            if (processedEntryType == null) {
                diagnostics.LogError($"Cannot find processed type for entry type {entryType.GetTypeName()}");
                compileResult = new CompileResult() {
                    diagnostics = diagnostics,
                };
                return false;
            }

            if (processedEntryType.IsUnresolvedGeneric) {
                diagnostics.LogError($"Cannot use an open generic type as an application entry point. {entryType.GetTypeName()} is invalid.");
                compileResult = new CompileResult() {
                    diagnostics = diagnostics,
                };
                return false;
            }

            if (processedEntryType.module == null) {
                diagnostics.LogError($"{entryType.GetTypeName()} had no module associated with it.");
                compileResult = new CompileResult() {
                    diagnostics = diagnostics,
                };
                return false;
            }

            Module[] moduleList = processedEntryType.module.GetFlattenedDependencyTree();

            TemplateFileShell[] templatesToParse = PopulateTemplateList(moduleList);
            LightList<StyleFile> styleFiles = new LightList<StyleFile>(64);

            DisposedCompileData compileData = new DisposedCompileData() {
                moduleArrayHandle = new GCHandleArray<Module>(moduleList),
                templateFiles = new GCHandleArray<TemplateFileShell>(templatesToParse),
                mergedStyleFileList = new GCHandleList<StyleFile>(styleFiles),
                perThread_StyleFiles = new PerThreadList<StyleFile>(JobsUtility.MaxJobThreadCount),
                perThread_Diagnostics = new PerThreadObject<Diagnostics>(JobsUtility.MaxJobThreadCount),
                baseCachePath = new GCHandle<string>(Path.Combine(UnityEngine.Application.dataPath, "UIForiaCache")),
                gatheredDiagnostics = new GCHandle<Diagnostics>(diagnostics),
                parseCount = new HeapAllocated<int>(0)
            };

            JobHandle compileStyles = VertigoScheduler.ParallelFor(new GatherStyleFiles_Managed() {
                    parallel = new ParallelParams(moduleList.Length, 1),
                    baseCachePath = compileData.baseCachePath,
                    moduleListHandle = compileData.moduleArrayHandle,
                    perThread_styleFileList = compileData.perThread_StyleFiles
                })
                .Then(new MergeStyleFileLists_Managed() {
                    perThread_styleFileList = compileData.perThread_StyleFiles,
                    mergeResult = compileData.mergedStyleFileList,
                    lengthResult = compileData.parseCount
                })
                .ThenDeferParallel(new ParseStyleFiles_Managed() {
                    defer = new ParallelParams.Deferred(compileData.parseCount, 5),
                    styleFileListHandle = compileData.mergedStyleFileList,
                    perThread_DiagnosticsHandle = compileData.perThread_Diagnostics
                })
                .ThenDeferParallel(new CompileStyleFiles_Managed() {
                    defer = new ParallelParams.Deferred(compileData.parseCount, 5),
                    styleFileListHandle = compileData.mergedStyleFileList,
                    perThread_DiagnosticsHandle = compileData.perThread_Diagnostics
                });

            // maybe setup style db here, but would include all styles from referenced modules, not just the used ones
            // which might be desirable anyway tbh

            JobHandle parseTemplates = VertigoScheduler.Parallel(new LoadTemplates_Managed() {
                    parallel = new ParallelParams(templatesToParse.Length, 5),
                    perThread_diagnosticsList = compileData.perThread_Diagnostics,
                    fileShells = compileData.templateFiles,
                    cachePathBase = compileData.baseCachePath
                })
                .ThenParallel(new ValidateTemplates_Managed() {
                    parallel = new ParallelParams(templatesToParse.Length, 5),
                    perThread_diagnostics = compileData.perThread_Diagnostics,
                    templateFileArray = compileData.templateFiles
                });

            JobHandle gatherDiagnostics = VertigoScheduler.Await(parseTemplates, compileStyles).Then(new GatherDiagnostics_Managed() {
                perThread_diagnostics = compileData.perThread_Diagnostics,
                gathered = compileData.gatheredDiagnostics
            });

            // sync point!
            JobHandle.CompleteAll(ref parseTemplates, ref compileStyles, ref gatherDiagnostics);

            // could kick off the write back jobs now for parse caching instead of writing as soon as it completes parsing

            // start compiling if we dont have any problems

            bool allModulesValid = !diagnostics.HasErrors();

            if (!allModulesValid) {
                compileData.Dispose();
                compileResult = new CompileResult() {
                    diagnostics = diagnostics,
                };
                return false;
            }

            StringInternSystem internSystem = new StringInternSystem();
            StyleDatabase styleDatabase = new StyleDatabase(internSystem);

            styleDatabase.Initialize(styleFiles);

            if (compileType == CompilationType.Dynamic) {
                
            }
            else {
                
            }
            LightList<TemplateData> compiledTemplateData = CompileTemplates(processedEntryType, ref compileData);

            bool applicationValid = !diagnostics.HasErrors();

            compileData.Dispose();

            if (!applicationValid) {
                internSystem.Dispose();
                styleDatabase.Dispose();
                compileResult = new CompileResult() {
                    diagnostics = diagnostics
                };
                return false;
            }

            // todo -- replace with array lookup by type index (does make caching templates harder, but can still cache bindings which is better anyway)

            Dictionary<Type, TemplateData> templateDataMap = new Dictionary<Type, TemplateData>(compiledTemplateData.size);

            for (int i = 0; i < compiledTemplateData.size; i++) {
                templateDataMap.Add(compiledTemplateData.array[i].type, compiledTemplateData.array[i]);
            }

            compileResult = new CompileResult() {
                diagnostics = diagnostics,
                internSystem = internSystem,
                styleDatabase = styleDatabase,
                templateDataMap = templateDataMap
            };

            return true;

        }

        private static TemplateFileShell[] PopulateTemplateList(Module[] moduleList) {
            int count = 0;
            for (int i = 0; i < moduleList.Length; i++) {
                count += moduleList[i].templateShells.size;
            }

            int size = 0;
            TemplateFileShell[] retn = new TemplateFileShell[count];

            for (int i = 0; i < moduleList.Length; i++) {

                Array.Copy(moduleList[i].templateShells.array, 0, retn, size, moduleList[i].templateShells.size);
                size += moduleList[i].templateShells.size;
            }

            return retn;
        }

        private static LightList<TemplateData> CompileTemplates(ProcessedType processedEntryType, ref DisposedCompileData compileData) {

            TemplateCompiler2 compiler = new TemplateCompiler2();

            LightList<TemplateExpressionSet> compileTemplateList = new LightList<TemplateExpressionSet>(64);
            LightList<TemplateData> templateDataList = new LightList<TemplateData>(64);

            GCHandleList<TemplateExpressionSet> compiledTemplateHandle = new GCHandleList<TemplateExpressionSet>(compileTemplateList);
            GCHandleList<TemplateData> templateDataHandle = new GCHandleList<TemplateData>(templateDataList);
            PerThreadObject<Diagnostics> diagnosticsHandle = compileData.perThread_Diagnostics;

            compileData.compiledTemplateHandle = compiledTemplateHandle;
            compileData.templateDataHandle = templateDataHandle;

            StructList<JobHandle> handles = new StructList<JobHandle>();

            int templateCount = 5;
            Action<TemplateExpressionSet> callback = (set) => {

                compileTemplateList.Add(set);

                // job will write into this slot, just make sure list can hold it
                templateDataList.Add(default);

                JobHandle compileHandle = new CompileBuiltTemplates_Managed() {
                    compiledTemplateHandle = compiledTemplateHandle,
                    outputList = templateDataHandle,
                    templateIndex = compileTemplateList.size - 1,
                    perThread_diagnostics = diagnosticsHandle
                }.Schedule();

                if (templateCount++ >= 5) {
                    JobHandle.ScheduleBatchedJobs();
                    templateCount = 0;
                }

                handles.Add(compileHandle);

            };

            compiler.onTemplateCompiled += callback;

            compiler.CompileTemplate(processedEntryType);

            compiler.onTemplateCompiled -= callback;

            NativeArray<JobHandle> handleArray = new NativeArray<JobHandle>(handles.size, Allocator.Temp);

            fixed (JobHandle* handleptr = handles.array) {
                UnsafeUtility.MemCpy(handleArray.GetUnsafePtr(), handleptr, sizeof(JobHandle) * handles.size);
            }

            JobHandle.CompleteAll(handleArray);

            new GatherDiagnostics_Managed() {
                perThread_diagnostics = compileData.perThread_Diagnostics,
                gathered = compileData.gatheredDiagnostics
            }.Execute();

            handleArray.Dispose();

            return templateDataList;

        }

        public static void LoadPrecompiled(VertigoApplication vertigoApplication, Type entryPoint) { }

    }

}