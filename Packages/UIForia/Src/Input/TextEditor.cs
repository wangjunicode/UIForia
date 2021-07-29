using System;
using System.Collections.Generic;
using System.Globalization;
using UIForia.Elements;
using UIForia.Text;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    public struct SelectionInfo : IEquatable<SelectionInfo> {

        public int cursor;
        public int selection;
        public bool showCursor;
        // todo remove those
        public float2 cursorPosition;
        public float cursorHeight;

        public static bool operator ==(SelectionInfo a, SelectionInfo b) {
            return a.Equals(b);
        }

        public static bool operator !=(SelectionInfo a, SelectionInfo b) {
            return !(a == b);
        }

        public bool Equals(SelectionInfo other) {
            return cursor == other.cursor && selection == other.selection && showCursor == other.showCursor && cursorPosition.Equals(other.cursorPosition) && cursorHeight.Equals(other.cursorHeight);
        }

        public override bool Equals(object obj) {
            return obj is SelectionInfo other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = cursor;
                hashCode = (hashCode * 397) ^ selection;
                hashCode = (hashCode * 397) ^ showCursor.GetHashCode();
                hashCode = (hashCode * 397) ^ cursorPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ cursorHeight.GetHashCode();
                return hashCode;
            }
        }
    }

    enum DisplayedCharacterType {
        NewLine, Whitespace, Character
    }

    struct DisplayedCharacter {
        public RangeInt dataBufferRange;
        public DisplayedCharacterType type; // newline, whitespace, character
        public float advance;
        public float2 position;
        public float height;
    }

    public struct TextEditorData {

        public bool isMultiline;
        public bool isPassword;
        public bool clearOnEscape;

        public Func<char, bool> characterFilter;
    }

    /// <summary>
    /// Main Thread only.
    /// </summary>
    internal class TextEditor {
        
        private IMECompositionMode backupIMEMode;

        internal TextEditorData settings;

        private EditableText editableText;

        private bool needsCursorUpdate;
        
        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public string text {
            get => editableText.text;
            set {
                editableText.text = value ?? string.Empty;
                EnsureValidCodePointIndex(ref editableText.cursor.cursor);
                EnsureValidCodePointIndex(ref editableText.cursor.selection);
            }
        }

        public int cursorIndex {
            get => editableText.cursor.cursor;
            set {
                // int prev = editableText.cursor.cursor;
                editableText.cursor.cursor = value;
                EnsureValidCodePointIndex(ref editableText.cursor.cursor);
                // if (editableText.cursor.cursor == prev) {
                //     return;
                // }
                //OnCursorIndexChange();
            }
        }

        public int selectIndex {
            get => editableText.cursor.selection;
            set {
                int prev = editableText.cursor.selection;
                editableText.cursor.selection = value;
                EnsureValidCodePointIndex(ref editableText.cursor.selection);
                if (editableText.cursor.selection == prev) {
                    return;
                }
                //this.OnSelectIndexChange();
            }
        }

        public bool hasSelection => editableText.cursor.selection > -1 && editableText.cursor.cursor != editableText.cursor.selection;

        internal static Dictionary<QueuedKeyboardEvent, TextEditOp> s_Keyactions;
        private bool requiresConversion;

        private LightList<LineMapping> glyphLineMappings;

        private ref TextShapeCache GetShapeCache() {
            unsafe {
                return ref editableText.application.applicationLoop.appInfo->shapeCache;
            }
        }

        private unsafe ref TextDataEntry GetEntry() {
            return ref editableText.textTable->GetEntry(editableText.textId);
        }

        public unsafe void Update() {
            if (editableText == null) return;

            glyphLineMappings = TextUtil.CreateGlyphLineMappings(GetEntry(), ref GetShapeCache());

            if (requiresConversion) {
                requiresConversion = false;
                // for (int i = 0; i < glyphLineMappings.size; i++) {
                //     TextUtil.LineMapping c = glyphLineMappings[i];
                //     Debug.Log(i + ": " + c);
                // }

                // todo process key navigation events
                Queue<QueuedKeyboardEvent> queue = editableText.application.keyboardAdapter.keyEventQueue;
                if (queue.Count > 0) {
                    needsCursorUpdate = true;
                }
                while (queue.Count > 0) {
                    HandleGraphicalNavigation(queue.Dequeue());
                }

            }

            for (var index = 0; index < glyphLineMappings.Count; index++) {
                LineMapping lineMapping = glyphLineMappings[index];
                lineMapping.mappings.Release();
            }

            if (needsCursorUpdate) {
                editableText.RecalcCursorPosition();
                editableText.textTable->SetCursor(editableText.textId, editableText.cursor);
                needsCursorUpdate = false;
            }
        }

        private void HandleGraphicalNavigation(QueuedKeyboardEvent evt) {
            if (s_Keyactions.TryGetValue(evt, out TextEditOp op)) {
                HandleKeyEvent(ref evt, op);
            }
        }

        public static GlyphMapping FindGlyphMappingForCursor(in SelectionInfo cursor, LightList<LineMapping> mapping) {
            return default;
        }

        public unsafe string ProcessTextInputEvent(Queue<QueuedKeyboardEvent> queue) {

            if (editableText == null) {
                return String.Empty;
            }

            bool keepGoing = true;

            InitKeyActions();

            while (queue.Count > 0 && keepGoing) {
                QueuedKeyboardEvent evt = queue.Peek();
                if (evt.eventType != EventType.KeyDown) {
                    // todo figure that out later
                    queue.Dequeue();
                    continue;
                }

                if (!s_Keyactions.TryGetValue(evt, out TextEditOp op)) {
                    HandleKeyDown(ref evt);
                    needsCursorUpdate = true;
                    queue.Dequeue();
                    continue;
                }

                if (IsGraphicalNavigation(op)) {
                    keepGoing = false;
                    requiresConversion = true;
                }
                else {
                    HandleKeyEvent(ref evt, op);
                    queue.Dequeue();
                }

                needsCursorUpdate = true;
            }
            
            editableText.textTable->SetCursor(editableText.textId, editableText.cursor);

            return text;
        }

        internal void HandleKeyEvent(ref QueuedKeyboardEvent e, in TextEditOp op) {
            EventModifiers modifiers = e.modifiers;
            e.modifiers &= ~EventModifiers.CapsLock;
            PerformOperation(op, false);
            e.modifiers = modifiers;
        }

        public void HandleKeyDown(ref QueuedKeyboardEvent current) {
            char character = current.character;

            //if (settings.keyHandler != null && settings.keyHandler.Invoke(current.keyCode)) { 
            // specific character intercept handler...what do we give caller and what do we expect back?
            //}

            if (current.keyCode == KeyCode.Escape) {
                SelectNone();
                if (settings.clearOnEscape) {
                    text = "";
                    // EndEditing();
                }

            }
            else if (character == '\n' || character == '\u0003') {
                if (!settings.isMultiline || (current.alt || current.shift || current.control)) {
                    // todo emit blur event probably
                    // EndEditing();
                }
                else {
                    Insert(character);
                }

            }
            else if (character == '\t' || current.keyCode == KeyCode.Tab) {
                if (settings.isMultiline) {
                    bool charIsValid = settings.characterFilter != null && settings.characterFilter.Invoke(character);
                    if ((!current.alt && !current.shift && !current.control && character == '\t') & charIsValid) {
                        Insert(character);
                    }
                }

            }
            else {
                bool charIsValid = settings.characterFilter == null || settings.characterFilter.Invoke(character);

                if (charIsValid && IsPrintableChar(character)) {
                    Insert(character);
                }
                else if (Input.compositionString != "" && hasSelection) {
                    ReplaceSelection("");
                }
            }
        }

        private bool MightBePrintableKey(Event evt) {

            if (evt.command || evt.control || evt.keyCode >= KeyCode.Mouse0 && evt.keyCode <= KeyCode.Mouse6 || (evt.keyCode >= KeyCode.JoystickButton0 && evt.keyCode <= KeyCode.Joystick8Button19 || evt.keyCode >= KeyCode.F1 && evt.keyCode <= KeyCode.F15)) {
                return false;
            }

            switch (evt.keyCode) {
                case KeyCode.None:
                    return IsPrintableChar(evt.character);

                case KeyCode.Backspace:
                case KeyCode.Clear:
                case KeyCode.Pause:
                case KeyCode.Escape:
                case KeyCode.Delete:
                case KeyCode.UpArrow:
                case KeyCode.DownArrow:
                case KeyCode.RightArrow:
                case KeyCode.LeftArrow:
                case KeyCode.Insert:
                case KeyCode.Home:
                case KeyCode.End:
                case KeyCode.PageUp:
                case KeyCode.PageDown:
                case KeyCode.Numlock:
                case KeyCode.CapsLock:
                case KeyCode.ScrollLock:
                case KeyCode.RightShift:
                case KeyCode.LeftShift:
                case KeyCode.RightControl:
                case KeyCode.LeftControl:
                case KeyCode.RightAlt:
                case KeyCode.LeftAlt:
                case KeyCode.RightCommand:
                case KeyCode.LeftCommand:
                case KeyCode.LeftWindows:
                case KeyCode.RightWindows:
                case KeyCode.AltGr:
                case KeyCode.Help:
                case KeyCode.Print:
                case KeyCode.SysReq:
                case KeyCode.Menu:
                    return false;

                default:
                    return true;
            }
        }

        private bool IsPrintableChar(char character) {
            return character >= ' ';
        }

        internal void InitKeyActions() {

            if (s_Keyactions != null) {
                return;
            }

            static void MapKey(string key, TextEditOp action) {
                s_Keyactions[KeyboardEvent(key)] = action;
            }

            s_Keyactions = new Dictionary<QueuedKeyboardEvent, TextEditOp>();

            MapKey("left", TextEditOp.MoveLeft);
            MapKey("right", TextEditOp.MoveRight);
            MapKey("up", TextEditOp.MoveUp);
            MapKey("down", TextEditOp.MoveDown);
            MapKey("#left", TextEditOp.SelectLeft);
            MapKey("#right", TextEditOp.SelectRight);
            MapKey("#up", TextEditOp.SelectUp);
            MapKey("#down", TextEditOp.SelectDown);
            MapKey("delete", TextEditOp.Delete);
            MapKey("backspace", TextEditOp.Backspace);
            MapKey("#backspace", TextEditOp.Backspace);

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX) {
                MapKey("^left", TextEditOp.MoveGraphicalLineStart);
                MapKey("^right", TextEditOp.MoveGraphicalLineEnd);
                MapKey("&left", TextEditOp.MoveWordLeft);
                MapKey("&right", TextEditOp.MoveWordRight);
                MapKey("%left", TextEditOp.MoveGraphicalLineStart);
                MapKey("%right", TextEditOp.MoveGraphicalLineEnd);
                MapKey("%up", TextEditOp.MoveTextStart);
                MapKey("%down", TextEditOp.MoveTextEnd);
                MapKey("#home", TextEditOp.SelectTextStart);
                MapKey("#end", TextEditOp.SelectTextEnd);
                MapKey("#^left", TextEditOp.ExpandSelectGraphicalLineStart);
                MapKey("#^right", TextEditOp.ExpandSelectGraphicalLineEnd);
                MapKey("#&left", TextEditOp.SelectWordLeft);
                MapKey("#&right", TextEditOp.SelectWordRight);
                MapKey("#%left", TextEditOp.ExpandSelectGraphicalLineStart);
                MapKey("#%right", TextEditOp.ExpandSelectGraphicalLineEnd);
                MapKey("#%up", TextEditOp.SelectTextStart);
                MapKey("#%down", TextEditOp.SelectTextEnd);
                MapKey("%a", TextEditOp.SelectAll);
                MapKey("%x", TextEditOp.Cut);
                MapKey("%c", TextEditOp.Copy);
                MapKey("%v", TextEditOp.Paste);
                MapKey("^d", TextEditOp.Delete);
                MapKey("^h", TextEditOp.Backspace);
                MapKey("^b", TextEditOp.MoveLeft);
                MapKey("^f", TextEditOp.MoveRight);
                MapKey("^a", TextEditOp.MoveLineStart);
                MapKey("^e", TextEditOp.MoveLineEnd);
                MapKey("&delete", TextEditOp.DeleteWordForward);
                MapKey("&backspace", TextEditOp.DeleteWordBack);
            }
            else {
                MapKey("home", TextEditOp.MoveGraphicalLineStart);
                MapKey("end", TextEditOp.MoveGraphicalLineEnd);
                MapKey("%left", TextEditOp.MoveWordLeft);
                MapKey("%right", TextEditOp.MoveWordRight);
                MapKey("^left", TextEditOp.MoveToEndOfPreviousWord);
                MapKey("^right", TextEditOp.MoveToStartOfNextWord);
                MapKey("#^left", TextEditOp.SelectToEndOfPreviousWord);
                MapKey("#^right", TextEditOp.SelectToStartOfNextWord);
                MapKey("#home", TextEditOp.SelectGraphicalLineStart);
                MapKey("#end", TextEditOp.SelectGraphicalLineEnd);
                MapKey("^delete", TextEditOp.DeleteWordForward);
                MapKey("^backspace", TextEditOp.DeleteWordBack);
                MapKey("^a", TextEditOp.SelectAll);
                MapKey("^x", TextEditOp.Cut);
                MapKey("^c", TextEditOp.Copy);
                MapKey("^v", TextEditOp.Paste);
                MapKey("#delete", TextEditOp.Cut);
                MapKey("^insert", TextEditOp.Copy);
                MapKey("#insert", TextEditOp.Paste);
            }
        }

        public static QueuedKeyboardEvent KeyboardEvent(string key) {

            QueuedKeyboardEvent evt = new QueuedKeyboardEvent() {
                eventType = EventType.KeyDown,
                commandName = ""
            };

            if (string.IsNullOrEmpty(key)) {
                return evt;
            }

            int num = 0;
            bool loop;
            do {
                loop = true;
                if (num >= key.Length) {
                    break;
                }

                switch (key[num]) {
                    case '#':
                        evt.modifiers |= EventModifiers.Shift;
                        ++num;
                        break;

                    case '%':
                        evt.modifiers |= EventModifiers.Command;
                        ++num;
                        break;

                    case '&':
                        evt.modifiers |= EventModifiers.Alt;
                        ++num;
                        break;

                    case '^':
                        evt.modifiers |= EventModifiers.Control;
                        ++num;
                        break;

                    default:
                        loop = false;
                        break;
                }
            } while (loop);

            string lowerInvariant = key.Substring(num, key.Length - num).ToLowerInvariant();
            switch (lowerInvariant) {
                case "[+]":
                    evt.character = '+';
                    evt.keyCode = KeyCode.KeypadPlus;
                    break;

                case "[-]":
                    evt.character = '-';
                    evt.keyCode = KeyCode.KeypadMinus;
                    break;

                case "[.]":
                    evt.character = '.';
                    evt.keyCode = KeyCode.KeypadPeriod;
                    break;

                case "[/]":
                    evt.character = '/';
                    evt.keyCode = KeyCode.KeypadDivide;
                    break;

                case "[0]":
                    evt.character = '0';
                    evt.keyCode = KeyCode.Keypad0;
                    break;

                case "[1]":
                    evt.character = '1';
                    evt.keyCode = KeyCode.Keypad1;
                    break;

                case "[2]":
                    evt.character = '2';
                    evt.keyCode = KeyCode.Keypad2;
                    break;

                case "[3]":
                    evt.character = '3';
                    evt.keyCode = KeyCode.Keypad3;
                    break;

                case "[4]":
                    evt.character = '4';
                    evt.keyCode = KeyCode.Keypad4;
                    break;

                case "[5]":
                    evt.character = '5';
                    evt.keyCode = KeyCode.Keypad5;
                    break;

                case "[6]":
                    evt.character = '6';
                    evt.keyCode = KeyCode.Keypad6;
                    break;

                case "[7]":
                    evt.character = '7';
                    evt.keyCode = KeyCode.Keypad7;
                    break;

                case "[8]":
                    evt.character = '8';
                    evt.keyCode = KeyCode.Keypad8;
                    break;

                case "[9]":
                    evt.character = '9';
                    evt.keyCode = KeyCode.Keypad9;
                    break;

                case "[=]":
                    evt.character = '=';
                    evt.keyCode = KeyCode.KeypadEquals;
                    break;

                case "[enter]":
                    evt.character = '\n';
                    evt.keyCode = KeyCode.KeypadEnter;
                    break;

                case "[equals]":
                    evt.character = '=';
                    evt.keyCode = KeyCode.KeypadEquals;
                    break;

                case "[esc]":
                    evt.keyCode = KeyCode.Escape;
                    break;

                case "backspace":
                    evt.keyCode = KeyCode.Backspace;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "delete":
                    evt.keyCode = KeyCode.Delete;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "down":
                    evt.keyCode = KeyCode.DownArrow;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "end":
                    evt.keyCode = KeyCode.End;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f1":
                    evt.keyCode = KeyCode.F1;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f10":
                    evt.keyCode = KeyCode.F10;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f11":
                    evt.keyCode = KeyCode.F11;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f12":
                    evt.keyCode = KeyCode.F12;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f13":
                    evt.keyCode = KeyCode.F13;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f14":
                    evt.keyCode = KeyCode.F14;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f15":
                    evt.keyCode = KeyCode.F15;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f2":
                    evt.keyCode = KeyCode.F2;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f3":
                    evt.keyCode = KeyCode.F3;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f4":
                    evt.keyCode = KeyCode.F4;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f5":
                    evt.keyCode = KeyCode.F5;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f6":
                    evt.keyCode = KeyCode.F6;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f7":
                    evt.keyCode = KeyCode.F7;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f8":
                    evt.keyCode = KeyCode.F8;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "f9":
                    evt.keyCode = KeyCode.F9;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "home":
                    evt.keyCode = KeyCode.Home;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "insert":
                    evt.keyCode = KeyCode.Insert;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "left":
                    evt.keyCode = KeyCode.LeftArrow;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "page down":
                    evt.keyCode = KeyCode.PageDown;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "page up":
                    evt.keyCode = KeyCode.PageUp;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "pgdown":
                    evt.keyCode = KeyCode.PageUp;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "pgup":
                    evt.keyCode = KeyCode.PageDown;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "return":
                    evt.character = '\n';
                    evt.keyCode = KeyCode.Return;
                    evt.modifiers &= ~EventModifiers.FunctionKey;
                    break;

                case "right":
                    evt.keyCode = KeyCode.RightArrow;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                case "space":
                    evt.keyCode = KeyCode.Space;
                    evt.character = ' ';
                    evt.modifiers &= ~EventModifiers.FunctionKey;
                    break;

                case "tab":
                    evt.keyCode = KeyCode.Tab;
                    break;

                case "up":
                    evt.keyCode = KeyCode.UpArrow;
                    evt.modifiers |= EventModifiers.FunctionKey;
                    break;

                default:
                    if (lowerInvariant.Length != 1) {
                        try {
                            evt.keyCode = (KeyCode) Enum.Parse(typeof(KeyCode), lowerInvariant, true);
                            break;
                        }
                        catch (ArgumentException ex) {
                            break;
                        }
                    }
                    else {
                        evt.character = lowerInvariant.ToLower()[0];
                        evt.keyCode = (KeyCode) evt.character;
                        if ((uint) evt.modifiers > 0U)
                            evt.character = char.MinValue;
                        break;
                    }
            }

            return evt;
        }

        public void OnTextChanged(TextInputEvent evt) { }

        public void OnMouseDown() { }

        public void OnMouseUp() { }

        public void OnMouseDrag() { }

        public float2 GetCusorAtIndex() {
            return 0;
        }

        public int GetIndexAtPoint(float2 point) {
            return 0;
        }

        internal bool IsGraphicalNavigation(TextEditOp op) {
            switch (op) {
                case TextEditOp.MoveUp:
                case TextEditOp.MoveDown:
                case TextEditOp.MovePageDown:
                case TextEditOp.MovePageUp:
                case TextEditOp.SelectPageDown:
                case TextEditOp.SelectPageUp:
                case TextEditOp.SelectDown:
                case TextEditOp.SelectUp:
                case TextEditOp.MoveGraphicalLineStart:
                case TextEditOp.MoveGraphicalLineEnd:
                case TextEditOp.SelectGraphicalLineEnd:
                case TextEditOp.SelectGraphicalLineStart:
                case TextEditOp.ExpandSelectGraphicalLineEnd:
                case TextEditOp.ExpandSelectGraphicalLineStart:
                    return true;
                default:
                    return false;
            }
        }

        internal void PerformOperation(TextEditOp operation, bool textIsReadOnly) {

            switch (operation) {
                case TextEditOp.MoveLeft:
                    MoveLeft();
                    break;

                case TextEditOp.MoveRight:
                    MoveRight();
                    break;

                case TextEditOp.MoveUp:
                    MoveUp();
                    break;

                case TextEditOp.MoveDown:
                    MoveDown();
                    break;

                case TextEditOp.MoveLineStart:
                    MoveLineStart();
                    break;

                case TextEditOp.MoveLineEnd:
                    MoveLineEnd();
                    break;

                case TextEditOp.MoveTextStart:
                    MoveTextStart();
                    break;

                case TextEditOp.MoveTextEnd:
                    MoveTextEnd();
                    break;

                case TextEditOp.MoveGraphicalLineStart:
                    MoveGraphicalLineStart();
                    break;

                case TextEditOp.MoveGraphicalLineEnd:
                    MoveGraphicalLineEnd();
                    break;

                case TextEditOp.MoveWordLeft:
                    MoveWordLeft();
                    break;

                case TextEditOp.MoveWordRight:
                    MoveWordRight();
                    break;

                case TextEditOp.MoveToStartOfNextWord:
                    MoveToStartOfNextWord();
                    break;

                case TextEditOp.MoveToEndOfPreviousWord:
                    MoveToEndOfPreviousWord();
                    break;

                case TextEditOp.SelectLeft:
                    SelectLeft();
                    break;

                case TextEditOp.SelectRight:
                    SelectRight();
                    break;

                case TextEditOp.SelectUp:
                    SelectUp();
                    break;

                case TextEditOp.SelectDown:
                    SelectDown();
                    break;

                case TextEditOp.SelectTextStart:
                    SelectTextStart();
                    break;

                case TextEditOp.SelectTextEnd:
                    SelectTextEnd();
                    break;

                case TextEditOp.ExpandSelectGraphicalLineStart:
                    ExpandSelectGraphicalLineStart();
                    break;

                case TextEditOp.ExpandSelectGraphicalLineEnd:
                    ExpandSelectGraphicalLineEnd();
                    break;

                case TextEditOp.SelectGraphicalLineStart:
                    SelectGraphicalLineStart();
                    break;

                case TextEditOp.SelectGraphicalLineEnd:
                    SelectGraphicalLineEnd();
                    break;

                case TextEditOp.SelectWordLeft:
                    SelectWordLeft();
                    break;

                case TextEditOp.SelectWordRight:
                    SelectWordRight();
                    break;

                case TextEditOp.SelectToEndOfPreviousWord:
                    SelectToEndOfPreviousWord();
                    break;

                case TextEditOp.SelectToStartOfNextWord:
                    SelectToStartOfNextWord();
                    break;
                case TextEditOp.Delete:
                    Delete();
                    break;

                case TextEditOp.Backspace:
                    Backspace();
                    break;

                case TextEditOp.DeleteWordBack:
                    DeleteWordBack();
                    break;

                case TextEditOp.DeleteWordForward:
                    DeleteWordForward();
                    break;

                case TextEditOp.Cut:
                    Cut();
                    break;

                case TextEditOp.Copy:
                    Copy();
                    break;

                case TextEditOp.Paste: 
                    Paste();
                    break;

                case TextEditOp.SelectAll:
                    SelectAll();
                    break;

                case TextEditOp.SelectNone:
                    SelectNone();
                    break;

                default:
                    Debug.Log((object) ("Unimplemented: " + operation));
                    break;
            }
        }

        private void SelectNone() {
            editableText.cursor.selection = -1;
        }

        internal void SelectAll() {
            editableText.cursor.selection = 0;
            editableText.cursor.cursor = text.Length;
        }

        private void Paste() {
            ReplaceSelection(clipboard);
        }

        private void Copy() {
            if (cursorIndex < 0 || selectIndex < 0) {
                return;
            }

            int min = Mathf.Min(cursorIndex, selectIndex);
            int max = Mathf.Max(cursorIndex, selectIndex);

            clipboard = editableText.text.Substring(min, max - min);
        }

        private void Cut() {
            if (cursorIndex < 0 || selectIndex < 0) {
                return;
            }

            int min = Mathf.Min(cursorIndex, selectIndex);
            int max = Mathf.Max(cursorIndex, selectIndex);

            clipboard = editableText.text.Substring(min, max - min);
            DeleteSelection();
        }

        private void DeleteWordForward() {
            if (selectIndex == -1) {
                EnsureSelectionIndex();
                MoveWordRight();
            }
            Delete();
        }

        private void DeleteWordBack() {
            // only delete the word if there is no selection, otherwise just delete the selection
            if (selectIndex == -1) {
                EnsureSelectionIndex();
                MoveWordLeft();
            }
            Delete();
        }

        private void Backspace() {
            if (hasSelection) {
                DeleteSelection();
                return;
            }

            if (cursorIndex <= 0) {
                return;
            }

            int startIndex = PreviousCodePointIndex(cursorIndex);
            editableText.text = text.Remove(startIndex, cursorIndex - startIndex);
            cursorIndex = startIndex;
            ClearCursorPos();
        }

        private void Delete() {
            if (hasSelection) {
                DeleteSelection();
                return;
            }

            if (cursorIndex > text.Length) {
                cursorIndex = text.Length;
                return;
            }

            editableText.text = text.Remove(cursorIndex, NextCodePointIndex(cursorIndex) - cursorIndex);
        }

        private void SelectToStartOfNextWord() {
            EnsureSelectionIndex();
            MoveWordRight();
        }

        private void SelectToEndOfPreviousWord() {
            EnsureSelectionIndex();
            MoveWordLeft();
        }

        private void SelectWordRight() {
            EnsureSelectionIndex();
            MoveRight();
        }

        private void SelectWordLeft() {
            EnsureSelectionIndex();
            MoveLeft();
        }

        private void SelectGraphicalLineEnd() {
            ExpandSelectGraphicalLineEnd();
        }

        private void SelectGraphicalLineStart() {
            ExpandSelectGraphicalLineStart();
        }

        private void ExpandSelectGraphicalLineEnd() {
            EnsureSelectionIndex();
            MoveLineEnd();
        }

        private void ExpandSelectGraphicalLineStart() {
            EnsureSelectionIndex();
            MoveLineStart();
        }

        private void SelectTextEnd() {
            EnsureSelectionIndex();
            MoveTextEnd();
        }

        private void SelectTextStart() {
            EnsureSelectionIndex();
            MoveTextStart();
        }

        private void SelectDown() {
            EnsureSelectionIndex();
            MoveDown();
        }

        private void SelectUp() {
            if (editableText.cursor.selection == -1) {
                editableText.cursor.selection = editableText.cursor.cursor;
            }
            MoveUp();
        }

        private void SelectRight() {
            if (editableText.cursor.selection == -1) {
                editableText.cursor.selection = editableText.cursor.cursor;
            }
            
            editableText.cursor.cursor = NextCodePointIndex(cursorIndex);
        }

        private void SelectLeft() {
            if (editableText.cursor.selection == -1) {
                editableText.cursor.selection = editableText.cursor.cursor;
            }
            
            editableText.cursor.cursor = PreviousCodePointIndex(cursorIndex);
        }

        private void MoveToEndOfPreviousWord() {
            MoveWordLeft();
        }

        private void MoveToStartOfNextWord() {
            MoveWordRight();
        }

        private void MoveWordRight() {
            TextUtil.NextWord(glyphLineMappings, GetEntry(), ref editableText.cursor);
        }

        private void MoveWordLeft() {
            TextUtil.PreviousWord(glyphLineMappings, GetEntry(), ref editableText.cursor);
        }

        private void MoveGraphicalLineEnd() {
            TextUtil.EndOfLine(glyphLineMappings, ref editableText.cursor);
        }

        private void MoveGraphicalLineStart() {
            SelectionData selectionData = GetCursorPositionAt(new float2(0, -editableText.cursor.cursorPosition.y));
            editableText.cursor.selection = -1;
            editableText.cursor.cursor = selectionData.index;
            editableText.cursor.cursorPosition = selectionData.position;
            editableText.cursor.cursorHeight = selectionData.height;
        }

        private void MoveTextEnd() {
            editableText.cursor.cursor = text.Length;
            editableText.cursor.selection = -1;
        }

        private void MoveTextStart() {
            editableText.cursor.cursor = 0;
            editableText.cursor.selection = -1;
        }

        private void MoveLineStart() {
            MoveGraphicalLineStart();
        }

        private void MoveLineEnd() {
            MoveGraphicalLineEnd();
        }

        private void MoveDown() {
            TextUtil.NextLine(glyphLineMappings, ref editableText.cursor, GetEntry(), GetShapeCache());
        }

        private void MoveUp() {
            TextUtil.PreviousLine(glyphLineMappings, ref editableText.cursor, GetEntry(), GetShapeCache());
        }

        private void MoveRight() {
            editableText.cursor.cursor = NextCodePointIndex(cursorIndex);
            editableText.cursor.selection = -1;
        }

        private void MoveLeft() {
            editableText.cursor.cursor = PreviousCodePointIndex(cursorIndex);
            editableText.cursor.selection = -1;
        }

        public void Insert(char character) {
            ReplaceSelection(character.ToString()); // todo -- make character based version
        }

        public void ReplaceSelection(string replace) {
            DeleteSelection();
            text = text.Insert(cursorIndex, replace);
            cursorIndex += replace.Length;
            
            // (cursorIndex += replace.Length);
            ClearCursorPos();
        }

        private void ClearCursorPos() {
            editableText.cursor.selection = -1;
        }

        private void DeleteSelection() {
            if (cursorIndex == selectIndex || selectIndex < 0) {
                return;
            }

            if (cursorIndex < selectIndex) {
                text = text.Substring(0, cursorIndex) + text.Substring(selectIndex, text.Length - selectIndex);
            }
            else {
                text = text.Substring(0, selectIndex) + text.Substring(cursorIndex, text.Length - cursorIndex);
                cursorIndex = selectIndex;
            }

            ClearCursorPos();
        }

        // public void BeginEditing() {
        //     backupIMEMode = Input.imeCompositionMode;
        //     Input.imeCompositionMode = IMECompositionMode.On;
        // }
        //
        // public void EndEditing() {
        //     isActuallyEditing = false;
        //     Input.imeCompositionMode = backupIMEMode;
        // }

        private void ClampTextIndex(ref int index) => index = Mathf.Clamp(index, 0, text.Length);

        private void EnsureValidCodePointIndex(ref int index) {
            ClampTextIndex(ref index);
            if (IsValidCodePointIndex(index)) {
                return;
            }

            index = NextCodePointIndex(index);
        }

        private bool IsValidCodePointIndex(int index) {
            if (index < 0 || index > text.Length) {
                return false;
            }

            if (index == 0 || index == text.Length) {
                return true;
            }

            return char.GetUnicodeCategory(text[index]) != UnicodeCategory.NonSpacingMark && !char.IsLowSurrogate(text[index]);
        }

        private int PreviousCodePointIndex(int index) {
            if (index > 0) {
                --index;
            }

            while (index > 0 && !IsValidCodePointIndex(index)) {
                --index;
            }

            return index;
        }

        private int NextCodePointIndex(int index) {
            if (index < text.Length) {
                ++index;
            }

            while (index < text.Length && !IsValidCodePointIndex(index)) {
                ++index;
            }

            return index;
        }

        public void SetTextEditorData(TextEditorData textSettings, EditableText editableText) {
            this.settings = textSettings;
            this.editableText = editableText;
            this.requiresConversion = true;
        }

        private SelectionData GetCursorPositionAt(in float2 position) {
            return TextUtil.GetCursorPosition(glyphLineMappings, new float2(position.x, position.y), GetEntry(), GetShapeCache());
        }

        /// <summary>
        /// sets the selection index to the current cursor index if there's no selection yet
        /// </summary>
        private void EnsureSelectionIndex() {
            if (editableText.cursor.selection == -1) {
                editableText.cursor.selection = editableText.cursor.cursor;
            }
        }

    }

}