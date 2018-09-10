using System;
using System.Collections;
using JetBrains.Annotations;
using Src;
using Src.Input;
using Src.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

    // <InputField type="Text" placeholder="" onValueChanged={} onFocus={} onBlur="{}" onMouseDown={} onKeyUp={}/>

    // todo -- enforce 1 child 
//    [SingleChild(typeof(UITextElement))]

    [AcceptFocus]
    [Template("Templates/InputField.xml")]
    public class UIInputFieldElement : UITextContainerElement, IDirectDrawMesh, IFocusable {

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
        public bool m_AllowInput;
        public bool m_CaretVisible;
        public float m_BlinkStartTime;
        private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);
        private float m_CaretBlinkRate = 0.85f;
        private Coroutine m_BlinkCoroutine = null;
        public float m_CaretWidth = 1;
        public string m_OriginalText;
        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;
        private bool isStringPositionDirty;

        public TMP_InputField.LineType lineType;

        private static readonly VertexHelper s_VertexHelper = new VertexHelper();

        public Mesh mesh;

        private readonly UIVertex[] cursorVertices;

        public UIInputFieldElement() {
            mesh = new Mesh();
            m_Text = string.Empty;
            cursorVertices = new[] {
                UIVertex.simpleVert,
                UIVertex.simpleVert,
                UIVertex.simpleVert,
                UIVertex.simpleVert
            };
        }

        public override void OnCreate() {
            //caret = template.FindById<Graphic>("caret");
            //caret.Enabled = true;
            //caret.transform.position = position;
            //caret.transform.scale.x = caretWidth;
            // template.FindByType<>();
            // template.FindByName();
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
            set {
                if (m_Text == value) {
                    return;
                }

                m_Text = value ?? string.Empty;

                if (m_StringPosition > m_Text.Length) {
                    m_StringPosition = m_Text.Length;
                    m_StringSelectPosition = m_Text.Length;
                }

                SendOnValueChangedAndUpdateLabel();
            }
        }

        // validators?
        // formatters?

        public override void OnUpdate() {
            UpdateMesh();
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

        private void UpdateMesh() {
            if (isStringPositionDirty) { }

//            if (!hasSelection) {
//                GenerateCaret(s_VertexHelper);
//            }
//            else {
//                GenerateHighlight(s_VertexHelper);
//            }

            GenerateCaret(s_VertexHelper);
//
            s_VertexHelper.FillMesh(mesh);
            onMeshUpdate?.Invoke(this, mesh);
            s_VertexHelper.Clear();
        }

        // width, text info, charInfo[], caret position, string position
        private void GenerateCaret(VertexHelper vbo) {

            float width = m_CaretWidth;

            int characterCount = textInfo.characterCount;
            Vector2 startPosition = Vector2.zero;
            float height = 0;
            TMP_CharacterInfo currentCharacter;
            caretColor = Color.black;

            // Get the position of the Caret based on position in the string.
            int caretPosition = Mathf.Max(0, GetCaretPositionFromStringIndex(stringPositionInternal));

            if (caretPosition == 0) {
                currentCharacter = textInfo.characterInfo[0];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else if (caretPosition < characterCount) {
                currentCharacter = textInfo.characterInfo[caretPosition];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else {
                currentCharacter = textInfo.characterInfo[characterCount - 1];
                startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }

            // Clamp Caret height
            float top = startPosition.y + height;
            float bottom = top - height;

            Rect rect = new Rect(startPosition.x, top, width, bottom);
            cursorVertices[0].position = new Vector3(startPosition.x, bottom, 0.0f);
            cursorVertices[1].position = new Vector3(startPosition.x, top, 0.0f);
            cursorVertices[2].position = new Vector3(startPosition.x + width, top, 0.0f);
            cursorVertices[3].position = new Vector3(startPosition.x + width, bottom, 0.0f);

            // Set Vertex Color for the caret color.
            cursorVertices[0].color = caretColor;
            cursorVertices[1].color = caretColor;
            cursorVertices[2].color = caretColor;
            cursorVertices[3].color = caretColor;

            vbo.AddUIVertexQuad(cursorVertices);

            int screenHeight = Screen.height;
            // Removed multiple display support until it supports none native resolutions(case 741751)
            //int displayIndex = m_TextComponent.canvas.targetDisplay;
            //if (Screen.fullScreen && displayIndex < Display.displays.Length)
            //    screenHeight = Display.displays[displayIndex].renderingHeight;

            startPosition.y = screenHeight - startPosition.y;
            Input.compositionCursorPos = startPosition;

        }

        [OnMouseDown]
        private void OnMouseDown(MouseInputEvent evt) {
            Debug.Log(evt.mousePosition);
        }

        private void GenerateHighlight(VertexHelper vbo) {
            caretPositionInternal = m_CaretPosition = GetCaretPositionFromStringIndex(stringPositionInternal);
            caretSelectPositionInternal = m_CaretSelectPosition = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            //Debug.Log("StringPosition:" + caretPositionInternal + "  StringSelectPosition:" + caretSelectPositionInternal);

            Vector2 caretPosition;
            float height = 0;
            if (caretSelectPositionInternal < textInfo.characterCount) {
                caretPosition = new Vector2(textInfo.characterInfo[caretSelectPositionInternal].origin, textInfo.characterInfo[caretSelectPositionInternal].descender);
                height = textInfo.characterInfo[caretSelectPositionInternal].ascender - textInfo.characterInfo[caretSelectPositionInternal].descender;
            }
            else {
                caretPosition = new Vector2(textInfo.characterInfo[caretSelectPositionInternal - 1].xAdvance, textInfo.characterInfo[caretSelectPositionInternal - 1].descender);
                height = textInfo.characterInfo[caretSelectPositionInternal - 1].ascender - textInfo.characterInfo[caretSelectPositionInternal - 1].descender;
            }

            // TODO: Don't adjust the position of the RectTransform if Reset On Deactivation is disabled
            // and we just selected the Input Field again.
            //  AdjustRectTransformRelativeToViewport(caretPosition, height, true);

            int startChar = Mathf.Max(0, caretPositionInternal);
            int endChar = Mathf.Max(0, caretSelectPositionInternal);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar) {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;

            //Debug.Log("Updating Highlight... Caret Position: " + startChar + " Caret Select POS: " + endChar);


            int currentLineIndex = textInfo.characterInfo[startChar].lineNumber;
            int nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = m_SelectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < textInfo.characterCount) {
                if (currentChar == nextLineStartIdx || currentChar == endChar) {
                    TMP_CharacterInfo startCharInfo = textInfo.characterInfo[startChar];
                    TMP_CharacterInfo endCharInfo = textInfo.characterInfo[currentChar];

                    // Extra check to handle Carriage Return
                    if (currentChar > 0 && endCharInfo.character == 10 && textInfo.characterInfo[currentChar - 1].character == 13) {
                        endCharInfo = textInfo.characterInfo[currentChar - 1];
                    }

                    Vector2 startPosition = new Vector2(startCharInfo.origin, textInfo.lineInfo[currentLineIndex].ascender);
                    Vector2 endPosition = new Vector2(endCharInfo.xAdvance, textInfo.lineInfo[currentLineIndex].descender);

                    var startIndex = vbo.currentVertCount;
                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                    vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    if (currentLineIndex < textInfo.lineCount) {
                        nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;
                    }
                }

                currentChar++;
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
            HandleDeleteKey();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit) {
                return;
            }

            m_Text = text.Insert(m_StringPosition, replaceString);
            stringSelectPositionInternal = stringPositionInternal += replaceString.Length;
        }


        private void SendOnValueChangedAndUpdateLabel() {
            UpdateLabel();
        }

        protected void UpdateLabel() {
            string fullText;
            if (Input.compositionString.Length > 0) {
                fullText = text.Substring(0, m_StringPosition) + Input.compositionString + text.Substring(m_StringPosition);
            }
            else {
                fullText = text;
            }

            string processed = fullText;

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

        public Mesh GetMesh() {
            return mesh;
        }

        public event Action<FocusEvent> onFocus;
        public event Action<BlurEvent> onBlur;

        public bool HasFocus { get; }
        public bool HasFocusLocked { get; }

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
            int count = textInfo.characterCount;

            for (int i = 0; i < count; i++) {
                if (textInfo.characterInfo[i].index >= stringIndex) {
                    return i;
                }
            }

            return count;
        }

        private int GetStringIndexFromCaretPosition(int caretPosition) {
            return textInfo.characterInfo[ClampCaretPosition(m_TextInfo.characterCount, caretPosition)].index;
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