using UIForia.Attributes;
using UIForia.UIInput;

namespace UIForia.Elements {
    [TagName("Default", "EditableText")]
    public class EditableText : SelectableText {

        public TextSettings settings;


        [OnPropertyChanged(nameof(textMode))]
        public void EditModeChanged(TextMode prev) {
            if (prev == TextMode.Edit) {
                application.ReleaseTextInputFocus();
            }
        }

        [OnTextInput]
        public void OnTextInputEvent(TextInputEvent evt) {

            // flush event queue 

            // value = editor.EditText(value, ref caret, evt.eventQueue);
            // characterQueue -> Dequeue() (clear on blur / no focused text input) 
            text = evt.text;
            // application.SetEditedText(value, caret);

            // --- * ----

            // tehyatlha (745) 414

            // value = Mask.Apply(value);
            // this.displaytext = evt;

            // for (int index = 0; index < evt.characters.size; index++) {
            //     value += evt.characters[index];
            // }

            // view.EditText(elementId, evt);

        }

        // todo fix that bug that does not respect mouse click handlers when being inherited
        [OnMouseClick]
        public unsafe void RequestFocusOnClick(MouseInputEvent evt) {
            OnMouseClick(evt);
            if (application.focusedTextInputElementId != elementId) {
                switch (textMode) {
                    case TextMode.Default:
                    case TextMode.Select:
                        break;
                    case TextMode.Edit:
                        if (cursor.cursor == -1) {
                            cursor.cursor = 0;
                        }
                        application.ActivateTextEditor(this, new TextEditorData() {
                            isMultiline = settings.multiline
                            // onTextChange = OnTextInputEvent
                            // onCharacter = (text, newChar, editor) => {
                            //
                            //     if (newChar == '\u00B1') {
                            //         editor.EndEditing(); // if has more queued keys they stay in queue 
                            //         // editor.KeyQueue.Clear();
                            //     }
                            //
                            // }
                        });

                        break;
                }
            }
        }

    }
}