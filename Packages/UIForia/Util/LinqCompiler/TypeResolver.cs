using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Parsing.Expressions;
using Debug = UnityEngine.Debug;

namespace UIForia.Util {

    public class TypeResolver {

        private readonly string[] SingleNamespace = new string[1]; // instance for thread safety

        private static Dictionary<string, LightList<Assembly>> s_NamespaceMap;

        private static TypeResolver defaultResolver;

        private static Assembly[] s_AllAssemblies;

        public TypeResolver() {
            Initialize();
        }

        public static void Initialize() {
            if (s_AllAssemblies == null) {
                s_AllAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToArray();
                FilterAssemblies();
            }
        }

        public static TypeResolver Default {
            get {
                if (defaultResolver != null) {
                    return defaultResolver;
                }

                defaultResolver = new TypeResolver();
                return defaultResolver;
            }
        }

        private static void FilterAssemblies() {
            // todo -- could be optimized by writing a tmp file with all non package/asset assemblies namespaces
            
            // string path = Path.GetTempPath();
            //
            // if (File.Exists(Path.Combine(Path.GetTempPath(), "uiforia_type-scan.txt"))) {
            //     
            // }
            
            if (s_NamespaceMap != null) return;

            s_NamespaceMap = new Dictionary<string, LightList<Assembly>>();

            Stopwatch watch = Stopwatch.StartNew();

            // Note, I tried to do this in parallel but assembly.GetTypes() must be locked or something since
            // the performance of doing this in parallel was worse than doing it single threaded

            int cnt = 0;

            Assembly[] a = new Assembly[1];
            a[0] = typeof(List<>).Assembly;
            for (int i = 0; i < s_mscorlib.Length; i++) {
                s_NamespaceMap.Add(s_mscorlib[i], new LightList<Assembly>(a));
            }

            Assembly current = Assembly.GetExecutingAssembly();

            for (int i = 0; i < s_AllAssemblies.Length; i++) {
                Assembly assembly = s_AllAssemblies[i];

                if (assembly.IsDynamic || assembly == a[0] || assembly.ReflectionOnly || assembly.FullName.StartsWith("nunit.framework", StringComparison.Ordinal)) {
                    continue;
                }

                cnt++;

                try {
                    
                    Type[] types = assembly == current ? assembly.GetTypes() : assembly.GetExportedTypes();
                    
                    for (int j = 0; j < types.Length; j++) {
                        Type currentType = types[j];
                        // can be null if assembly referenced is unavailable
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse

                        // nested types are always in their parent namespace, no need to scan
                        if (currentType == null || !currentType.IsPublic || currentType.IsNested) {
                            continue;
                        }

                        if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> list)) {
                            list = new LightList<Assembly>(4);
                            s_NamespaceMap.Add(currentType.Namespace ?? "null", list);
                        }

                        if (!list.Contains(assembly)) {
                            list.Add(assembly);
                        }
                    }
                }
                catch (ReflectionTypeLoadException) {
                    Debug.Log($"{assembly.FullName}");
                    throw;
                }
            }

