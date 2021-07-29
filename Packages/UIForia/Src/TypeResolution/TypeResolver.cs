using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UIForia.Extensions;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Debug = UnityEngine.Debug;

namespace UIForia.Compilers {

    public static class TypeResolver {

        private static Dictionary<string, LightList<Assembly>> s_NamespaceMap;
        private static Stats stats;

        [ThreadStatic] private static string[] s_SingleNamespace;

        public struct Stats {

            public int namespaceCount;
            public int assemblyCount;
            public int typeCount;
            public double namespaceScanTime;

        }

        public static Stats Initialize() {
            BuildNamespaceMap();
            return stats;
        }

        public struct AssemblyTypes {

            public const int Version = 1;

            public string assembly;
            public Guid guid;
            public string[] namespaces;

        }

        private static bool GetNamespaces(Assembly assembly, LightList<string> namespaceList, AssemblyTypes[] assemblyTypes) {
            try {
                namespaceList.size = 0;
                string assemblyFullName = assembly.FullName;
                for (int j = 0; j < assemblyTypes.Length; j++) {
                    if (assemblyTypes[j].assembly == assemblyFullName) {

                        if (assemblyTypes[j].guid != assembly.ManifestModule.ModuleVersionId) {
                            Debug.Log($"Assembly {assembly.GetName().Name} changed, rebuilding namespace map for this assembly."); // todo -- diagnostics
                            break;
                        }

                        namespaceList.AddRange(assemblyTypes[j].namespaces);
                        return false;
                    }
                }

                Type[] types = assembly.GetExportedTypes();

                for (int j = 0; j < types.Length; j++) {
                    Type currentType = types[j];

                    // can be null if assembly referenced is unavailable
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (currentType == null) {
                        continue;
                    }

                    stats.typeCount++;

                    string typeNamespace = currentType.Namespace;

                    bool found = false;
                    for (int x = 0; x < namespaceList.size; x++) {
                        if (namespaceList.array[x] == typeNamespace) {
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        namespaceList.Add(typeNamespace);
                    }

                }
            }
            catch (ReflectionTypeLoadException e) {
                Debug.LogError($"{assembly.FullName} -> {e.Message}");
            }

            return true;
        }

        private static void BuildNamespaceMap() {
            if (s_NamespaceMap != null) {
                stats.namespaceScanTime = 0;
                return;
            }

            s_NamespaceMap = new Dictionary<string, LightList<Assembly>>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            string cachePath = GetCacheFilePath();

            ManagedByteBuffer buffer;
            AssemblyTypes[] assemblyTypes = null;
            bool needsUpdate = false;

            if (File.Exists(cachePath)) {

                using (FileStream fileStream = new FileStream(cachePath, FileMode.Open)) {
                    byte[] bytes = new byte[(int) (fileStream.Length * 1.25)];
                    buffer = new ManagedByteBuffer(bytes);
                    fileStream.Read(bytes, 0, (int) fileStream.Length);
                    buffer.Read(out int c);
                    buffer.Read(out int version);

                    if (version == AssemblyTypes.Version) {
                        assemblyTypes = new AssemblyTypes[c];
                        for (int i = 0; i < c; i++) {
                            buffer.Read(out assemblyTypes[i].assembly);
                            buffer.Read(out assemblyTypes[i].guid);
                            buffer.Read(out assemblyTypes[i].namespaces);
                        }
                    }
                    else {
                        Debug.Log("Namespace map version changed, rebuilding.");
                    }

                    buffer.ptr = 0;
                }
            }
            else {
                needsUpdate = true;
                buffer = new ManagedByteBuffer(new byte[TypedUnsafe.Megabytes(1)]);
                Debug.Log("Type resolve cache missing, running full resolve");
            }

            if (assemblyTypes == null) {
                assemblyTypes = new AssemblyTypes[0];
            }

            LightList<string> namespaces = new LightList<string>(128);
            Stopwatch watch = Stopwatch.StartNew();

            buffer.Write(0); // patched later
            buffer.Write(AssemblyTypes.Version);

            int count = 0;
            foreach (Assembly assembly in assemblies) {
                stats.assemblyCount++;

                if (assembly.IsDynamic || assembly.ReflectionOnly) {
                    continue;
                }

                count++;

                needsUpdate ^= GetNamespaces(assembly, namespaces, assemblyTypes);

                AssemblyTypes output = new AssemblyTypes();

                output.namespaces = new string[namespaces.size];
                output.guid = assembly.ManifestModule.ModuleVersionId;
                output.assembly = assembly.FullName;

                for (int i = 0; i < namespaces.size; i++) {
                    string namespaceName = namespaces.array[i] ?? "null";
                    output.namespaces[i] = namespaceName;

                    if (s_NamespaceMap.TryGetValue(namespaceName, out LightList<Assembly> assemblyList)) {
                        assemblyList.Add(assembly);
                    }
                    else {
                        assemblyList = new LightList<Assembly>();
                        assemblyList.Add(assembly);
                        s_NamespaceMap[namespaceName] = assemblyList;
                    }
                }

                buffer.Write(output.assembly);
                buffer.Write(output.guid);
                buffer.Write(output.namespaces);
            }

            if (needsUpdate) {
                // patch the first value to the the count
                int ptr = buffer.ptr;
                buffer.ptr = 0;
                buffer.Write(count);
                buffer.ptr = ptr;
                new Thread(() => {
                    // write the result back async since we don't need it to continue loading
                    using (FileStream fStream = new FileStream(cachePath, FileMode.OpenOrCreate)) {
                        fStream.Seek(0, SeekOrigin.Begin);
                        fStream.Write(buffer.array, 0, buffer.ptr);
                    }
                }).Start();
            }

            stats.namespaceScanTime = watch.Elapsed.TotalMilliseconds;

        }

        private static string GetCacheFilePath() {
            return PathUtil.GetTempFilePath("__UIForiaNamespaceCache__.bytes");
        }

        public static Type ResolveTypeExpression(Type invokingType, IList<string> namespaces, string typeExpression, StructList<GenericResolution> genericResolutions = null) {

            if (TypeParser.TryParseTypeName(typeExpression, out TypeLookup typeLookup)) {
                return ResolveType(typeLookup, (IReadOnlyList<string>) namespaces, invokingType, genericResolutions);
            }

            return null;
        }

        public static Type ResolveType(string typeName) {
            s_SingleNamespace ??= new string[1];
            s_SingleNamespace[0] = string.Empty;
            return ResolveType(new TypeLookup(typeName), s_SingleNamespace);
        }

        public static Type ResolveType(string typeName, string namespaceName,  StructList<GenericResolution> genericResolutions = null) {
            s_SingleNamespace ??= new string[1];
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(new TypeLookup(typeName), s_SingleNamespace, null, genericResolutions);
        }

        public static Type ResolveType(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null, StructList<GenericResolution> genericResolutions = null) {
            if (typeLookup.resolvedType != null) {
                return typeLookup.resolvedType;
            }

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces, genericResolutions);

            if (typeLookup.generics.array != null && typeLookup.generics.size != 0) {
                if (!baseType.IsGenericTypeDefinition) {
                    throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
                }

                baseType = ReflectionUtil.CreateGenericType(baseType, ResolveGenericTypes(typeLookup, namespaces, scopeType));
            }

            if (typeLookup.isArray) {
                baseType = baseType.MakeArrayType();
            }

            return baseType;
        }
        
