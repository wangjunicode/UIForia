using System;
using Src;
using Src.Input;
using Src.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

    // <InputField type="Text" placeholder="" onValueChanged={} onFocus={} onBlur="{}" onMouseDown={} onKeyUp={}/>

    public class OnFocusAttribute : Attribute { }

    public class OnBlurAttribute : Attribute { }

    public class OnMouseDownAttribute : Attribute { }

    public class OnKeyPressedWhileFocusedAttribute : Attribute {

        public OnKeyPressedWhileFocusedAttribute() { }

        public OnKeyPressedWhileFocusedAttribute(KeyCode key, EventModifiers modifiers = EventModifiers.None) {
            
        }

    }

    public interface IDirectDrawMesh {

        Mesh GetMesh();

    }

    [AcceptFocus]
    [Template("Templates/InputField.xml")]
    public class UIInputFieldElement : UIElement, IDirectDrawMesh {

        protected enum EditState {

            Continue,
            Finish

        }

        public delegate void ValueChanged<in T>(T value);

        public string content;
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
        public event ValueChanged<string> onValueChanged;

        public TextMeshProUGUI m_TextComponent;
        public TextMeshProUGUI m_Placeholder;

        public Mesh mesh;

        public UIInputFieldElement() {
            flags |= UIElementFlags.InputField;
            mesh = new Mesh();
        }

        public void SetText(string newText) {
            content = newText;
            onValueChanged?.Invoke(newText);
        }

        // validators?
        // formatters?

        public void GameObjectRenderable(Rect rect) { }

        [OnFocus]
        public void OnFocus() { }

        [OnBlur]
        public void OnBlur() { }

        [OnKeyPressedWhileFocused]
        public void OnKeyPressWhileFocused(KeyboardInputEvent evt) {
            //UpdateText();
            VertexHelper h;

        }

        [OnMouseDown]
        public void OnMouseDown(MouseInputEvent evt) {
            CaretPosition cursor;
            int cursorIndex = TexMeshProUtil.GetCursorIndexFromPosition(m_TextComponent.textInfo, evt.mousePosition, out cursor);
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
            else if (pos > m_TextComponent.textInfo.characterCount - 1) {
                pos = m_TextComponent.textInfo.characterCount - 1;
            }
        }

        protected void SelectAll() {
            m_isSelectAll = true;
            stringPositionInternal = text.Length;
            stringSelectPositionInternal = 0;
        }

        protected EditState KeyPressed(Event evt) {
            // move to input event
            var currentEventModifiers = evt.modifiers;
            RuntimePlatform rp = Application.platform;
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);
            bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            switch (evt.keyCode) {
                case KeyCode.Backspace: {
                    Backspace();
                    return EditState.Continue;
                }

                case KeyCode.Delete: {
                    ForwardSpace();
                    return EditState.Continue;
                }

                case KeyCode.Home: {
                  //  MoveToStartOfLine(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.End: {
                   // MoveToEndOfLine(shift, ctrl);
                    return EditState.Continue;
                }

                // Select All
                case KeyCode.A: {
                    if (ctrlOnly) {
                        SelectAll();
                        return EditState.Continue;
                    }

                    break;
                }

                // Copy
                case KeyCode.C: {
                    if (ctrlOnly) {
                        if (!isPasswordField)
                            clipboard = GetSelectedString();
                        else
                            clipboard = "";
                        return EditState.Continue;
                    }

                    break;
                }

                // Paste
                case KeyCode.V: {
                    if (ctrlOnly) {
                        Append(clipboard);
                        return EditState.Continue;
                    }

                    break;
                }

                // Cut
                case KeyCode.X: {
                    if (ctrlOnly) {
                        if (isPasswordField) {
                            clipboard = GetSelectedString();
                        }
                        else {
                            clipboard = "";
                        }

                        Delete();
                        SendOnValueChangedAndUpdateLabel();
                        return EditState.Continue;
                    }

                    break;
                }

                case KeyCode.LeftArrow: {
                   // MoveLeft(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.RightArrow: {
                   // MoveRight(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.UpArrow: {
                    //MoveUp(shift);
                    return EditState.Continue;
                }

                case KeyCode.DownArrow: {
                   // MoveDown(shift);
                    return EditState.Continue;
                }

                case KeyCode.PageUp: {
                   // MovePageUp(shift);
                    return EditState.Continue;
                }

                case KeyCode.PageDown: {
                   // MovePageDown(shift);
                    return EditState.Continue;
                }

                // Submit
                case KeyCode.Return:
                case KeyCode.KeypadEnter: {
                    if (lineType != TMP_InputField.LineType.MultiLineNewline) {
                        return EditState.Finish;
                    }

                    break;
                }

                case KeyCode.Escape: {
                    m_WasCanceled = true;
                    return EditState.Finish;
                }
            }

            char c = evt.character;

            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (!isMulitLine && (c == '\t' || c == '\r' || c == 10))
                return EditState.Continue;

            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int) c == 3)
                c = '\n';

            //if (IsValidChar(c)) {
            //    Append(c);
            // }

            if (c == 0) {
                if (Input.compositionString.Length > 0) {
                    UpdateLabel();
                }
            }

            return EditState.Continue;
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
            if (isReadOnly) {
                return;
            }

            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit)
                return;

            m_Text = text.Insert(m_StringPosition, replaceString);
            stringSelectPositionInternal = stringPositionInternal += replaceString.Length;

            onValueChanged?.Invoke(m_Text);
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
                    if (caretPositionInternal < m_TextComponent.textInfo.characterCount - 1) {
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
            onValueChanged?.Invoke(m_Text);
            UpdateLabel();
        }

        private int GetStringIndexFromCaretPosition(int caretPosition) {
            // Clamp values between 0 and character count.
            ClampCaretPos(ref caretPosition);

            return m_TextComponent.textInfo.characterInfo[caretPosition].index;
        }


        protected void UpdateLabel() {
            if (m_TextComponent != null && m_TextComponent.font != null) {
                string fullText;
                if (Input.compositionString.Length > 0)
                    fullText = text.Substring(0, m_StringPosition) + Input.compositionString + text.Substring(m_StringPosition);
                else
                    fullText = text;

                string processed;
                if (inputType == TMP_InputField.InputType.Password)
                    processed = new string(asteriskChar, fullText.Length);
                else
                    processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (m_Placeholder != null)
                    m_Placeholder.enabled = isEmpty; // && !isFocused;

                // If not currently editing the text, set the visible range to the whole text.
                // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
                // We can't do this when text is being edited since it would discard the current scroll,
                // which is defined by means of the m_DrawStart and m_DrawEnd indices.

                if (!isEmpty) {
                    SetCaretVisible();
                }

                m_TextComponent.text = processed + "\u200B"; // Extra space is added for Caret tracking.
                MarkGeometryAsDirty();

                // Scrollbar should be updated.
                // m_IsScrollbarUpdateRequired = true;

                //m_PreventFontCallback = false;
            }
        }

        void SetCaretVisible() {
//            if (!m_AllowInput)
//                return;
//
//            m_CaretVisible = true;
//            m_BlinkStartTime = Time.unscaledTime;
//            SetCaretActive();
        }

        private void MarkGeometryAsDirty() { }

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

    public static class TexMeshProUtil {

        public static int FindNearestLine(TMP_TextInfo textInfo, Vector2 positionInRect) {
            float num1 = float.PositiveInfinity;
            int num2 = -1;

            for (int index = 0; index < textInfo.lineCount; ++index) {
                TMP_LineInfo tmpLineInfo = textInfo.lineInfo[index];
                float y1 = tmpLineInfo.ascender;
                float y2 = tmpLineInfo.descender;

                if (y1 > positionInRect.y && y2 < positionInRect.y) {
                    return index;
                }

                float num3 = Mathf.Min(Mathf.Abs(y1 - positionInRect.y), Mathf.Abs(y2 - positionInRect.y));
                if (num3 < num1) {
                    num1 = num3;
                    num2 = index;
                }
            }

            return num2;
        }

        public static int FindNearestCharacterOnLine(TMP_TextInfo textInfo, Vector2 position, int line, bool visibleOnly) {
            int firstCharacterIndex = textInfo.lineInfo[line].firstCharacterIndex;
            int lastCharacterIndex = textInfo.lineInfo[line].lastCharacterIndex;

            float num1 = float.PositiveInfinity;
            int num2 = lastCharacterIndex;

            for (int index = firstCharacterIndex; index < lastCharacterIndex; ++index) {
                TMP_CharacterInfo tmpCharacterInfo = textInfo.characterInfo[index];
                if (!visibleOnly || tmpCharacterInfo.isVisible) {
                    Vector3 vector3_1 = tmpCharacterInfo.bottomLeft;
                    Vector3 vector3_2 = new Vector3(tmpCharacterInfo.bottomLeft.x, tmpCharacterInfo.topRight.y, 0.0f);
                    Vector3 vector3_3 = tmpCharacterInfo.topRight;
                    Vector3 vector3_4 = new Vector3(tmpCharacterInfo.topRight.x, tmpCharacterInfo.bottomLeft.y, 0.0f);

                    Rect characterRect = new Rect(
                        new Vector2(tmpCharacterInfo.topLeft.x, tmpCharacterInfo.topLeft.y),
                        new Vector2(tmpCharacterInfo.bottomRight.x, tmpCharacterInfo.bottomRight.y)
                    );

                    if (characterRect.Contains(position)) {
                        num2 = index;
                        break;
                    }

                    float line1 = DistanceToLine(vector3_1, vector3_2, position);
                    float line2 = DistanceToLine(vector3_2, vector3_3, position);
                    float line3 = DistanceToLine(vector3_3, vector3_4, position);
                    float line4 = DistanceToLine(vector3_4, vector3_1, position);

                    float num3 = line1 >= line2 ? line2 : line1;
                    float num4 = num3 >= line3 ? line3 : num3;
                    float num5 = num4 >= line4 ? line4 : num4;

                    if (num1 > num5) {
                        num1 = num5;
                        num2 = index;
                    }
                }
            }

            return num2;
        }

        // todo convert to 2d
        public static float DistanceToLine(Vector3 a, Vector3 b, Vector3 point) {
            Vector3 vector3_1 = b - a;
            Vector3 vector3_2 = a - point;
            float num = Vector3.Dot(vector3_1, vector3_2);
            if ((double) num > 0.0)
                return Vector3.Dot(vector3_2, vector3_2);
            Vector3 vector3_3 = point - b;
            if ((double) Vector3.Dot(vector3_1, vector3_3) > 0.0)
                return Vector3.Dot(vector3_3, vector3_3);
            Vector3 vector3_4 = vector3_2 - vector3_1 * (num / Vector3.Dot(vector3_1, vector3_1));
            return Vector3.Dot(vector3_4, vector3_4);
        }

        public static int GetCursorIndexFromPosition(TMP_TextInfo textInfo, Vector2 position, out CaretPosition cursor) {
            int nearestLine = FindNearestLine(textInfo, position);
            int nearestCharacterOnLine = FindNearestCharacterOnLine(textInfo, position, nearestLine, false);

            if (textInfo.lineInfo[nearestLine].characterCount == 1) {
                cursor = CaretPosition.Left;
                return nearestCharacterOnLine;
            }

            TMP_CharacterInfo tmpCharacterInfo = textInfo.characterInfo[nearestCharacterOnLine];

            Vector3 vector3_1 = tmpCharacterInfo.bottomLeft;
            Vector3 vector3_2 = tmpCharacterInfo.topRight;

            if ((position.x - vector3_1.x) / (vector3_2.x - vector3_1.x) < 0.5) {
                cursor = CaretPosition.Left;
                return nearestCharacterOnLine;
            }

            cursor = CaretPosition.Right;
            return nearestCharacterOnLine;
        }

    }

}