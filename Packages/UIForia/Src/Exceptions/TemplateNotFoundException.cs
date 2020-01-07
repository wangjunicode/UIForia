using System;
using UIForia.Parsing;

namespace UIForia.Exceptions {

    public class TemplateNotFoundException : Exception {

        public TemplateNotFoundException(ProcessedType processedType, string xmlPath) : base($"Unable to find default template for type {processedType.rawType}. Searched using default resolver at paths: \n[{xmlPath}]") { }

    }

}