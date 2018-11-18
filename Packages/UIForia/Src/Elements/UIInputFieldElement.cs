using System;
using UIForia.Rendering;
using UIForia;
using UIForia.Elements;
using UIForia.Input;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

[Template(TemplateType.String, @"
<UITemplate>
    
    <Style>
        
        style ignored {
            LayoutBehavior = Ignored;
            TransformBehaviorX = AnchorMinOffset;
            TransformBehaviorY = AnchorMinOffset;
            AnchorTarget = Parent;
        }
        
        style text {
            TextFontSize = 24;
            MaxWidth = 1pca; 
        }

        style highlight {
            LayoutBehavior = Ignored;
            TransformBehaviorX = AnchorMinOffset;
            TransformBehaviorY = AnchorMinOffset;
            AnchorTarget = ParentContentArea;
            BackgroundColor = #A8CEFF;
        }
        
        style container {
            FlexLayoutMainAxisAlignment = Center;
        }

    </Style>
    
    <Contents style='container'>
        
        <Graphic x-id=""highlight"" style=""highlight""/>
        
        <Text style='text' x-id=""text""/>
        
        <Graphic x-id='cursor' style='ignored'/>
        
</Contents>

</UITemplate>
")]
public class UIInputFieldElement : UIElement, IFocusable, IPropertyChangedHandler {

    public string text;
    public string placeholder;
    public bool selectAllOnFocus;
    public float caretBlinkRate = 0.85f;
    public event Action<string> onValueChanged;
    
    private UIGraphicElement caret;
    private UIGraphicElement highlight;
    private UITextElement textElement;
    private UITextElement.SelectionRange selectionRange = new UITextElement.SelectionRange(0, UITextElement.TextEdge.Right);
    private UITextElement.SelectionRange previousSelectionRange = new UITextElement.SelectionRange(0, UITextElement.TextEdge.Right);

    private float blinkStartTime;
    private bool hasFocus;
    private bool canSetCaret;

    protected static string clipboard {
        get { return GUIUtility.systemCopyBuffer; }
        set { GUIUtility.systemCopyBuffer = value; }
    }

    [OnPropertyChanged(nameof(text))]
    public void OnTextPropChanged(string field) {
        textElement?.SetText(text);
    }
    
    public string GetText() {
        return textElement.text;
    }
    
    public override void OnCreate() {
        text = text ?? string.Empty;
        caret = FindById<UIGraphicElement>("cursor");
        highlight = FindById<UIGraphicElement>("highlight");
        textElement = FindById<UITextElement>("text");
        
        caret.rebuildGeometry = UpdateCaretVertices;
        highlight.rebuildGeometry = UpdateHighlightVertices;
        caret.SetEnabled(false);
        highlight.SetEnabled(false);
        textElement.SetText(text);
    }

    public override void OnUpdate() {
        
        if (!hasFocus) return;

        if (selectionRange != previousSelectionRange) {
            if (textElement.text.Length > 0) {
                if (selectionRange.HasSelection) {
                    highlight.SetEnabled(true);
                    highlight.MarkGeometryDirty();
                }
                else {
                    highlight.SetEnabled(false);
                }
            }

            blinkStartTime = Time.unscaledTime;
            previousSelectionRange = selectionRange;
        }

        float blinkPeriod = 1f / caretBlinkRate;
        bool blinkState = (Time.unscaledTime - blinkStartTime) % blinkPeriod < blinkPeriod / 2;
        if (canSetCaret) {
            caret.style.SetTransformPositionX(layoutResult.contentRect.x + textElement.GetCursorPosition(selectionRange).x, StyleState.Normal);
            caret.style.SetTransformPositionY(textElement.GetCursorPosition(selectionRange).y, StyleState.Normal);
//            caret.style.SetTransformPositionX(layoutResult.contentRect.x + textElement.GetCursorPosition(selectionRange).x, StyleState.Normal);
//            caret.style.SetTransformPositionY(textElement.layoutResult.localPosition.y, StyleState.Normal);
        }
        caret.style.SetBackgroundColor(Color.black, StyleState.Normal);
        caret.SetEnabled(blinkState);
        canSetCaret = true;
    }

    [OnKeyDown]
    private void EnterText(KeyboardInputEvent evt) {
        char c = evt.character;

        if (!textElement.style.TextFontAsset.characterDictionary.ContainsKey(c)) {
            return;
        }

        if (selectionRange.HasSelection) {
            selectionRange = textElement.DeleteRange(selectionRange);
            textElement.InsertText(selectionRange, c);
        }
        else {
            selectionRange = textElement.AppendText(c);
        }

        onValueChanged?.Invoke(textElement.text);
        
        canSetCaret = false;
    }

    [OnMouseDown]
    private void OnMouseDown(MouseInputEvent evt) {
        bool hadFocus = hasFocus;

        if (evt.IsConsumed || (!hasFocus && !Input.RequestFocus(this))) {
            return;
        }

        if (!hadFocus && selectAllOnFocus) {
            return;
        }

        evt.StopPropagation();

        Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.contentRect.position;
        
        if (evt.IsDoubleClick) {
            selectionRange = textElement.SelectWordAtPoint(mouse);
        }
        else if (evt.Shift) {
            selectionRange = textElement.SelectToPoint(selectionRange, mouse);
        }
        else {
            selectionRange = textElement.GetSelectionAtPoint(mouse);
        }
    }

    [OnKeyDownWithFocus(KeyCode.A, KeyboardModifiers.Control)]
    private void HandleSelectAll(KeyboardInputEvent evt) {
        if (evt.onlyControl) {
            SelectAll();
            evt.StopPropagation();
            canSetCaret = false;
        }
    }

    [OnKeyDownWithFocus(KeyCode.C, KeyboardModifiers.Control)]
    private void HandleCopy(KeyboardInputEvent evt) {
        if (evt.onlyControl && selectionRange.HasSelection) {
            clipboard = textElement.GetSubstring(selectionRange);
            evt.StopPropagation();
            canSetCaret = false;
        }
    }

    [OnKeyDownWithFocus(KeyCode.V, KeyboardModifiers.Control)]
    private void HandlePaste(KeyboardInputEvent evt) {
        if (evt.onlyControl) {
            textElement.AppendText(clipboard);
            evt.StopPropagation();
            canSetCaret = false;
        }
    }

    [OnKeyDownWithFocus(KeyCode.X, KeyboardModifiers.Control)]
    private void HandleCut(KeyboardInputEvent evt) {
        if (evt.onlyControl && selectionRange.HasSelection) {
            clipboard = textElement.GetSubstring(selectionRange);
            DeleteSelection();
            canSetCaret = false;
        }
    }
    
    [OnKeyDownWithFocus(KeyCode.Backspace)]
    private void HandleBackspace(KeyboardInputEvent evt) {
        selectionRange = textElement.DeleteTextBackwards(selectionRange);
        canSetCaret = false;
    }
    
    [OnKeyDownWithFocus(KeyCode.Delete)]
    private void HandleDelete(KeyboardInputEvent evt) {
        selectionRange = textElement.DeleteTextForwards(selectionRange);
        canSetCaret = false;
    }
    
    [OnKeyDownWithFocus(KeyCode.LeftArrow)]
    private void HandleLeftArrow(KeyboardInputEvent evt) {
        selectionRange = textElement.MoveCursorLeft(selectionRange, evt.shift);
        canSetCaret = false;
    }
    
    [OnKeyDownWithFocus(KeyCode.RightArrow)]
    private void HandleRightArrow(KeyboardInputEvent evt) {
        selectionRange = textElement.MoveCursorRight(selectionRange, evt.shift);
        canSetCaret = false;
    }

    private void SelectAll() {
        selectionRange = textElement.SelectAll();
        canSetCaret = false;
    }

    private void DeleteSelection() { }

    [OnDragCreate]
    public TextSelectDragEvent CreateDragEvent(MouseInputEvent evt) {
        TextSelectDragEvent retn = new TextSelectDragEvent(this);
        retn.onUpdate += HandleDragUpdate;
        Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.contentRect.position;
        selectionRange = textElement.BeginSelection(mouse);
        return retn;
    }

    private void HandleDragUpdate(DragEvent obj) {
        Vector2 mouse = obj.MousePosition - layoutResult.screenPosition - layoutResult.contentRect.position;
        selectionRange = textElement.SelectToPoint(selectionRange, mouse);
    }

    private void UpdateHighlightVertices(Mesh obj) {
        if (selectionRange.HasSelection) {
            highlight.SetMesh(textElement.GetHighlightMesh(selectionRange));
        }
    }

    private void UpdateCaretVertices(Mesh obj) {
        caret.SetMesh(MeshUtil.CreateStandardUIMesh(new Size(1f, textElement.GetLineHeight())));
    }

    public void Focus() {
        hasFocus = true;
        caret.SetEnabled(true);
        caret.MarkGeometryDirty();
    }

    public void Blur() {
        hasFocus = false;
        caret.SetEnabled(false);
        highlight.SetEnabled(false);
    }

    public void OnPropertyChanged(string propertyName, object oldValue) {
        if (textElement == null) return;

        if (propertyName == nameof(text)) {
            textElement.SetText(text);
            selectionRange = textElement.ValidateSelectionRange(selectionRange);
        }
    }

    public override string GetDisplayName() {
        return "TextInput";
    }

    public class TextSelectDragEvent : CallbackDragEvent {

        public TextSelectDragEvent(UIElement origin) : base(origin) { }

    }

}
//
//[Template("Templates/InputField.xml")]
//public class UIInputFieldElement : UIElement, IFocusable {
//
//    public class TextSelectDragEvent : CallbackDragEvent { }
//
//    private static readonly char[] kSeparators = {' ', '.', ',', '\t', '\r', '\n'};
//
//    private TMP_TextInfo m_TextInfo;
//    private TMP_FontAsset m_FontAsset;
//
//    public string placeholder;
//    public Color caretColor;
//    public Color highlightColor;
//    public bool showPlaceholder = false;
//    public bool isReadOnly;
//    public bool isMulitLine;
//    public bool isPasswordField;
//    public bool isRichTextEditingAllowed;
//    public bool m_isLastKeyBackspace;
//    public bool m_WasCanceled;
//    public bool m_isSelectAll;
//    public int characterLimit;
//    public string m_Text;
//    public bool m_AllowInput;
//    public bool m_CaretVisible;
//    public float m_BlinkStartTime;
//    public bool m_SeamlessHighlight = true;
//    private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);
//    private float m_CaretBlinkRate = 0.85f;
//    public float m_CaretWidth = 1;
//    public string m_OriginalText;
//    protected int m_StringPosition = 0;
//    protected int m_StringSelectPosition = 0;
//    protected int m_CaretPosition = 0;
//    protected int m_CaretSelectPosition = 0;
//    private bool isStringPositionDirty;
//    private bool m_IsFocused;
//    public bool m_OnFocusSelectAll;
//    public bool multiLine = true;
//
//    public TMP_InputField.LineType lineType;
//
//    private static readonly VertexHelper s_VertexHelper = new VertexHelper();
//    private static readonly UIVertex[] s_Vertices = new UIVertex[4];
//
//    private UIGraphicElement caret;
//    private UIGraphicElement highlight;
//    private UITextContainerElement textElement;
//
//    public UIInputFieldElement() {
//        m_Text = string.Empty;
//        isMulitLine = true;
//    }
//
//    public override void OnCreate() {
//        caret = FindById<UIGraphicElement>("cursor");
//        highlight = FindById<UIGraphicElement>("highlight");
//        textElement = FindById<UITextContainerElement>("text");
//        m_TextInfo = textElement.textInfo;
//        m_FontAsset = textElement.fontAsset;
//        caret.rebuildGeometry = UpdateCaretVertices;
//        highlight.rebuildGeometry = UpdateHighlightVertices;
//        UpdateLabel();
//    }
//
//    protected static string clipboard {
//        get { return GUIUtility.systemCopyBuffer; }
//        set { GUIUtility.systemCopyBuffer = value; }
//    }
//
//    protected static string compositionString => UnityEngine.Input.compositionString;
//
//    protected int stringPositionInternal {
//        get { return m_StringPosition + compositionString.Length; }
//        set { m_StringPosition = ClampStringPosition(m_Text, value); }
//    }
//
//    protected int stringSelectPositionInternal {
//        get { return m_StringSelectPosition + compositionString.Length; }
//        set { m_StringSelectPosition = ClampStringPosition(m_Text, value); }
//    }
//
//    protected int caretPositionInternal {
//        get { return m_CaretPosition + compositionString.Length; }
//        set { m_CaretPosition = ClampCaretPosition(m_TextInfo.characterCount, value); }
//    }
//
//    protected int caretSelectPositionInternal {
//        get { return m_CaretSelectPosition + compositionString.Length; }
//        set { m_CaretSelectPosition = ClampCaretPosition(m_TextInfo.characterCount, value); }
//    }
//
//    protected bool hasSelection => stringPositionInternal != stringSelectPositionInternal;
//
//    public string text {
//        get { return m_Text; }
//        set { SetText(value); }
//    }
//
//    public override void OnUpdate() {
//        if (!m_IsFocused || hasSelection) {
//            return;
//        }
//
//        float blinkPeriod = 1f / m_CaretBlinkRate;
//        bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
//        if (m_CaretVisible != blinkState) {
//            m_CaretVisible = blinkState;
//            caret.SetEnabled(blinkState);
//        }
//    }
//
//    // validators?
//    // formatters?
//
//    private void SetText(string value) {
//        if (m_Text == value) {
//            return;
//        }
//
//        m_Text = value ?? string.Empty;
//
//        if (m_StringPosition > m_Text.Length) {
//            m_StringPosition = m_Text.Length;
//            m_StringSelectPosition = m_Text.Length;
//        }
//
//        UpdateLabel();
//    }
//
//    private void Delete() {
//        if (isReadOnly) {
//            return;
//        }
//
//        if (stringPositionInternal == stringSelectPositionInternal) {
//            return;
//        }
//
//        if (isRichTextEditingAllowed || m_isSelectAll) {
//            // Handling of Delete when Rich Text is allowed.
//            if (stringPositionInternal < stringSelectPositionInternal) {
//                m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);
//                stringSelectPositionInternal = stringPositionInternal;
//            }
//            else {
//                m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
//                stringPositionInternal = stringSelectPositionInternal;
//            }
//
//            m_isSelectAll = false;
//        }
//        else {
//            stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
//            stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
//
//            // Handling of Delete when Rich Text is not allowed.
//            if (caretPositionInternal < caretSelectPositionInternal) {
//                m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);
//
//                stringSelectPositionInternal = stringPositionInternal;
//                caretSelectPositionInternal = caretPositionInternal;
//            }
//            else {
//                m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
//                stringPositionInternal = stringSelectPositionInternal;
//
//                stringPositionInternal = stringSelectPositionInternal;
//                caretPositionInternal = caretSelectPositionInternal;
//            }
//        }
//    }
//
//    [UsedImplicitly]
//    [OnKeyDown(KeyCode.Backspace)]
//    private void HandleBackspace() {
//        if (isReadOnly) {
//            return;
//        }
//
//        if (hasSelection) {
//            Delete();
//        }
//        else {
//            if (isRichTextEditingAllowed) {
//                if (stringPositionInternal > 0) {
//                    m_Text = text.Remove(stringPositionInternal - 1, 1);
//                    stringSelectPositionInternal = stringPositionInternal = stringPositionInternal - 1;
//
//                    m_isLastKeyBackspace = true;
//                }
//            }
//            else {
//                if (caretPositionInternal > 0) {
//                    m_Text = text.Remove(GetStringIndexFromCaretPosition(caretPositionInternal - 1), 1);
//                    caretSelectPositionInternal = caretPositionInternal = caretPositionInternal - 1;
//                    stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
//                }
//
//                m_isLastKeyBackspace = true;
//            }
//        }
//
//        UpdateLabel();
//        UpdateGeometry();
//    }
//
//    [UsedImplicitly]
//    [OnKeyDown(KeyCode.Delete)]
//    private void HandleDeleteKey() {
//        if (isReadOnly) {
//            return;
//        }
//
//        if (hasSelection) {
//            Delete();
//        }
//        else {
//            if (isRichTextEditingAllowed) {
//                if (stringPositionInternal < text.Length) {
//                    m_Text = text.Remove(stringPositionInternal, 1);
//                }
//            }
//            else {
//                if (caretPositionInternal < m_TextInfo.characterCount - 1) {
//                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
//                    stringPositionInternal = stringSelectPositionInternal;
//                    m_Text = text.Remove(stringPositionInternal, 1);
//                }
//            }
//        }
//
//        UpdateLabel();
//        UpdateGeometry();
//    }
//
//    [OnKeyDownWithFocus(KeyCode.A)]
//    private void HandleSelectAll(KeyboardInputEvent evt) {
//        if (evt.onlyControl) {
//            SelectAll();
//            UpdateGeometry();
//            evt.StopPropagation();
//        }
//    }
//
//    [OnKeyDownWithFocus(KeyCode.C)]
//    private void HandleCopy(KeyboardInputEvent evt) {
//        if (evt.onlyControl) {
//            clipboard = GetSelectedString();
//            evt.StopPropagation();
//        }
//    }
//
//    [OnKeyDownWithFocus(KeyCode.V)]
//    private void HandlePaste(KeyboardInputEvent evt) {
//        if (evt.onlyControl) {
//            Append(clipboard);
//            evt.StopPropagation();
//        }
//    }
//
//    [OnKeyDownWithFocus(KeyCode.X)]
//    private void HandleCut(KeyboardInputEvent evt) {
//        if (evt.onlyControl) {
//            clipboard = GetSelectedString();
//            Delete();
//            UpdateLabel();
//            UpdateGeometry();
//        }
//    }
//
//    [OnKeyDownWithFocus(KeyCode.Return)]
//    [OnKeyDownWithFocus(KeyCode.KeypadEnter)]
//    private void HandleEnter(KeyboardInputEvent evt) {
//        if (isMulitLine) {
//            Append('\n');
//            evt.StopPropagation();
//        }
//        else {
////            Input.Submit();
//        }
//    }
//
//    [OnKeyDownWithFocus(KeyCode.LeftArrow)]
//    private void HandleLeftArrow(KeyboardInputEvent evt) {
//        MoveLeft(evt.shift, evt.ctrl);
//        UpdateGeometry();
//        evt.StopPropagation();
//    }
//
//    [OnKeyDownWithFocus(KeyCode.RightArrow)]
//    private void HandleRightArrow(KeyboardInputEvent evt) {
//        MoveRight(evt.shift, evt.ctrl);
//        UpdateGeometry();
//        evt.StopPropagation();
//    }
//
//    [OnKeyDownWithFocus(KeyCode.UpArrow)]
//    private void HandleUpArrow(KeyboardInputEvent evt) {
//        MoveUp(evt.shift, true);
//        UpdateGeometry();
//        evt.StopPropagation();
//    }
//
//    [OnKeyDownWithFocus(KeyCode.DownArrow)]
//    private void HandleDownArrow(KeyboardInputEvent evt) {
//        MoveDown(evt.shift, true);
//        UpdateGeometry();
//        evt.StopPropagation();
//    }
//
//    [OnKeyDownWithFocus(KeyCode.Home)]
//    private void HandleHome(KeyboardInputEvent evt) {
//        MoveToStartOfLine(evt.shift, evt.ctrl);
//        evt.StopPropagation();
//    }
//
//    [OnKeyDownWithFocus(KeyCode.End)]
//    private void HandleEndKey(KeyboardInputEvent evt) {
//        MoveToEndOfLine(evt.shift, evt.ctrl);
//        evt.StopPropagation();
//    }
//
//    public void MoveToStartOfLine(bool shift, bool ctrl) {
//        // Get the line the caret is currently located on.
//        int currentLine = m_TextInfo.characterInfo[caretPositionInternal].lineNumber;
//
//        // Get the last character of the given line.
//        int position = ctrl ? 0 : m_TextInfo.lineInfo[currentLine].firstCharacterIndex;
//
//        position = GetStringIndexFromCaretPosition(position);
//
//        if (shift) {
//            stringSelectPositionInternal = position;
//        }
//        else {
//            stringPositionInternal = position;
//            stringSelectPositionInternal = stringPositionInternal;
//        }
//
//        UpdateLabel();
//        UpdateGeometry();
//    }
//
//    public void MoveToEndOfLine(bool shift, bool ctrl) {
//        // Get the line the caret is currently located on.
//        int currentLine = m_TextInfo.characterInfo[caretPositionInternal].lineNumber;
//
//        // Get the last character of the given line.
//        int position = ctrl ? m_TextInfo.characterCount - 1 : m_TextInfo.lineInfo[currentLine].lastCharacterIndex;
//
//        position = GetStringIndexFromCaretPosition(position);
//
//        if (shift) {
//            stringSelectPositionInternal = position;
//        }
//        else {
//            stringPositionInternal = position;
//            stringSelectPositionInternal = stringPositionInternal;
//        }
//
//        UpdateLabel();
//        UpdateGeometry();
//    }
//
//    private void MoveUp(bool shift, bool goToFirstCharacter) {
//        if (hasSelection && !shift) {
//            // If we have a selection and press up without shift,
//            // set caret position to start of selection before we move it up.
//            caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
//        }
//
//        int position = multiLine ? TextMeshProUtil.LineUpCharacterPosition(m_TextInfo, caretSelectPositionInternal, goToFirstCharacter) : 0;
//
//        if (shift) {
//            caretSelectPositionInternal = position;
//            stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
//        }
//        else {
//            caretSelectPositionInternal = caretPositionInternal = position;
//            stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
//        }
//
//        UpdateLabel();
//        UpdateGeometry();
//    }
//
//    private void MoveDown(bool shift, bool goToLastChar) {
//        if (hasSelection && !shift) {
//            // If we have a selection and press down without shift,
//            // set caret to end of selection before we move it down.
//            caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
//        }
//
//        int position = multiLine ? TextMeshProUtil.LineDownCharacterPosition(m_TextInfo, caretSelectPositionInternal, goToLastChar) : m_TextInfo.characterCount - 1;
//
//        if (shift) {
//            caretSelectPositionInternal = position;
//            stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
//        }
//        else {
//            caretSelectPositionInternal = caretPositionInternal = position;
//            stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
//        }
//    }
//
//    private void MoveRight(bool shift, bool ctrl) {
//        if (hasSelection && !shift) {
//            // By convention, if we have a selection and move right without holding shift,
//            // we just place the cursor at the end.
//            stringPositionInternal = Mathf.Max(stringPositionInternal, stringSelectPositionInternal);
//            stringSelectPositionInternal = stringPositionInternal;
//            caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//            caretSelectPositionInternal = caretPositionInternal;
//
//            return;
//        }
//
//        int position;
//        if (ctrl) {
//            position = FindNextWordBegin();
//        }
//        else {
//            position = isRichTextEditingAllowed ? stringSelectPositionInternal + 1 : GetStringIndexFromCaretPosition(caretSelectPositionInternal + 1);
//        }
//
//        if (shift) {
//            stringSelectPositionInternal = position;
//            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//        }
//        else {
//            stringPositionInternal = position;
//            stringSelectPositionInternal = position;
//            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//            caretPositionInternal = caretSelectPositionInternal;
//        }
//    }
//
//    private void MoveLeft(bool shift, bool ctrl) {
//        if (hasSelection && !shift) {
//            // By convention, if we have a selection and move left without holding shift,
//            // we just place the cursor at the start.
//            stringPositionInternal = stringSelectPositionInternal = Mathf.Min(stringPositionInternal, stringSelectPositionInternal);
//            caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//
//            return;
//        }
//
//        int position;
//        if (ctrl) {
//            position = FindPrevWordBegin();
//        }
//        else {
//            if (isRichTextEditingAllowed) {
//                position = stringSelectPositionInternal - 1;
//            }
//            else {
//                position = GetStringIndexFromCaretPosition(caretSelectPositionInternal - 1);
//            }
//        }
//
//        if (shift) {
//            stringSelectPositionInternal = position;
//            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//        }
//        else {
//            stringPositionInternal = position;
//            stringSelectPositionInternal = position;
//            caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//            caretSelectPositionInternal = caretPositionInternal;
//        }
//    }
//
//    private int FindNextWordBegin() {
//        if (stringSelectPositionInternal + 1 >= text.Length) {
//            return text.Length;
//        }
//
//        int spaceLoc = text.IndexOfAny(kSeparators, stringSelectPositionInternal + 1);
//
//        return spaceLoc == -1 ? text.Length : spaceLoc + 1;
//    }
//
//    private int FindPrevWordBegin() {
//        if (stringSelectPositionInternal - 2 < 0) {
//            return 0;
//        }
//
//        int spaceLoc = text.LastIndexOfAny(kSeparators, stringSelectPositionInternal - 2);
//
//        return spaceLoc == -1 ? 0 : spaceLoc + 1;
//    }
//
//    private Rect GetCaretRect() {
//        float width = m_CaretWidth;
//        int characterCount = m_TextInfo.characterCount;
//        Vector2 startPosition = Vector2.zero;
//        float height = 0;
//        TMP_CharacterInfo currentCharacter;
//        caretColor = Color.black;
//
//        // Get the position of the Caret based on position in the string.
//        caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
//
//        if (caretPositionInternal == 0) {
//            currentCharacter = m_TextInfo.characterInfo[0];
//            startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
//            height = currentCharacter.ascender - currentCharacter.descender;
//        }
//        else if (caretPositionInternal < characterCount) {
//            currentCharacter = m_TextInfo.characterInfo[caretPositionInternal];
//            startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
//            height = currentCharacter.ascender - currentCharacter.descender;
//        }
//        else {
//            currentCharacter = m_TextInfo.characterInfo[characterCount - 1];
//            startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);
//            height = currentCharacter.ascender - currentCharacter.descender;
//        }
//
//        float top = startPosition.y + height;
//        float bottom = top - height;
//
//        return new Rect(startPosition.x, top, width, bottom);
//    }
//
//    [OnMouseDown]
//    private void OnMouseDown(MouseInputEvent evt) {
//        bool hadFocus = m_IsFocused;
//
//        if (evt.IsConsumed || (!m_IsFocused && !Input.RequestFocus(this))) {
//            return;
//        }
//
//        if (!hadFocus && m_OnFocusSelectAll) {
//            return;
//        }
//
//        evt.StopPropagation();
//
//        CaretPosition insertionSide;
//        int insertionIndex = TextMeshProUtil.GetCursorIndexFromPosition(m_TextInfo, evt.MousePosition, out insertionSide);
//
//        if (evt.Shift) {
//            switch (insertionSide) {
//                case CaretPosition.Left:
//                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
//                    break;
//                case CaretPosition.Right:
//                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;
//                    break;
//            }
//        }
//        else {
//            switch (insertionSide) {
//                case CaretPosition.Left:
//                    stringPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
//                    stringSelectPositionInternal = stringPositionInternal;
//                    break;
//                case CaretPosition.Right:
//                    stringPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;
//                    stringSelectPositionInternal = stringPositionInternal;
//                    break;
//            }
//        }
//
//        if (evt.IsDoubleClick) {
//            int wordIndex = TextMeshProUtil.FindIntersectingWord(m_TextInfo, evt.MousePosition);
//
//            if (wordIndex != -1) {
//                // Select current word
//                caretPositionInternal = m_TextInfo.wordInfo[wordIndex].firstCharacterIndex;
//                caretSelectPositionInternal = m_TextInfo.wordInfo[wordIndex].lastCharacterIndex + 1;
//
//                stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
//                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
//            }
//            else {
//                // Select current character
//                caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
//
//                stringSelectPositionInternal += 1;
//                caretSelectPositionInternal = caretPositionInternal + 1;
//                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//            }
//
//            UpdateGeometry();
//        }
//        else {
//            caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
//            SetCaretVisible();
//        }
//    }
//
//    private void UpdateGeometry() {
//        if (hasSelection) {
//            highlight.MarkGeometryDirty();
//            highlight.SetEnabled(true);
//            caret.SetEnabled(false);
//        }
//        else {
//            caret.SetEnabled(true);
//            highlight.SetEnabled(false);
//            caret.MarkGeometryDirty();
//        }
//    }
//
//    [UsedImplicitly]
//    [OnKeyDownWithFocus]
//    private void HandleKeyPress(KeyboardInputEvent evt) {
//        Debug.Log(evt.character);
//        char c = evt.character;
//        if (!isMulitLine && (c == '\t' || c == '\r' || c == 10)) {
//            return;
//        }
//
//        if (c == '\r' || c == 3) {
//            c = '\n';
//        }
//
//        Append(c);
//    }
//
//    protected virtual void Append(char input) {
//        if (isReadOnly) {
//            return;
//        }
//
//        // Append the character and update the label
//        Insert(input);
//    }
//
//    protected virtual void Append(string input) {
//        if (isReadOnly) {
//            return;
//        }
//
//        for (int i = 0; i < input.Length; i++) {
//            char c = input[i];
//
//            if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n') {
//                Append(c);
//            }
//        }
//    }
//
//    // Insert the character and update the label.
//    private void Insert(char c) {
//        if (isReadOnly || !m_FontAsset.HasCharacter(c)) {
//            return;
//        }
//
//        string replaceString = c.ToString();
//        Delete();
//
//        // Can't go past the character limit
//        if (characterLimit > 0 && m_Text.Length >= characterLimit) {
//            return;
//        }
//
//        if (m_Text == string.Empty) {
//            m_Text = replaceString;
//            stringPositionInternal = replaceString.Length;
//            stringSelectPositionInternal = replaceString.Length;
//        }
//        else {
//            m_Text = m_Text.Insert(m_StringPosition, replaceString);
//            stringSelectPositionInternal = stringPositionInternal += replaceString.Length;
//        }
//
//        UpdateLabel();
//        UpdateGeometry();
//    }
//
//    protected void UpdateLabel() {
//        string fullText;
//        if (compositionString.Length > 0) {
//            fullText = m_Text.Substring(0, m_StringPosition) + compositionString + m_Text.Substring(m_StringPosition);
//        }
//        else {
//            fullText = m_Text;
//        }
//
//        textElement.SetText(fullText + "\u200B");
//    }
//
//    private void SetCaretVisible() {
//        if (!m_IsFocused) {
//            return;
//        }
//
//        m_CaretVisible = true;
//        m_BlinkStartTime = Time.unscaledTime;
//        UpdateGeometry();
//    }
//
//    private string GetSelectedString() {
//        if (!hasSelection) return string.Empty;
//
//        int startPos = stringPositionInternal;
//        int endPos = stringSelectPositionInternal;
//
//        if (startPos > endPos) {
//            int temp = startPos;
//            startPos = endPos;
//            endPos = temp;
//        }
//
//        return text.Substring(startPos, endPos - startPos);
//    }
//
//    [OnDragCreate]
//    public TextSelectDragEvent CreateDragEvent() {
//        TextSelectDragEvent evt = new TextSelectDragEvent();
//        evt.onUpdate += HandleDragUpdate;
//        return evt;
//    }
//
//    private void HandleDragUpdate(DragEvent evt) {
//        CaretPosition insertionSide;
//
//        int insertionIndex = TextMeshProUtil.GetCursorIndexFromPosition(m_TextInfo, evt.MousePosition, out insertionSide);
//
//        switch (insertionSide) {
//            case CaretPosition.Left:
//                stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
//                break;
//            case CaretPosition.Right:
//                stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;
//                break;
//        }
//
//        caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//        UpdateGeometry();
//    }
//
//    public void Focus() {
//        if (!Input.RequestFocus(this)) {
//            return;
//        }
//
//        m_IsFocused = true;
//        m_AllowInput = true;
//        m_OriginalText = text;
//        m_WasCanceled = false;
//        SetCaretVisible();
//
//        UnityEngine.Input.imeCompositionMode = IMECompositionMode.On;
//
//        if (m_OnFocusSelectAll) {
//            SelectAll();
//        }
//
//        UpdateLabel();
//    }
//
//    public void Blur() {
//        Debug.Log("BLURRED");
//    }
//
//    private void SelectAll() {
//        m_isSelectAll = true;
//        stringPositionInternal = text.Length;
//        stringSelectPositionInternal = 0;
//    }
//
//    private int GetCaretPositionFromStringIndex(int stringIndex) {
//        int count = m_TextInfo.characterCount;
//
//        for (int i = 0; i < count; i++) {
//            if (m_TextInfo.characterInfo[i].index >= stringIndex) {
//                return i;
//            }
//        }
//
//        return count;
//    }
//
//    private int GetStringIndexFromCaretPosition(int caretPosition) {
//        return m_TextInfo.characterInfo[ClampCaretPosition(m_TextInfo.characterCount, caretPosition)].index;
//    }
//
//    private void UpdateCaretVertices(Mesh mesh) {
//        Rect caretRect = GetCaretRect();
//        s_Vertices[0].position = new Vector3(caretRect.x, caretRect.height, 0.0f);
//        s_Vertices[1].position = new Vector3(caretRect.x, caretRect.y, 0.0f);
//        s_Vertices[2].position = new Vector3(caretRect.x + caretRect.width, caretRect.y, 0.0f);
//        s_Vertices[3].position = new Vector3(caretRect.x + caretRect.width, caretRect.height, 0.0f);
//
//        s_Vertices[0].color = caretColor;
//        s_Vertices[1].color = caretColor;
//        s_Vertices[2].color = caretColor;
//        s_Vertices[3].color = caretColor;
//
//        s_VertexHelper.AddUIVertexQuad(s_Vertices);
//
//        s_VertexHelper.FillMesh(mesh);
//        s_VertexHelper.Clear();
//
//        Vector2 startPosition = new Vector2(caretRect.x, caretRect.y);
//        startPosition.y = Screen.height - startPosition.y;
//        UnityEngine.Input.compositionCursorPos = startPosition;
//    }
//
//    private void UpdateHighlightVertices(Mesh mesh) {
//        caretPositionInternal = m_CaretPosition = GetCaretPositionFromStringIndex(stringPositionInternal);
//        caretSelectPositionInternal = m_CaretSelectPosition = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
//
//        int startChar = Mathf.Max(0, caretPositionInternal);
//        int endChar = Mathf.Max(0, caretSelectPositionInternal);
//
//        if (startChar > endChar) {
//            int temp = startChar;
//            startChar = endChar;
//            endChar = temp;
//        }
//
//        endChar -= 1;
//
//        int currentLineIndex = m_TextInfo.characterInfo[startChar].lineNumber;
//        int nextLineStartIdx = m_TextInfo.lineInfo[currentLineIndex].lastCharacterIndex;
//
//        UIVertex vert = UIVertex.simpleVert;
//        vert.uv0 = Vector2.zero;
//        vert.color = m_SelectionColor;
//
//        int currentChar = startChar;
//        while (currentChar <= endChar && currentChar < m_TextInfo.characterCount) {
//            if (currentChar == nextLineStartIdx || currentChar == endChar) {
//                TMP_CharacterInfo startCharInfo = m_TextInfo.characterInfo[startChar];
//                TMP_CharacterInfo endCharInfo = m_TextInfo.characterInfo[currentChar];
//
//                // Extra check to handle Carriage Return
//                if (currentChar > 0 && endCharInfo.character == 10 && m_TextInfo.characterInfo[currentChar - 1].character == 13) {
//                    endCharInfo = m_TextInfo.characterInfo[currentChar - 1];
//                }
//
//                Vector2 startPosition = new Vector2(startCharInfo.origin, m_TextInfo.lineInfo[currentLineIndex].ascender);
//                Vector2 endPosition;
//                if (m_SeamlessHighlight && currentLineIndex != m_TextInfo.lineCount - 1) {
//                    endPosition = new Vector2(endCharInfo.xAdvance, m_TextInfo.lineInfo[currentLineIndex + 1].ascender);
//                }
//                else {
//                    endPosition = new Vector2(endCharInfo.xAdvance, m_TextInfo.lineInfo[currentLineIndex].descender);
//                }
//
//                int startIndex = s_VertexHelper.currentVertCount;
//                vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f);
//                s_VertexHelper.AddVert(vert);
//
//                vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f);
//                s_VertexHelper.AddVert(vert);
//
//                vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f);
//                s_VertexHelper.AddVert(vert);
//
//                vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f);
//                s_VertexHelper.AddVert(vert);
//
//                s_VertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
//                s_VertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);
//
//                startChar = currentChar + 1;
//                currentLineIndex++;
//
//                if (currentLineIndex < m_TextInfo.lineCount) {
//                    nextLineStartIdx = m_TextInfo.lineInfo[currentLineIndex].lastCharacterIndex;
//                }
//            }
//
//            currentChar++;
//        }
//
//        s_VertexHelper.FillMesh(mesh);
//        s_VertexHelper.Clear();
//    }
//
//    private static int ClampStringPosition(string text, int pos) {
//        if (pos < 0) {
//            return 0;
//        }
//
//        if (pos > text.Length) {
//            return text.Length;
//        }
//
//        return pos;
//    }
//
//    private static int ClampCaretPosition(int characterCount, int pos) {
//        if (characterCount == 0 || pos < 0) {
//            return 0;
//        }
//
//        if (pos > characterCount - 1) {
//            return characterCount - 1;
//        }
//
//        return pos;
//    }
//
//}