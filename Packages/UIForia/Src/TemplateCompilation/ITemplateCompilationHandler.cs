using UIForia.Parsing;

namespace UIForia.Compilers {

    internal interface ITemplateCompilationHandler {

        Diagnostics Diagnostics { get; }

        UIModule GetModuleForType(ProcessedType processedType);

        void OnExpressionReady(CompiledExpression compiledExpression);

        TemplateFileShell GetTemplateForType(ProcessedType processedType);

    }

}