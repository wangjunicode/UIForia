using System;
using JetBrains.Annotations;
using Src;
using Src.Input;
using Src.Systems;
using TMPro;
using UnityEngine;

namespace Debugger {

    // <InputField type="Text" placeholder="" onValueChanged={} onFocus={} onBlur="{}" onMouseDown={} onKeyUp={}/>

    public interface IDirectDrawMesh {

        Mesh GetMesh();
        event Action<UIElement, Mesh> onMeshUpdate;

    }

    // todo -- enforce 1 child 
//    [SingleChild(typeof(UITextElement))]

    [AcceptFocus]
    [Template("Templates/InputField.xml")]
    public class UIInputFieldElement : UITextContainerElement, IDirectDrawMesh {

        public event Action<UIElement, Mesh> onMeshUpdate;

        protected enum EditState {

            Continue,
            Finish

        }

        public string placeholder;
        public Color caretColor;
        public Color highlightColor;
        public bool showPlaceholder = false;
        public bool hasSelection;
        public bool isReadOnly;
        public bool isMulitLine;
        public bool isPasswordField;
        public bool isRichTextEditingAllowed;
        public bool m_isLastKeyBackspace;
        public bool m_WasCanceled;
        public bool m_isSelectAll;
        public int characterLimit;
        public string m_Text;
        public char m_AsteriskChar;

        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;
        public TMP_InputField.InputType inputType;

        public TMP_InputField.LineType lineType;

        public Mesh mesh;

        public UIInputFieldElement() {
            mesh = new Mesh();
            m_Text = string.Empty;
        }

        // validators?
        // formatters?

        [OnFocus]
        public void OnFocus() { }

        [OnBlur]
        public void OnBlur() { }

