using System;
using Src.Compilers.AliasSource;

namespace Src.Compilers.AliasSources {

    public class ExternalReferenceAliasSource : IAliasSource {

        public object referenceValue;
        public string referenceName;

        public ExternalReferenceAliasSource(string referenceName, object referenceValue) {
            this.referenceName = referenceName;
            this.referenceValue = referenceValue;
        }
        
        public object ResolveAlias(string alias, object data = null) {
            if (alias == referenceName) return referenceValue;
            return null;
        }

    }

}