            Debug.Log($"Scanned namespaces in {watch.Elapsed.TotalMilliseconds:F3} ms from {cnt} assemblies");
            watch.Stop();
        }

        public Type ResolveTypeExpression(Type invokingType, IList<string> namespaces, string typeExpression) {
            typeExpression = typeExpression.Replace("[", "<").Replace("]", ">");
            if (ExpressionParser.TryParseTypeName(typeExpression, out TypeLookup typeLookup)) {
                return ResolveType(typeLookup, (IReadOnlyList<string>) namespaces, invokingType);
            }

            return null;
        }

        public bool IsNamespace(string toCheck) {
            return s_NamespaceMap.ContainsKey(toCheck);
        }

        private static Type ResolveSimpleType(string typeName) {
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

            return null;
        }

        private LightList<Type> ResolveGenericTypes(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            int count = typeLookup.generics.size;

            LightList<Type> results = LightList<Type>.Get();
            results.EnsureCapacity(count);

            Type[] array = results.array;
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

                array[i] = array[i] ?? ResolveType(typeLookup.generics[i], namespaces);

                if (array[i] == null) {
                    throw new TypeResolutionException($"Failed to find a type from string {typeLookup.generics[i]}");
                }
            }

            results.Count = typeLookup.generics.size;
            return results;
        }

        private Type ResolveBaseTypePath(TypeLookup typeLookup, IReadOnlyList<string> namespaces) {
            Type retn = ResolveSimpleType(typeLookup.typeName);

            if (retn != null) {
                return retn;
            }

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

        public Type ResolveType(string typeName, string namespaceName) {
            SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(new TypeLookup(typeName), SingleNamespace);
        }

        public Type ResolveType(TypeLookup typeLookup, string namespaceName) {
            SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(typeLookup, SingleNamespace);
        }

        public Type ResolveType(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            if (typeLookup.resolvedType != null) {
                return typeLookup.resolvedType;
            }

            FilterAssemblies();

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces);

            if (typeLookup.generics != null && typeLookup.generics.Count != 0) {
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

        public Type ResolveNestedGenericType(Type containingType, Type baseType, TypeLookup typeLookup, LightList<string> namespaces) {
            FilterAssemblies();

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (typeLookup.generics == null || typeLookup.generics.Count == 0) {
                throw new TypeResolutionException($"Tried to resolve generic types from {baseType} but no generic types were given in the {nameof(typeLookup)} argument");
            }

            return ReflectionUtil.CreateNestedGenericType(containingType, baseType, ResolveGenericTypes(typeLookup, namespaces));
        }

        public Type ResolveType(string typeName, IReadOnlyList<string> namespaces) {
            FilterAssemblies();

            for (int i = 0; i < namespaces.Count; i++) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                if (assemblies == null) {
                    continue;
                }

                string prefixedTypeName = namespaces[i] + "." + typeName + ", ";
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

        // this is a big assembly to scan, so I hardcoded it's namespaces. System and System.XML assemblies could also be stripped to shave another ~2.5ms probably not worth it.
        private static readonly string[] s_mscorlib = {
            "Internal.Runtime.CompilerServices",
            "Internal.Runtime.Augments",
            "XamMac.CoreFoundation",
            "Mono",
            "Mono.Xml",
            "Mono.Interop",
            "Mono.Globalization.Unicode",
            "Mono.Security",
            "Mono.Security.X509",
            "Mono.Security.X509.Extensions",
            "Mono.Security.Cryptography",
            "Mono.Security.Authenticode",
            "Mono.Math",
            "Mono.Math.Prime",
            "Mono.Math.Prime.Generator",
            "Microsoft.Reflection",
            "Microsoft.Win32",
            "Microsoft.Win32.SafeHandles",
            "System",
            "System.Deployment.Internal",
            "System.Configuration.Assemblies",
            "System.Text",
            "System.Resources",
            "System.Reflection",
            "System.Reflection.Metadata",
            "System.Reflection.Emit",
            "System.IO",
            "System.IO.IsolatedStorage",
            "System.Globalization",
            "System.Numerics.Hashing",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Security",
            "System.Security.Policy",
            "System.Security.Permissions",
            "System.Security.AccessControl",
            "System.Security.Util",
            "System.Security.Principal",
            "System.Security.Claims",
            "System.Security.Cryptography",
            "System.Security.Cryptography.X509Certificates",
            "System.Runtime",
            "System.Runtime.Hosting",
            "System.Runtime.Versioning",
            "System.Runtime.Serialization",
            "System.Runtime.Serialization.Formatters",
            "System.Runtime.Serialization.Formatters.Binary",
            "System.Runtime.Remoting",
            "System.Runtime.Remoting.Services",
            "System.Runtime.Remoting.Proxies",
            "System.Runtime.Remoting.Lifetime",
            "System.Runtime.Remoting.Contexts",
            "System.Runtime.Remoting.Channels",
            "System.Runtime.Remoting.Activation",
            "System.Runtime.Remoting.Metadata",
            "System.Runtime.Remoting.Metadata.W3cXsd2001",
            "System.Runtime.Remoting.Messaging",
            "System.Runtime.ExceptionServices",
            "System.Runtime.ConstrainedExecution",
            "System.Runtime.CompilerServices",
            "System.Runtime.InteropServices",
            "System.Runtime.InteropServices.WindowsRuntime",
            "System.Runtime.InteropServices.Expando",
            "System.Runtime.InteropServices.ComTypes",
            "System.Buffers",
            "System.Buffers.Binary",
            "System.Collections",
            "System.Collections.ObjectModel",
            "System.Collections.Concurrent",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.Diagnostics.SymbolStore",
            "System.Diagnostics.Contracts",
            "System.Diagnostics.Contracts.Internal",
            "System.Diagnostics.CodeAnalysis",
            "System.Diagnostics.Private",
            "System.Diagnostics.Tracing",
            "System.Diagnostics.Tracing.Internal",
            "System.Runtime.DesignerServices",
            "Unity",
        };

    }

    public class TypeResolutionException : Exception {

        public TypeResolutionException(string message) : base(message) { }

    }

}