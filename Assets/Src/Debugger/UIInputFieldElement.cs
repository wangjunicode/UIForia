using System;
using JetBrains.Annotations;
using Src;
using Src.Elements;
using Src.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

// todo -- enforce 1 child 
//    [SingleChild(typeof(UITextElement))]

    [Template("Templates/InputField.xml")]
    public class UIInputFieldElement : UIElement, IFocusable {

        protected enum EditState {

            Continue,
            Finish

        }

        private TMP_TextInfo m_TextInfo;
        private TMP_FontAsset m_FontAsset;

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
        public bool m_AllowInput;
        public bool m_CaretVisible;
        public float m_BlinkStartTime;
        private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);
        private float m_CaretBlinkRate = 0.85f;
        public float m_CaretWidth = 1;
        public string m_OriginalText;
        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;
        private bool isStringPositionDirty;

        public TMP_InputField.LineType lineType;

        private static readonly VertexHelper s_VertexHelper = new VertexHelper();
        private static readonly UIVertex[] s_Vertices = new UIVertex[4];

        private UIGraphicElement caret;
        private UIGraphicElement highlight;
        private UITextContainerElement textElement;

        public UIInputFieldElement() {
            m_Text = string.Empty;
            isMulitLine = true;
        }

        public override void OnCreate() {
            caret = FindById<UIGraphicElement>("cursor");
            highlight = FindById<UIGraphicElement>("highlight");
            textElement = FindById<UITextContainerElement>("text");
            m_TextInfo = textElement.textInfo;
            m_FontAsset = textElement.fontAsset;
            caret.rebuildGeometry = UpdateCaretVertices;
            UpdateLabel();
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        protected int stringPositionInternal {
            get { return m_StringPosition + Input.compositionString.Length; }
            set { m_StringPosition = ClampStringPosition(m_Text, value); }
        }

        protected int stringSelectPositionInternal {
            get { return m_StringSelectPosition + Input.compositionString.Length; }
            set { m_StringSelectPosition = ClampStringPosition(m_Text, value); }
        }

        protected int caretPositionInternal {
            get { return m_CaretPosition + Input.compositionString.Length; }
            set { m_CaretPosition = ClampCaretPosition(m_TextInfo.characterCount, value); }
        }

        protected int caretSelectPositionInternal {
            get { return m_CaretSelectPosition + Input.compositionString.Length; }
            set { m_CaretSelectPosition = ClampCaretPosition(m_TextInfo.characterCount, value); }
        }

        public string text {
            get { return m_Text; }
            set { SetText(value); }
        }

        // validators?
        // formatters?

        private void SetText(string value) {
            if (m_Text == value) {
                return;
            }

            m_Text = value ?? string.Empty;

            if (m_StringPosition > m_Text.Length) {
                m_StringPosition = m_Text.Length;
                m_StringSelectPosition = m_Text.Length;
            }

            UpdateLabel();
        }

        public override void OnUpdate() {
            // if (m_CaretVisible) {
            float blinkPeriod = 1f / m_CaretBlinkRate;
            bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
            // caret.SetEnabled(blinkState);
            // }
        }

        private void SetCaretActive() {
            if (!m_AllowInput) return;

            if (m_CaretBlinkRate > 0f) { }
        }

        private void UpdateCaretVertices(Mesh mesh) {
            Rect caretRect = GetCaretRect();

            s_Vertices[0].position = new Vector3(caretRect.x, caretRect.height, 0.0f);
            s_Vertices[1].position = new Vector3(caretRect.x, caretRect.y, 0.0f);
            s_Vertices[2].position = new Vector3(caretRect.x + caretRect.width, caretRect.y, 0.0f);
            s_Vertices[3].position = new Vector3(caretRect.x + caretRect.width, caretRect.height, 0.0f);

            s_Vertices[0].color = caretColor;
            s_Vertices[1].color = caretColor;
            s_Vertices[2].color = caretColor;
            s_Vertices[3].color = caretColor;

            s_VertexHelper.AddUIVertexQuad(s_Vertices);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();

            Vector2 startPosition = new Vector2(caretRect.x, caretRect.y);
            startPosition.y = Screen.height - startPosition.y;
            Input.compositionCursorPos = startPosition;
        }

        [UsedImplicitly]
        [OnKeyDown(KeyCode.Backspace)]
        private void HandleBackspace() {
            if (isReadOnly) {
                return;
            }

            if (hasSelection) {
                HandleDeleteKey();
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

        private Rect GetCaretRect() {
            float width = m_CaretWidth;

            int characterCount = m_TextInfo.characterCount;
            Vector2 startPosition = Vector2.zero;
            float height = 0;
            TMP_CharacterInfo currentCharacter;
            caretColor = Color.black;

            // Get the position of the Caret based on position in the string.
            caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);

            if (caretPositionInternal == 0) {
                currentCharacter = m_TextInfo.characterInfo[0];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else if (caretPositionInternal < characterCount) {
                currentCharacter = m_TextInfo.characterInfo[caretPositionInternal];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else {
                currentCharacter = m_TextInfo.characterInfo[characterCount - 1];
                startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }

            float top = startPosition.y + height;
            float bottom = top - height;

            return new Rect(startPosition.x, top, width, bottom);
        }

        [OnMouseDown]
        private void OnMouseDown(MouseInputEvent evt) {
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

        protected virtual void Append(char input) {
            if (isReadOnly) {
                return;
            }

            // Append the character and update the label
            Insert(input);
        }


        // Insert the character and update the label.
        private void Insert(char c) {
            if (isReadOnly || !m_FontAsset.HasCharacter(c)) {
                return;
            }

            string replaceString = c.ToString();
           // HandleDeleteKey();

            // Can't go past the character limit
            if (characterLimit > 0 && m_Text.Length >= characterLimit) {
                return;
            }

            if (m_Text == string.Empty) {
                m_Text = replaceString;
                stringPositionInternal = replaceString.Length;
                stringSelectPositionInternal = replaceString.Length;
            }
            else {
                m_Text = m_Text.Insert(m_StringPosition, replaceString);
                stringSelectPositionInternal = stringPositionInternal += replaceString.Length;
            }

            UpdateLabel();
        }

        private void SendOnValueChangedAndUpdateLabel() {
            UpdateLabel();
        }

        protected void UpdateLabel() {
            string fullText;
            if (Input.compositionString.Length > 0) {
                fullText = m_Text.Substring(0, m_StringPosition) + Input.compositionString + m_Text.Substring(m_StringPosition);
            }
            else {
                fullText = m_Text;
            }

            string processed = fullText;

            // If not currently editing the text, set the visible range to the whole text.
            // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
            // We can't do this when text is being edited since it would discard the current scroll,
            // which is defined by means of the m_DrawStart and m_DrawEnd indices.

            if (!string.IsNullOrEmpty(fullText)) {
                SetCaretVisible();
            }

            textElement.SetText(processed + "\u200B");

            caret.MarkGeometryDirty();
        }

        void SetCaretVisible() {
            if (!m_AllowInput) {
                return;
            }

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            //SetCaretActive();
        }

        private string GetSelectedString() {
            if (!hasSelection) return string.Empty;

            int startPos = stringPositionInternal;
            int endPos = stringSelectPositionInternal;

            if (startPos > endPos) {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            return text.Substring(startPos, endPos - startPos);
        }

        public event Action<FocusEvent> onFocus;
        public event Action<BlurEvent> onBlur;

        public bool HasFocus { get; }
        public bool HasFocusLocked { get; }

        [OnDragCreate]
        public TextSelectDragEvent CreateDragEvent() {
            TextSelectDragEvent evt = new TextSelectDragEvent();
            evt.onUpdate += HandleDragUpdate;
            return evt;
        }

        private void HandleDragUpdate(DragEvent evt) {
                Debug.Log("UPDATING");
        }
        
        [OnDragMove(typeof(TextSelectDragEvent))]
        public void OnDragMove() {
            Debug.Log("Drag moved");
        }

        public class TextSelectDragEvent : DragEvent { }

        public void Focus() {
            Debug.Log("FOCUSED");
            onFocus?.Invoke(new FocusEvent());
            Input.imeCompositionMode = IMECompositionMode.On;
            m_AllowInput = true;
            m_OriginalText = text;
            m_WasCanceled = false;
            SetCaretVisible();
            UpdateLabel();
        }

        public void Blur() {
            Debug.Log("BLURRED");
            onBlur?.Invoke(new BlurEvent());
        }


        private int GetCaretPositionFromStringIndex(int stringIndex) {
            int count = m_TextInfo.characterCount;

            for (int i = 0; i < count; i++) {
                if (m_TextInfo.characterInfo[i].index >= stringIndex) {
                    return i;
                }
            }

            return count;
        }

        private int GetStringIndexFromCaretPosition(int caretPosition) {
            return m_TextInfo.characterInfo[ClampCaretPosition(m_TextInfo.characterCount, caretPosition)].index;
        }

        private static int ClampStringPosition(string text, int pos) {
            if (pos < 0) {
                return 0;
            }

            if (pos > text.Length) {
                return text.Length;
            }

            return pos;
        }

        private static int ClampCaretPosition(int characterCount, int pos) {
            if (characterCount == 0 || pos < 0) {
                return 0;
            }

            if (pos > characterCount - 1) {
                return characterCount - 1;
            }

            return pos;
        }

    }

}