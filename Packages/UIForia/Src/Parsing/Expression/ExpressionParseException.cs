using System;

namespace UIForia.Exceptions {

    internal class ExpressionParseException : Exception {

        private string fileName = "";

        public ExpressionParseException(string message = null) : base(message) { }

        public override string Message => fileName + base.Message;

    }

}