        // todo -- delete won't work until cursor / focus is working
        [UsedImplicitly]
        [OnKeyDown(KeyCode.Backspace)]
        private void HandleBackspace() {
            Debug.Log("DELETE");
            if (isReadOnly) {
                return;
            }

            if (hasSelection) {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else {
                if (isRichTextEditingAllowed) {
                    if (stringPositionInternal > 0) {
                        m_Text = m_Text.Remove(stringPositionInternal - 1, 1);
                        stringSelectPositionInternal = stringPositionInternal = stringPositionInternal - 1;

                        m_isLastKeyBackspace = true;

                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else {
                    if (caretPositionInternal > 0) {
                        m_Text = m_Text.Remove(GetStringIndexFromCaretPosition(caretPositionInternal - 1), 1);
                        caretSelectPositionInternal = caretPositionInternal = caretPositionInternal - 1;
                        stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                    }

                    m_isLastKeyBackspace = true;

                    SendOnValueChangedAndUpdateLabel();
                }
            }
        }

        [UsedImplicitly]
        [OnKeyDown(KeyCode.Delete)]
        private void HandleDeleteKey() {
            Debug.Log("DELETE");
            if (isReadOnly) {
                return;
            }

            if (stringPositionInternal == stringSelectPositionInternal)
                return;

            if (isRichTextEditingAllowed || m_isSelectAll) {
                // Handling of Delete when Rich Text is allowed.
                if (stringPositionInternal < stringSelectPositionInternal) {
                    m_Text = text.Substring(0, stringPositionInternal) + m_Text.Substring(stringSelectPositionInternal, m_Text.Length - stringSelectPositionInternal);
                    stringSelectPositionInternal = stringPositionInternal;
                }
                else {
                    m_Text = m_Text.Substring(0, stringSelectPositionInternal) + m_Text.Substring(stringPositionInternal, m_Text.Length - stringPositionInternal);
                    stringPositionInternal = stringSelectPositionInternal;
                }

                m_isSelectAll = false;
            }
            else {
                stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);

                // Handling of Delete when Rich Text is not allowed.
                if (caretPositionInternal < caretSelectPositionInternal) {
                    m_Text = m_Text.Substring(0, stringPositionInternal) + m_Text.Substring(stringSelectPositionInternal, m_Text.Length - stringSelectPositionInternal);

                    stringSelectPositionInternal = stringPositionInternal;
                    caretSelectPositionInternal = caretPositionInternal;
                }
                else {
                    m_Text = m_Text.Substring(0, stringSelectPositionInternal) + m_Text.Substring(stringPositionInternal, m_Text.Length - stringPositionInternal);
                    stringPositionInternal = stringSelectPositionInternal;

                    stringPositionInternal = stringSelectPositionInternal;
                    caretPositionInternal = caretSelectPositionInternal;
                }
            }
        }

        [UsedImplicitly]
        [OnKeyDown]
        private void HandleKeyPress(KeyboardInputEvent evt) {
            char c = evt.character;
            if (!isMulitLine && (c == '\t' || c == '\r' || c == 10)) {
                return;
            }

            if (c == '\r' || c == 3) {
                c = '\n';
            }

            Append(c);
        }

        [OnMouseDown]
        public void OnMouseDown(MouseInputEvent evt) {
            CaretPosition cursor;
            int cursorIndex = TexMeshProUtil.GetCursorIndexFromPosition(textInfo, evt.mousePosition, out cursor);
        }

        public char asteriskChar {
            get { return m_AsteriskChar; }
            set {
                if (m_AsteriskChar != value) {
                    m_AsteriskChar = value;
                    UpdateLabel();
                }
            }
        }

        public string text {
            get { return m_Text; }
            set {
                if (m_Text == value) {
                    return;
                }

                if (value == null) value = string.Empty;

                m_Text = value;

                if (m_StringPosition > m_Text.Length) {
                    m_StringPosition = m_StringSelectPosition = m_Text.Length;
                }

                // Set RectTransform relative position to top of viewport.
                // AdjustTextPositionRelativeToViewport(0);

                // m_forceRectTransformAdjustment = true;

                SendOnValueChangedAndUpdateLabel();
            }
        }

        //[OnKeyCommand(KeyCommand.Copy)]
        public void DoKeyPress() {
            // for each key pressed this frame
            //
        }

        static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        protected int stringPositionInternal {
            get { return m_StringPosition + Input.compositionString.Length; }
            set {
                m_StringPosition = value;
                ClampStringPos(ref m_StringPosition);
            }
        }

        protected int stringSelectPositionInternal {
            get { return m_StringSelectPosition + Input.compositionString.Length; }
            set {
                m_StringSelectPosition = value;
                ClampStringPos(ref m_StringSelectPosition);
            }
        }

        protected int caretPositionInternal {
            get { return m_CaretPosition + Input.compositionString.Length; }
            set {
                m_CaretPosition = value;
                ClampCaretPos(ref m_CaretPosition);
            }
        }

        protected int caretSelectPositionInternal {
            get { return m_CaretSelectPosition + Input.compositionString.Length; }
            set {
                m_CaretSelectPosition = value;
                ClampCaretPos(ref m_CaretSelectPosition);
            }
        }

        protected void ClampStringPos(ref int pos) {
            if (pos < 0)
                pos = 0;
            else if (pos > text.Length)
                pos = text.Length;
        }

        protected void ClampCaretPos(ref int pos) {
            if (pos < 0) {
                pos = 0;
            }
            else if (pos > textInfo.characterCount - 1) {
                pos = textInfo.characterCount - 1;
            }
        }

        protected void SelectAll() {
            m_isSelectAll = true;
            stringPositionInternal = text.Length;
            stringSelectPositionInternal = 0;
        }

        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>
        protected virtual void Append(string input) {
            if (isReadOnly) {
                return;
            }

            for (int i = 0; i < input.Length; ++i) {
                char c = input[i];

                if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n') {
                    Append(c);
                }
            }
        }

        protected virtual void Append(char input) {
            if (isReadOnly) {
                return;
            }

            // Append the character and update the label
            Insert(input);
        }


        // Insert the character and update the label.
        private void Insert(char c) {
            if (isReadOnly || !IsCharacterValid(c)) {
                return;
            }

            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit) {
                return;
            }

            m_Text = text.Insert(m_StringPosition, replaceString);
            stringSelectPositionInternal = stringPositionInternal += replaceString.Length;
        }

