using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace UIForia {

    public class TypeCreator {

        public ModuleBuilder moduleBuilder;
        private Dictionary<string, int> nameMap;
        private static int version;
        
        public TypeCreator() {
            nameMap = new Dictionary<string, int>();
            moduleBuilder = AppDomain
                .CurrentDomain
                // note: using just `Run` seems to crash unity when it garbage collects assets.
                // note: this *might* cause memory inflation, I'm not sure if the generated types get collected
                // seems like unity crashes if the access is 'RunAndCollect'. 
                .DefineDynamicAssembly(new AssemblyName("UIForiaDynamicAssembly" + version), AssemblyBuilderAccess.RunAndSave)
                .DefineDynamicModule("UIForiaDynamicModule" + version);
            version++;
        }

        public TypeBuilder DefineClassType(string typeName, Type baseType = null) {
            baseType ??= typeof(object);
            
            if (nameMap.TryGetValue(typeName, out int count)) {
                typeName += count;
            }
            count++;
            nameMap[typeName] = count;

            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, baseType);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            return typeBuilder;
        }
        
        public TypeBuilder DefineStructType(string typeName) {
            if (nameMap.TryGetValue(typeName, out int count)) {
                typeName += count;
                count++;
            }

            nameMap[typeName] = count;
            
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, typeof(ValueType)); // value types are apparently ~2x faster to create then ref types
            return typeBuilder;
        }

    }

}