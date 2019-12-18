using System;
using UIForia.Parsing;
using UIForia.Parsing.Style.Tokenizer;

namespace UIForia.Exceptions {
    
    public class ParseException : Exception {

        private string fileName = "";

        public readonly StyleToken token;
        
        public ParseException(string fileName, string message = null) : base(message) {
            this.fileName = fileName;
        }
        
        public ParseException(StyleToken token, string message = null) : 
            base($"Parse error at line {token.line}, column {token.column}, token type '{token.styleTokenType}' -> {token.value}\n{message}") {
            this.token = token;
        }
        
        public ParseException(string message = null) : base(message) {
        }

        public void SetFileName(string name) {
            this.fileName = "Error in file " + name + ": ";
        }

        public override string Message => fileName + base.Message;

        public static ParseException MultipleSlotsWithSameName(string filePath, string slotName) {
            return new ParseException($"{filePath} defines a slot with the name '{slotName}' multiple times.");
        }

        public static ParseException InvalidSlotOverride(string parentTag, string slotTag) {
            return new ParseException($"Slot overrides can only be defined as a direct child of an expanded template. {parentTag} is not an expanded template and cannot support slot override {slotTag}");
        }

        public static ParseException MultipleSlotOverrides(string nodeSlotName) {
            return new ParseException($"Slot with name {nodeSlotName} was overridden multiple times, which is invalid");
        }

        public static ParseException UnnamedSlotOverride(string fileName, in TemplateLineInfo templateLineInfo) {
            return new ParseException(fileName + " -> Invalid slot override at line: " + templateLineInfo + " a slot:name attribute is required");
        }

        public static ParseException SlotNotFound(string fileName, string slotName, TemplateLineInfo templateLineInfo) {
            return new ParseException(fileName + $" -> A slot with the name {slotName} does not exist to override at line: " + templateLineInfo);
        }

    }
}