using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UnityEditor;

namespace UIForia.Compilers {

    public class CompiledTemplate {

        public GUID guid;
        public int templateId;
        public string filePath;
        public string templateName;
        public LambdaExpression templateFn;
        public TemplateMetaData templateMetaData;
        internal ProcessedType elementType;
        internal AttributeDefinition[] attributes;

    }

}