        // Handling of DEL key
        private void ForwardSpace() {
            if (isReadOnly) {
                return;
            }

            if (hasSelection) {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else {
                if (isRichTextEditingAllowed) {
                    if (stringPositionInternal < text.Length) {
                        m_Text = text.Remove(stringPositionInternal, 1);
                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else {
                    if (caretPositionInternal < textInfo.characterCount - 1) {
                        stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                        m_Text = text.Remove(stringPositionInternal, 1);
                        SendOnValueChangedAndUpdateLabel();
                    }
                }
            }
        }

        private void Delete() {
            if (isReadOnly) {
                return;
            }

            if (stringPositionInternal == stringSelectPositionInternal)
                return;

            if (isRichTextEditingAllowed || m_isSelectAll) {
                // Handling of Delete when Rich Text is allowed.
                if (stringPositionInternal < stringSelectPositionInternal) {
                    m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);
                    stringSelectPositionInternal = stringPositionInternal;
                }
                else {
                    m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
                    stringPositionInternal = stringSelectPositionInternal;
                }

                m_isSelectAll = false;
            }
            else {
                stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);

                // Handling of Delete when Rich Text is not allowed.
                if (caretPositionInternal < caretSelectPositionInternal) {
                    m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);

                    stringSelectPositionInternal = stringPositionInternal;
                    caretSelectPositionInternal = caretPositionInternal;
                }
                else {
                    m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
                    stringPositionInternal = stringSelectPositionInternal;

                    stringPositionInternal = stringSelectPositionInternal;
                    caretPositionInternal = caretSelectPositionInternal;
                }
            }
        }

        private void Backspace() {
            if (isReadOnly) {
                return;
            }

            if (hasSelection) {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else {
                if (isRichTextEditingAllowed) {
                    if (stringPositionInternal > 0) {
                        m_Text = m_Text.Remove(stringPositionInternal - 1, 1);
                        stringSelectPositionInternal = stringPositionInternal = stringPositionInternal - 1;

                        m_isLastKeyBackspace = true;

                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else {
                    if (caretPositionInternal > 0) {
                        m_Text = m_Text.Remove(GetStringIndexFromCaretPosition(caretPositionInternal - 1), 1);
                        caretSelectPositionInternal = caretPositionInternal = caretPositionInternal - 1;
                        stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                    }

                    m_isLastKeyBackspace = true;

                    SendOnValueChangedAndUpdateLabel();
                }
            }
        }

        private void SendOnValueChangedAndUpdateLabel() {
            UpdateLabel();
        }

        private int GetStringIndexFromCaretPosition(int caretPosition) {
            // Clamp values between 0 and character count.
            ClampCaretPos(ref caretPosition);

            return textInfo.characterInfo[caretPosition].index;
        }


        protected void UpdateLabel() {
            string fullText;
            if (Input.compositionString.Length > 0) {
                fullText = text.Substring(0, m_StringPosition) + Input.compositionString + text.Substring(m_StringPosition);
            }
            else {
                fullText = text;
            }

            string processed = inputType == TMP_InputField.InputType.Password ? new string(asteriskChar, fullText.Length) : fullText;

            // If not currently editing the text, set the visible range to the whole text.
            // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
            // We can't do this when text is being edited since it would discard the current scroll,
            // which is defined by means of the m_DrawStart and m_DrawEnd indices.

            if (!string.IsNullOrEmpty(fullText)) {
                SetCaretVisible();
            }

            m_Text = processed + "\u200B"; // Extra space is added for Caret tracking.

            // Scrollbar should be updated.
            // m_IsScrollbarUpdateRequired = true;
        }

        void SetCaretVisible() {
//            if (!m_AllowInput)
//                return;
//
//            m_CaretVisible = true;
//            m_BlinkStartTime = Time.unscaledTime;
//            SetCaretActive();
        }


        private string GetSelectedString() {
            if (!hasSelection) return "";

            int startPos = stringPositionInternal;
            int endPos = stringSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos) {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            return text.Substring(startPos, endPos - startPos);
        }

        public Mesh GetMesh() {
            return mesh;
        }

    }

}