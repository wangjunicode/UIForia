using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Compilers;
using UIForia.Exceptions;
using UIForia.Style2;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class KlangWindowModule : Module { }

    public abstract class Module {

        public Action INTERNAL_DO_NOT_CALL; // use this for precompiled loading instead of doing type reflection to find caller type
        
        private LightList<StyleSheet2> styleSheets;
        private ModuleReference[] moduleReferences;

        private static string GetFilePath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            return sourceFilePath;
        }

        public void GetStylePaths() {

            string path = GetFilePath();

            IList<string> filepaths = new List<string>();
            Debug.Log("Source: " + path);

            foreach (string file in Directory.EnumerateFiles(path, "*.style", SearchOption.AllDirectories)) {
                Debug.Log(file);
                filepaths.Add(file);
            }

        }

        internal static bool CreateRootModule<T>() where T : Module {

            IList<ModuleReference> references = new List<ModuleReference>();

            Module root = Activator.CreateInstance<T>();

            Dictionary<Type, Module> moduleTypes = new Dictionary<Type, Module>();

            GatherDependencies(root);

            DependencySort(root);
            
            // make sure no module has overlapping paths

            return true;

            void GatherDependencies(Module m) {

                m.DependenciesResolved = true;

                IList<ModuleReference> dependencies = m.GetCachedDependencies();

                for (int i = 0; i < dependencies.Count; i++) {

                    Type moduleType = dependencies[i].GetModuleType();

                    if (!moduleTypes.TryGetValue(moduleType, out Module instance)) {
                        instance = (Module) Activator.CreateInstance(moduleType);
                        moduleTypes[moduleType] = instance;
                    }

                    dependencies[i].ResolveModule(instance);

                    if (!instance.DependenciesResolved) {
                        GatherDependencies(instance);
                    }

                }

            }

        }

        private bool DependenciesResolved { get; set; }

        private VisitMark visitedMark;

        private enum VisitMark {

            Alive,
            Dead,
            Undead

        }

        public static List<Module> DependencySort(Module root) {

            int count = CountSizeAndReset(root, 1);

            List<Module> sorted = new List<Module>(count);

            IList<ModuleReference> dependencies = root.GetCachedDependencies();

            for (int i = 0; i < dependencies.Count; i++) {
                Visit(dependencies[i].GetModuleInstance(), sorted);
            }

            return sorted;
        }

        private static int CountSizeAndReset(Module module, int count) {
            module.visitedMark = VisitMark.Alive;

            IList<ModuleReference> dependencies = module.GetCachedDependencies();

            for (int i = 0; i < dependencies.Count; i++) {
                count += CountSizeAndReset(dependencies[i].GetModuleInstance(), count);
            }

            return count;

        }

        private static void Visit(Module module, List<Module> sorted) {

            if (module.visitedMark == VisitMark.Dead) {
                return;
            }

            if (module.visitedMark == VisitMark.Undead) {
                throw new ArgumentException("Cyclic dependency found."); // probably need a stack to track this properly
            }

            module.visitedMark = VisitMark.Undead;

            IList<ModuleReference> dependencies = module.GetCachedDependencies();

            for (int i = 0; i < dependencies.Count; i++) {
                Visit(dependencies[i].GetModuleInstance(), sorted);
            }

            module.visitedMark = VisitMark.Undead;

            sorted.Add(module);
        }

        private List<bool> conditionResults;
        private IList<StyleCondition> styleConditions;

        // import const from KlangWindowModule;
        // import module:KlangWindowModule as window;
        // import {name, name, name} from 'path/to/file';

        // export style styleName {}

        // export style styleName2 {}

        // export mixin mixinName {}

        // style definitions that aren't part of uiforia core need to be generated somehow
        // ideally without re-compiling uiforia core

        // uiforia.core -> ship as dll
        // uiforia.runtime -> ship as dll, rely on on style
        // uiforia.style -> regenerated with style properties
        // uiforia.util 

        public IList<ModuleReference> GetDependencies() {
            return new ModuleReference[] {
                new ModuleReference<KlangWindowModule>("alias"),
                new ModuleReference<Module>(),
            };
        }

        private IList<ModuleReference> cachedDependencies;

        internal IList<ModuleReference> GetCachedDependencies() {
            if (cachedDependencies == null) {
                cachedDependencies = GetDependencies();

                if (cachedDependencies == null) {
                    cachedDependencies = new ModuleReference[0];
                }

                for (int i = 0; i < cachedDependencies.Count; i++) {
                    if (cachedDependencies[i].GetModuleType().IsAbstract) {
                        throw new InvalidArgumentException("Dependency declared on an abstract type " + TypeNameGenerator.GetTypeName(cachedDependencies[i].GetModuleType()));
                    }
                }

                if (cachedDependencies.Count >= 2) {
                    // todo -- pool
                    List<Type> references = new List<Type>(cachedDependencies.Count);

                    for (int i = 0; i < cachedDependencies.Count; i++) {

                        if (references.Contains(cachedDependencies[i].GetModuleType())) {
                            throw new InvalidArgumentException($"Duplicate dependency declared on {TypeNameGenerator.GetTypeName(cachedDependencies[i].GetModuleType())}");
                        }

                        references.Add(cachedDependencies[i].GetModuleType());

                    }
                }

            }

            return cachedDependencies;

        }

        public virtual bool EvaluateStyleCondition(string conditionName) {
            return false;
        }

        public virtual string GetTemplatePath(string path, string callerPath) {
            return path;
        }

        public virtual string GetStylePath(string path, string callerPath) {
            return path;
        }

        public void UpdateConditions(DisplayConfiguration displayConfiguration) {
            if (styleConditions == null) return;

            conditionResults = conditionResults ?? new List<bool>();
            conditionResults.Clear();

            for (int i = 0; i < styleConditions.Count; i++) {
                conditionResults.Add(styleConditions[i].fn(displayConfiguration));
            }
        }

        public List<bool> GetDisplayConditions() {
            return conditionResults;
        }

        public void RegisterDisplayCondition(string condition, Func<DisplayConfiguration, bool> fn) {
            if (styleConditions == null) {
                styleConditions = new List<StyleCondition>();
                styleConditions.Add(new StyleCondition(0, condition, fn));
                return;
            }

            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == condition) {
                    throw new Exception("Duplicate DisplayCondition '" + condition + "'");
                }
            }

            styleConditions.Add(new StyleCondition(styleConditions.Count, condition, fn));
        }

        public int GetDisplayConditionId(CharSpan conditionSpan) {
            if (styleConditions == null) return -1;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionSpan) {
                    return i;
                }
            }

            return -1;
        }

        public bool HasStyleCondition(string conditionName) {
            return HasStyleCondition(new CharSpan(conditionName));
        }

        public bool HasStyleCondition(CharSpan conditionName) {
            if (styleConditions == null) return false;
            for (int i = 0; i < styleConditions.Count; i++) {
                if (styleConditions[i].name == conditionName) {
                    return true;
                }
            }

            return false;
        }

    }

    public abstract class ModuleReference {

        public abstract Type GetModuleType();

        private Module module;

        public void ResolveModule(Module module) {
            this.module = module;
        }

        public Module GetModuleInstance() {
            return module;
        }

    }

    public class ModuleReference<T> : ModuleReference where T : Module {

        public readonly string alias;

        public ModuleReference(string alias = null) {
            this.alias = alias;
        }

        public override Type GetModuleType() {
            return typeof(T);
        }

    }

}