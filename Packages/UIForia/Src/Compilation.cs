using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Compilers {

    internal class Compilation {

        public Application application;
        public CompilationType compilationType;
        public ProcessedType entryType;
        public TemplateParseCache parseCache;
        
        public Dictionary<ProcessedType, UIModule> moduleMap;
        public Dictionary<ProcessedType, TemplateFileShell> templateMap;

        internal Diagnostics diagnostics;
        private HashSet<ProcessedType> resolvedTypeSet;
        private LightStack<ProcessedType> pendingTypeStack;
        private UIForiaMLParser templateParser;
        private UIModule defaultModule;

        public Compilation() {
            this.diagnostics = new Diagnostics();
            this.templateParser = new UIForiaMLParser(diagnostics);
            this.resolvedTypeSet = new HashSet<ProcessedType>();
            this.pendingTypeStack = new LightStack<ProcessedType>();
            this.templateMap = new Dictionary<ProcessedType, TemplateFileShell>();
        }

        private struct ModuleUsage {

            public string name;
            public UIModule module;

        }

        public void Compile() {
            TemplateCompiler2 templateCompiler = new TemplateCompiler2(this);

            templateCompiler.onTemplateCompiled += set => {
                Debug.Log(set.processedType.GetType() + " compiled");
                // todo -- consumer threads should pick up the results here
            };
            
            // need to go from type and resolve module, template shell, template
            templateCompiler.CompileTemplate(entryType);
            
        }

        public void ResolveTagNames() {
            Stopwatch sw = Stopwatch.StartNew();

            foreach (KeyValuePair<ProcessedType, UIModule> m in moduleMap) {
                if (m.Value.name == "UIForia") {
                    defaultModule = m.Value;
                    break;
                }
            }

            pendingTypeStack.Push(entryType);

            StructList<ModuleUsage> moduleUsages = new StructList<ModuleUsage>();
            StructStack<int> itemStack = new StructStack<int>();

            while (pendingTypeStack.size != 0) {
                ResolveTagNames(pendingTypeStack.Pop(), itemStack, moduleUsages);
            }

            Debug.Log("Resolved tags in: " + sw.Elapsed.TotalMilliseconds.ToString("F3"));
            diagnostics.Dump();

        }

        private void ResolveTagNames(ProcessedType type, StructStack<int> itemStack, StructList<ModuleUsage> moduleUsages) {
            itemStack.size = 0;
            moduleUsages.size = 0;
            resolvedTypeSet.Add(type);

            UIModule module = moduleMap[type];

            TemplateLookup lookup = new TemplateLookup() {
                elementLocation = type.elementPath,
                elementType = type.rawType,
                moduleLocation = module.location,
                modulePath = module.path,
                templatePath = type.templatePath,
                templateId = type.templateId
            };

            TemplateLocation templatePath = module.ResolveTemplate(lookup);

            if (!parseCache.TryGetTemplateFile(templatePath.filePath, out TemplateFileShell shell)) {
                // either file not found, not in cache, or write times don't match

                if (!File.Exists(templatePath.filePath)) {
                    diagnostics.LogError($"Cannot resolve template for type {type.rawType.GetTypeName()} in module `{module.name}`. Resolved module location was `{templatePath.filePath}` but the file did not exist.");
                    return;
                }

                string contents = File.ReadAllText(templatePath.filePath);
                DateTime timestamp = File.GetLastWriteTime(templatePath.filePath);

                if (!templateParser.TryParse(templatePath.filePath, contents, out shell)) {
                    return; // diagnostics handled inside parse method
                }

                shell.lastWriteTime = timestamp;
                shell.checkedTimestamp = true;
                parseCache.SetTemplateFile(templatePath.filePath, shell);
            }

            // I think this is the right place to check the parse cache and update our parse 
            // if failed to parse, log errors and skip traversal

            // with multiple templates per file we might have already mapped a different template in
            // this file and we dont want to overwrite our previous mapping
            shell.typeMappings = shell.typeMappings ?? new ProcessedType[shell.templateNodes.Length];
            TemplateASTRoot template = shell.GetRootTemplateForType(type);

            StructStack<int> stack = StructStack<int>.Get();

            templateMap[type] = shell;
            
            ref TemplateASTNode rootNode = ref shell.templateNodes[template.templateIndex];

            int ptr = shell.templateNodes[template.templateIndex].firstChildIndex;
            for (int i = 0; i < rootNode.childCount; i++) {
                stack.Push(ptr);
                ptr = shell.templateNodes[ptr].nextSiblingIndex;
            }

            while (stack.size > 0) {
                int idx = stack.PopUnchecked();

                TemplateASTNode node = shell.templateNodes[idx];

                if ((node.templateNodeType & TemplateNodeType.Slot) != 0) {
                    switch (node.templateNodeType) {
                        case TemplateNodeType.SlotDefine:
                            MapTypeToTemplateNode(shell, idx, TypeProcessor.GetSlotDefine());
                            break;
                        case TemplateNodeType.SlotForward:
                            MapTypeToTemplateNode(shell, idx, TypeProcessor.GetSlotForward());
                            break;
                        case TemplateNodeType.SlotOverride:
                            MapTypeToTemplateNode(shell, idx, TypeProcessor.GetSlotOverride());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (node.templateNodeType == TemplateNodeType.Meta) {
                    MapTypeToTemplateNode(shell, idx, TypeProcessor.GetMetaElement());
                }
                else {
                    CharSpan moduleName = shell.GetCharSpan(node.moduleNameRange);
                    CharSpan tagName = shell.GetCharSpan(node.tagNameRange);

                    // todo -- need to handle parsing generic tag syntax
                    // todo -- avoid toString with string trick
                    string tagNameString = tagName.ToString();

                    if (moduleName.HasValue) {
                        UIModule searchModule = ResolveModule(moduleUsages, module, moduleName);

                        if (searchModule == null) {
                            // keep going optimistically but dont start compiling
                            diagnostics.LogError($"Unable to resolve tag <{moduleName}:{tagName}> because module {moduleName} could not be resolved", shell.filePath, node.lineNumber, node.columnNumber);
                        }
                        else if (searchModule.tagNameMap.TryGetValue(tagNameString, out ProcessedType processedType)) {
                            MapTypeToTemplateNode(shell, idx, processedType);
                        }
                        else {
                            // keep going optimistically but dont start compiling
                            diagnostics.LogError($"Unable to resolve tag <{moduleName}:{tagName}> because module `{moduleName}` did not declare a tag `{tagName}`", shell.filePath, node.lineNumber, node.columnNumber);
                        }
                    }
                    else {
                        // need to know which modules to look in if no module name was provided
                        ProcessedType processedType = ResolveTagName(module, tagNameString);
                        if (processedType == null) {
                            diagnostics.LogError($"Unable to resolve tag <{tagName}> because module `{module.name}` did not declare a tag `{tagName}` and none of its referenced implicit modules did either", shell.filePath, node.lineNumber, node.columnNumber);
                        }
                        else {
                            MapTypeToTemplateNode(shell, idx, processedType);
                        }
                    }
                }

                int childNodeId = node.firstChildIndex;
                for (int i = 0; i < node.childCount; i++) {
                    stack.Push(childNodeId);
                    childNodeId = shell.templateNodes[childNodeId].nextSiblingIndex;
                }
            }

            StructStack<int>.Release(ref stack);
        }

        private void MapTypeToTemplateNode(TemplateFileShell shell, int idx, ProcessedType processedType) {
            
            shell.typeMappings[idx] = processedType;
            
            if (processedType.DeclaresTemplate) {
                if (resolvedTypeSet.Contains(processedType)) {
                    return;
                }

                bool found = false;

                for (int i = 0; i < pendingTypeStack.size; i++) {
                    if (pendingTypeStack.array[i] == processedType) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    pendingTypeStack.Push(processedType);
                }
            }
        }

        private UIModule ResolveModule(StructList<ModuleUsage> moduleUsages, UIModule source, in CharSpan moduleName) {
            for (int i = 0; i < moduleUsages.size; i++) {
                if (moduleUsages.array[i].name == moduleName) {
                    return moduleUsages.array[i].module;
                }
            }

            // todo -- support <Using module="" as=""/> aliasing

            foreach (KeyValuePair<ProcessedType, UIModule> kvp in moduleMap) {
                if (kvp.Value.name == moduleName) {
                    moduleUsages.Add(new ModuleUsage() {
                        name = moduleName.ToString(),
                        module = kvp.Value
                    });
                    return kvp.Value;
                }
            }

            return null;
        }

        private ProcessedType ResolveTagName(UIModule module, string tagName) {
            // look for the tag name in the module tag map
            // if didnt find it, walk down the list of implicit modules in declaration order for the module
            // if didnt find it, look in UIForia default
            // if didnt find it, fail
            if (module.tagNameMap.TryGetValue(tagName, out ProcessedType processedType)) {
                return processedType;
            }

            if (module.implicitModuleReferences != null) {
                for (int i = 0; i < module.implicitModuleReferences.Length; i++) {
                    if (module.implicitModuleReferences[i].tagNameMap.TryGetValue(tagName, out processedType)) {
                        return processedType;
                    }
                }
            }

            if (module != defaultModule) {
                if (defaultModule.tagNameMap.TryGetValue(tagName, out processedType)) {
                    return processedType;
                }
            }

            return null;
        }

    }

}