using System;
using System.Xml;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Parsing.Expression.Tokenizer;

namespace UIForia.Exceptions {
    
    public class TemplateParseException : Exception {

        private int column;

        private int line;

        private string fileName;

        private string message;

        private ExpressionTokenType tokenType;

        private ExpressionToken token;

        private ASTNode node;

        public TemplateParseException(string message, ASTNode node) : base(message) {
            this.message = message;
            this.node = node;
        }

        public TemplateParseException(string fileName, string message, Exception rootCause) : base(message, rootCause) {
            this.message = message;
            this.fileName = fileName;
        }
        
        public TemplateParseException(string fileName, string message, Exception rootCause, ASTNode node) : base(message, rootCause) {
            this.message = message;
            this.fileName = fileName;
            if (node != null) {
                this.line = node.line;
                this.column = node.column;
                this.node = node;
            }
        }

        public TemplateParseException(string fileName, string message) : this(fileName, null, message) {
        }

        public TemplateParseException(IXmlLineInfo element, string message) : this(string.Empty, element, message) {
        }

        public TemplateParseException(string fileName, IXmlLineInfo element, string message) {
            if (element != null) {
                line = element.LineNumber;
                column = element.LinePosition;
            }
            this.fileName = fileName;
            this.message = message;
        }

        public void SetFileName(string name) {
            this.fileName = $"Error in file {name}: ";
        }
        
        public override string Message => fileName + $"\nYour template contains an error in line {line} column {column}." 
                                                   + $"\n\tMessage:\n\t\t{message}" 
                                                   + $"\n\tToken:\n\t\t{token.value}" 
                                                   + (node != default ? $"\n\tNode:\n\t\t{node}" : string.Empty);

    }
}
