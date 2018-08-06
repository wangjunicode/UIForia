using System;

namespace Src {

    public class InvalidTemplateException : Exception {

        public InvalidTemplateException(string templateName, string message) : base($"Template '{templateName}' is invalid: {message}") { }

    }

}