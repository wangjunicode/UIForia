using System;

namespace Src {

    public class InvalidTemplateException : Exception {

        public readonly string templateName;

        public InvalidTemplateException(string templateName, string message) : base(message) {
            this.templateName = templateName;
        }

        public string FullErrorMessage => $"Template '{templateName}' is invalid: {Message}";

    }

}