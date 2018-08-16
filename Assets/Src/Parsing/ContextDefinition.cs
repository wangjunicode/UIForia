using System;

namespace Src {

    public class ContextDefinition {

        public readonly ProcessedType processedType;
        public string alias;
        public string indexAlias;
        public string lengthAlias;
        
        public ContextDefinition(ProcessedType processedType) {
            this.processedType = processedType;
        }

        public ContextDefinition(Type type) {
            this.processedType = TypeProcessor.GetType(type);
        }

    }

}