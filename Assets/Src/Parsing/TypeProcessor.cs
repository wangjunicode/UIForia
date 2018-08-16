using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;

namespace Src {

    public static class TypeProcessor {

        private const BindingFlags BindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<string, ProcessedType> typeMap = new Dictionary<string, ProcessedType>();

        public static ProcessedType GetType(string typeName, List<ImportDeclaration> importPaths = null) {
            if (typeMap.ContainsKey(typeName)) {
                return typeMap[typeName];
            }

            Type type = Type.GetType(typeName);

            // todo -- wrong!
//            if (type == null && importPaths != null) {
//                for (int i = 0; i < importPaths.Count; i++) {
//                    type = Type.GetType(importPaths[i].alias);
//                    if (type != null) break;
//                }    
//            }

            return GetType(type);
        }

        public static ProcessedType GetType(Type type) {
            
            Assert.IsNotNull(type, "type != null");
            
            ProcessedType processedType = new ProcessedType(type);

            FieldInfo[] fields = type.GetFields(BindFlags);
            for (int i = 0; i < fields.Length; i++) {
                FieldInfo fieldInfo = fields[i];
                object[] attrs = fieldInfo.GetCustomAttributes(typeof(PropAttribute), true);
                if (attrs.Length > 0) {
                    processedType.propFields.Add(fieldInfo);
                    continue; //todo -- props are not compatible with observed properties
                }

               
            }

            typeMap[type.Name] = processedType;
            return processedType;
        }

    }

}