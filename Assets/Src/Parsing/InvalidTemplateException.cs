using System;

namespace UIForia {

    public class InvalidTemplateException : Exception {

        private readonly string templateName;

        public InvalidTemplateException(string message) : base(message) {
            this.templateName = string.Empty;
        }
        
        public InvalidTemplateException(string templateName, string message) : base(message) {
            this.templateName = templateName;
        }

    }

}