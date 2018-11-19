using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Assembly = System.Reflection.Assembly;

namespace UIForia {

    public static class TypeProcessor {

        private static readonly Dictionary<string, ProcessedType> typeMap = new Dictionary<string, ProcessedType>();
        private static List<Assembly> filteredAssemblies;
        private static List<Type> loadedTypes;
        private static Type[] templateTypes;
        
        private static void FilterAssemblies() {
            if (filteredAssemblies != null) return;
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            filteredAssemblies = new List<Assembly>();
            loadedTypes = new List<Type>();

            for (int i = 0; i < assemblies.Length; i++) {
                Assembly assembly = assemblies[i];

                if (assembly == null) {
                    continue;
                }
                
                if (!FilterAssembly(assembly)) continue;

                filteredAssemblies.Add(assembly);
                Type[] types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++) {
                    // can be null if assembly referenced is unavailable
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (types[j] != null) {
                        loadedTypes.Add(types[j]);
                    }
                }
            }
            
            loadedTypes.Add(typeof(Color));
            
        }

        public static ProcessedType GetType(string typeName, List<ImportDeclaration> importPaths = null) {
            FilterAssemblies();
            if (typeMap.ContainsKey(typeName)) {
                return typeMap[typeName];
            }

            for (int i = 0; i < TemplateParser.IntrinsicElementTypes.Length; i++) {
                if (typeName == TemplateParser.IntrinsicElementTypes[i].name) {
                    return new ProcessedType(TemplateParser.IntrinsicElementTypes[i].type);
                }    
            }
            
            Type type = Type.GetType(typeName);

            if (type == null) {
                for (int i = 0; i < loadedTypes.Count; i++) {
                    if (loadedTypes[i].Name == typeName) {
                        type = loadedTypes[i];
                        break;
                    }
                }
            }

            Assert.IsNotNull(type, $"type != null, unable to find type {typeName}");

            return GetType(type);
        }

        public static Type GetRuntimeType(string typeName) {
            FilterAssemblies();

            Type type = Type.GetType(typeName);

            if (type == null) {
                for (int i = 0; i < loadedTypes.Count; i++) {
                    if (loadedTypes[i].FullName.EndsWith(typeName)) {
                        type = loadedTypes[i];
                        break;
                    }
                }
            }

            return type;
        }

        public static Type GetStyleExportType(string typeName) {
            FilterAssemblies();

            Type type = Type.GetType(typeName);

            if (type == null) {
                for (int i = 0; i < loadedTypes.Count; i++) {
                    if (loadedTypes[i].Name.EndsWith(typeName)) {
                        type = loadedTypes[i];
                        break;
                    }
                }
            }

            return type;
        }
        
        public static ProcessedType GetType(Type type) {
            ProcessedType processedType = new ProcessedType(type);
            typeMap[type.Name] = processedType;
            return processedType;
        }

        private static bool FilterAssembly(Assembly assembly) {
            string name = assembly.FullName;
            
            if (assembly.IsDynamic
                || name.StartsWith("System,")
                || name.StartsWith("nunit")
                || name.Contains("Unity")
                || name.StartsWith("System.")
                || name.StartsWith("Microsoft.")
                || name.StartsWith("Mono")
                || name.Contains("mscorlib")
                || name.Contains("Jetbrains")) {
                return false;
            }

            return name.IndexOf("-firstpass", StringComparison.Ordinal) == -1;
        }

        public static Type[] GetTemplateTypes() {
            if (templateTypes == null) {
                FilterAssemblies();
                List<Type> types = new List<Type>();
                for (int i = 0; i < loadedTypes.Count; i++) {
                    if (typeof(UIElement).IsAssignableFrom(loadedTypes[i])) {

                        if (loadedTypes[i].Assembly.FullName.Contains("UIForia.Demo")) {
                            types.Add(loadedTypes[i]);   
                            continue;
                        }
                        
                        if (loadedTypes[i].Assembly.FullName.Contains("UIForia")) {
                            continue;
                        }
                        
                        types.Add(loadedTypes[i]);

                    }
                }

                templateTypes = types.ToArray();
            }

            return templateTypes;
        }

    }

}