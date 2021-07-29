using UIForia.Text;

namespace UIForia.Elements {

    [TagName("Default", "Text")]
    public unsafe class UITextElement : UIElement {

        // [ImplicitTemplateArgument]
        public string text {
            get => GetText();
            set => SetText(value);
        }

        internal string _text;
        internal RichText _richText;

        public RichText richText {
            get => _richText;
            set => _richText = value; // todo detect changes accordingly 
        }

        internal TextId textId; // set at creation time and unset when destroyed 
        internal TextDataTable* textTable; // set at creation time and unset when destroyed

        private bool stringIsDirty;
        private bool layoutContentChanged;

        internal bool LayoutContentChanged => layoutContentChanged || richText != null && richText.layoutContentChanged;

        public string GetText() {

            if (!stringIsDirty) {
                return _text;
            }

            stringIsDirty = false;

            int size = textTable->GetMainThreadContent(textId, out char* cbuffer);

            _text = size <= 0
                ? string.Empty
                : new string(cbuffer, 0, size);

            return _text;
        }

        private static readonly char[] s_EmptyChars = { };

        public void SetTextFromCharacters(char[] newText, int length) {

            if (newText == null) newText = s_EmptyChars;

            if (textTable == null) {
                // todo -- do this before create via template compiler 
                textTable = application.textDataTable;
                textId = textTable->AllocateTextId(elementId);
            }

            fixed (char* newTextPtr = newText) {
                stringIsDirty = textTable->SetMainThreadTextData(textId, newTextPtr, length);
            }
        }

        public void SetText(string newText) {

            if (newText == null) newText = string.Empty;

            if (textTable == null) {
                // todo -- do this before create via template compiler 
                textTable = application.textDataTable;
                textId = textTable->AllocateTextId(elementId);
            }

            fixed (char* newTextPtr = newText) {
                textTable->SetMainThreadTextData(textId, newTextPtr, newText.Length);
                stringIsDirty = false;
                _text = newText;
            }

        }

        public override string GetDisplayName() {
            if (text == null) {
                return "Text";
            }

            return "Text('" + text + "')";
        }

    }

}