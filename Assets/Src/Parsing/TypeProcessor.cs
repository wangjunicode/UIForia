using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine.Assertions;
using Assembly = System.Reflection.Assembly;

namespace Src {

    public static class TypeProcessor {

        private static readonly Dictionary<string, ProcessedType> typeMap = new Dictionary<string, ProcessedType>();
        private static List<Assembly> filteredAssemblies;
        private static List<Type> loadedTypes;

        private static void FilterAssemblies() {
            if (filteredAssemblies != null) return;
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            filteredAssemblies = new List<Assembly>();
            loadedTypes = new List<Type>();

            for (int i = 0; i < assemblies.Length; i++) {
                Assembly assembly = assemblies[i];

                if (!FilterAssembly(assembly)) continue;
                
                filteredAssemblies.Add(assembly);
                loadedTypes.AddRange(assembly.GetExportedTypes());
            }
            
        }

        public static ProcessedType GetType(string typeName, List<ImportDeclaration> importPaths = null) {
            FilterAssemblies();
            if (typeMap.ContainsKey(typeName)) {
                return typeMap[typeName];
            }

            Type type = Type.GetType(typeName);

            if (type == null) {
                for (int i = 0; i < loadedTypes.Count; i++) {
                    if (loadedTypes[i].Name.EndsWith(typeName)) {
                        type = loadedTypes[i];
                        break;
                    }
                }
            }

            Assert.IsNotNull(type, $"type != null, unable to find type {typeName}");

            return GetType(type);
        }

        public static ProcessedType GetType(Type type) {
            ProcessedType processedType = new ProcessedType(type);
            typeMap[type.Name] = processedType;
            return processedType;
        }

        private static bool FilterAssembly(Assembly assembly) {
            string name = assembly.FullName;
            
            if (assembly.IsDynamic
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

    }

}