        public static Type ResolveTypeDeclaration(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            if (typeLookup.resolvedType != null) {
                return typeLookup.resolvedType;
            }

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces, null);

            if (typeLookup.generics.size != 0) {
                if (!baseType.IsGenericTypeDefinition) {
                    throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
                }

                // baseType = ReflectionUtil.CreateGenericType(baseType, ResolveGenericTypes(typeLookup, namespaces, scopeType));
            }

            if (typeLookup.isArray) {
                baseType = baseType.MakeArrayType();
            }

            return baseType;
        }
        
        public static Type ResolveNestedGenericType(Type containingType, Type baseType, TypeLookup typeLookup, LightList<string> namespaces) {

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (typeLookup.generics.array == null || typeLookup.generics.size == 0) {
                throw new TypeResolutionException($"Tried to resolve generic types from {baseType} but no generic types were given in the {nameof(typeLookup)} argument");
            }

            return ReflectionUtil.CreateNestedGenericType(containingType, baseType, ResolveGenericTypes(typeLookup, namespaces));
        }

        public static Type ResolveType(string typeName, IReadOnlyList<string> namespaces, StructList<GenericArguments> generics = default) {

            StringBuilder stringBuilder = new StringBuilder(256); // todo -- cache 
            
            Initialize();
            
            for (int i = 0; i < namespaces.Count; i++) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                if (assemblies == null) {
                    continue;
                }

                stringBuilder.Clear();

                // namespaces[i] + "." + typeName + ", ";
                stringBuilder.Append(namespaces[i]);
                stringBuilder.Append(".");
                stringBuilder.Append(typeName);
                stringBuilder.Append(", ");
                
                string prefixedTypeName = stringBuilder.ToString();
                
                foreach (Assembly assembly in assemblies) {
                    string fullTypeName = prefixedTypeName + assembly.FullName;

                    Type retn = Type.GetType(fullTypeName);

                    if (retn != null) {
                        return retn;
                    }
                }
            }

            LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
            if (lastDitchAssemblies != null) {
                string typename = typeName;
                for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                    Type retn = lastDitchAssemblies[a].GetType(typename);
                    if (retn != null) {
                        return retn;
                    }
                }
            }

            return null;
        }

        private static Type[] ResolveGenericTypes(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null, StructList<GenericResolution> genericResolutions = null) {
            int count = typeLookup.generics.size;

            // new array because this is called from multiple threads
            Type[] array = new Type[count];
            Type[] generics = null;
            Type[] concreteArgs = null;
            if (scopeType != null) {
                if (scopeType.IsGenericType) {
                    generics = scopeType.GetGenericTypeDefinition().GetGenericArguments();
                    concreteArgs = scopeType.GetGenericArguments();
                }
            }

            for (int i = 0; i < count; i++) {
                if (generics != null) {
                    for (int j = 0; j < generics.Length; j++) {
                        if (typeLookup.generics[i].typeName == generics[j].Name) {
                            array[i] = concreteArgs[i];
                            break;
                        }
                    }
                }

                array[i] = array[i] ?? ResolveType(typeLookup.generics[i], namespaces, null, genericResolutions);

                if (array[i] == null) {
                    throw new TypeResolutionException($"Failed to find a type from string {typeLookup.generics[i]}");
                }
            }

            return array;
        }

        private static Type ResolveSimpleType(string typeName, StructList<GenericResolution> genericResolutions) {
            switch (typeName) {
                case "bool": return typeof(bool);
                case "byte": return typeof(byte);
                case "sbyte": return typeof(sbyte);
                case "char": return typeof(char);
                case "decimal": return typeof(decimal);
                case "double": return typeof(double);
                case "float": return typeof(float);
                case "single": return typeof(float);
                case "int": return typeof(int);
                case "uint": return typeof(uint);
                case "long": return typeof(long);
                case "ulong": return typeof(ulong);
                case "object": return typeof(object);
                case "short": return typeof(short);
                case "ushort": return typeof(ushort);
                case "string": return typeof(string);
            }

            if (genericResolutions != null) {
                for (int i = 0; i < genericResolutions.size; i++) {
                    if (typeName == genericResolutions.array[i].genericName) {
                        return genericResolutions.array[i].resolvedType;
                    }
                }
            }

            return null;
        }

        public struct GenericResolution {

            public CharSpan genericName;
            public Type resolvedType;

        }

        private static Type ResolveBaseTypePath(TypeLookup typeLookup, IReadOnlyList<string> namespaces, StructList<GenericResolution> genericResolutions) {
            
            Type retn = ResolveSimpleType(typeLookup.typeName, genericResolutions);
            if (retn != null) {
                return retn;
            }

            Initialize();
            
            
            string baseTypeName = "." + typeLookup.GetBaseTypeName(); // save some string concat
            if (!string.IsNullOrEmpty(typeLookup.namespaceName)) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(typeLookup.namespaceName);

                if (assemblies == null) {
                    throw new TypeResolutionException($"No loaded assemblies found for namespace {typeLookup.namespaceName}");
                }

                string typename = typeLookup.namespaceName + baseTypeName;

                for (int a = 0; a < assemblies.Count; a++) {
                    retn = assemblies[a].GetType(typename);
                    if (retn != null) {
                        return retn;
                    }
                }

                LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
                if (lastDitchAssemblies != null) {
                    typename = typeLookup.typeName;
                    for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                        retn = lastDitchAssemblies[a].GetType(typename);
                        if (retn != null) {
                            return retn;
                        }
                    }
                }
            }
            else {
                if (namespaces != null) {
                    for (int i = 0; i < namespaces.Count; i++) {
                        
                        // todo -- this can end up being 30 + assemblies for things like UnityEngine, probably want to avoid that somehow 
                        
                        LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                        if (assemblies == null) {
                            continue;
                        }

                        string typename = namespaces[i] + baseTypeName;
                        for (int a = 0; a < assemblies.Count; a++) {
                            retn = assemblies[a].GetType(typename);
                            if (retn != null) {
                                return retn;
                            }
                        }
                    }
                }

                LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
                if (lastDitchAssemblies != null) {
                    string typename = typeLookup.typeName;
                    for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                        retn = lastDitchAssemblies[a].GetType(typename);
                        if (retn != null) {
                            return retn;
                        }
                    }
                }
            }

            if (namespaces != null && namespaces.Count > 0 && namespaces[0] != string.Empty) {
                string checkedNamespaces = string.Join(",", namespaces.ToArray());
                throw new TypeResolutionException($"Unable to resolve type {typeLookup}. Looked in namespaces: {checkedNamespaces}");
            }

            throw new TypeResolutionException($"Unable to resolve type {typeLookup}.");
        }

        public class TypeResolutionException : Exception {

            public TypeResolutionException(string message) : base(message) { }

        }

        public static bool IsNamespace(string check) {
            return s_NamespaceMap.ContainsKey(check);
        }

    }

}