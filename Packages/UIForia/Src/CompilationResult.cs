using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia {

    public class CompilationResult {

        public bool successful { get; internal set; }

        public ProcessedType rootType;
        internal LightList<TemplateData> compiledTemplates;
        public Dictionary<Type, int> templateDataMap;

        internal CompilationResult() {
            compiledTemplates = new LightList<TemplateData>(32);
            templateDataMap = new Dictionary<Type, int>(32);
        }

    }

}