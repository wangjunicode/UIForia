using UIForia.Compilers;
using UIForia.Util;

namespace UIForia {

    public class CompilationResult {

        public bool successful { get; internal set; }

        internal LightList<TemplateExpressionSet> compiledTemplates;

        internal CompilationResult() {
            compiledTemplates = new LightList<TemplateExpressionSet>(32);
        }

    }

}