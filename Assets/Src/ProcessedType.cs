using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Src {
    [DebuggerDisplay("{type.Name}")]
    public class ProcessedType {
        
        public readonly Type type;
        public readonly List<FieldInfo> propFields;
        public readonly List<FieldInfo> contextProperties;

        private static readonly string[] BuildInProps = {
            
        };
        
        public ProcessedType(Type type) {
            this.type = type;
            propFields = new List<FieldInfo>();
            contextProperties = new List<FieldInfo>();
        }
        
        public bool HasProp(string propName) {
          
            for (int i = 0; i < propFields.Count; i++) {
                if (propFields[i].Name == propName) {
                    return true;
                }
            }

            return false;
        }
        
    }
}