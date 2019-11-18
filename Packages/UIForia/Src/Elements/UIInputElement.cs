using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.UIInput;
using UnityEngine;

#pragma warning disable 0649
namespace UIForia.Elements {

    public class CallbackSerializer<T> : IInputSerializer<T> {

        public Func<T, string> serialize;

        public CallbackSerializer(Func<T, string> serialize) {
            this.serialize = serialize;
        }

        public string Serialize(T input) {
            return serialize.Invoke(input);
        }

    }

    public class CallbackDeserializer<T> : IInputDeserializer<T> {

        public Func<string, T> deserialize;

        public CallbackDeserializer(Func<string, T> deserialize) {
            this.deserialize = deserialize;
        }

        public T Deserialize(string input) {
            return deserialize.Invoke(input);
        }

    }

    public static class InputSerializers {

        public static IInputSerializer<int> IntSerializer = new CallbackSerializer<int>((int input) => input.ToString("D"));
        public static IInputSerializer<float> FloatSerializer = new CallbackSerializer<float>((float input) => input.ToString("G"));
        public static IInputSerializer<double> DoubleSerializer = new CallbackSerializer<double>((double input) => input.ToString("G"));
        public static IInputSerializer<string> StringSerializer = new CallbackSerializer<string>((string input) => input);

    }

    public static class InputDeserializers {

        public static IInputDeserializer<int> IntDeserializer = new CallbackDeserializer<int>((string input) => {
            try {
                return int.Parse(input);
            } catch (Exception) {
                return 0;
            }
        });
        public static IInputDeserializer<float> FloatDeserializer = new CallbackDeserializer<float>((string input) => {
            try {
                return float.Parse(input);
            } catch (Exception) {
                return 0f;
            }
        });
        public static IInputDeserializer<double> DoubleDeserializer = new CallbackDeserializer<double>((string input) => {
            try {
                return double.Parse(input);
            } catch (Exception) {
                return 0f;
            }
        });
        public static IInputDeserializer<string> StringDeserializer = new CallbackDeserializer<string>((string input) => input);

    }

    public static class InputFormatters {

        public static IInputFormatter FloatFormatter = new FloatFormatter();
        public static IInputFormatter IntFormatter = new IntFormatter();

    }

    public class IntFormatter : IInputFormatter {

        private static StringBuilder builder = new StringBuilder(32);

        public string Format(string input) {
            builder.Clear();
            bool foundDigit = false;
            bool foundSign = false;

            for (int i = 0; i < input.Length; i++) {
                char c = input[i];

                if (!foundDigit && !foundSign && c == '-') {
                    builder.Append(c);
                    foundSign = true;
                }
                else if (char.IsDigit(c)) {
                    builder.Append(c);
                    foundDigit = true;
                }
            }

            return builder.ToString();
        }
    }

    public class FloatFormatter : IInputFormatter {

        private static StringBuilder builder = new StringBuilder(32);

        private static char k_Decimal = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        public string Format(string input) {
            builder.Clear();
            bool foundDecimal = false;
            bool foundDigit = false;
            bool foundSign = false;

            for (int i = 0; i < input.Length; i++) {
                char c = input[i];

                if (!foundDigit && !foundSign && c == '-') {
                    builder.Append(c);
                    foundSign = true;
                }
                else if (char.IsDigit(c)) {
                    builder.Append(c);
                    foundDigit = true;
                }
                else if (c == k_Decimal && !foundDecimal) {
                    builder.Append(k_Decimal);
                    foundDecimal = true;
                }
            }

            return builder.ToString();
        }
    }

    public interface IInputFormatter {

        string Format(string input);

    }

    public interface IInputSerializer<in T> {

        string Serialize(T input);

    }

    public interface IInputDeserializer<out T> {

        T Deserialize(string input);

    }

    public class FormValidateEvent {

        public void Fail(string key, string message) { }

    }

    public interface IFormElementValidator {

        void OnValidate(FormValidateEvent evt);

    }

    // todo use StructList<char> instead of string to alloc less
    [Template(TemplateType.Internal, "Elements/InputElement.xml")]
    public class InputElement<T> : UIInputElement where T : IEquatable<T> {

        public T value;

//        public Func<string, T> parseValue;
//        public Func<string, string> formatValueAsString;
//        public Func<T, string> formatValue;
        public IInputFormatter formatter;
        public IInputSerializer<T> serializer;
        public IInputDeserializer<T> deserializer;
        
        public int MaxLength = Int32.MaxValue;
        
        [WriteBinding(nameof(value))]
        public event Action<T> onValueChanged;

        public override void OnCreate() {
            base.OnCreate();
            deserializer = deserializer ?? (IInputDeserializer<T>) GetDeserializer();
            serializer = serializer ?? (IInputSerializer<T>) GetSerializer();
            formatter = formatter ?? GetFormatter();
        }

        [OnPropertyChanged(nameof(value))]
        protected void OnInputValueChanged(string name) {
            string oldText = text;
            text = serializer.Serialize(value) ?? string.Empty;

            selectionRange = new SelectionRange(int.MaxValue);
            T v = deserializer.Deserialize(text);

            if (hasFocus) {
                ScrollToCursor();
            }

            onValueChanged?.Invoke(v);

            if (oldText != text) {
                EmitTextChanged();
            }
        }

        protected override void HandleCharactersEntered(string characters) {
            string previous = text;
            text = SelectionRangeUtil.InsertText(text, ref selectionRange, characters);
            HandleTextChanged(previous);
        }

        protected override void HandleCharactersDeletedForwards() {
            string previous = text;
            text = SelectionRangeUtil.DeleteTextForwards(text, ref selectionRange);
            HandleTextChanged(previous);
        }

        protected override void HandleCharactersDeletedBackwards() {
            string previous = text;
            text = SelectionRangeUtil.DeleteTextBackwards(text, ref selectionRange);
            HandleTextChanged(previous);
        }

        private void HandleTextChanged(string previous) {
            string preFormat = text;

            if (formatter != null) { // todo -- handle when to format
                text = formatter.Format(text);
            }

            T newValue = deserializer.Deserialize(text);

            if (text.Length > MaxLength) {
                text = text.Substring(0, MaxLength);
                newValue = deserializer.Deserialize(text);
            }

            if (text != preFormat) {
                int diff = text.Length - preFormat.Length;
                selectionRange = new SelectionRange(selectionRange.cursorIndex + diff);
            }

            if ((value == null && newValue != null) || !value.Equals(newValue)) {
                value = newValue;
                onValueChanged?.Invoke(value);
            }

            if (text != previous) {
                EmitTextChanged();
            }
        }

        public bool ShowPlaceholder => placeholder != null && string.IsNullOrEmpty(text);

        public override string GetDisplayName() {
            return $"InputElement<{typeof(T).Name}>";
        }

        protected object GetDeserializer() {
            if (typeof(T) == typeof(int)) {
                return InputDeserializers.IntDeserializer;
            }

            if (typeof(T) == typeof(float)) {
                return InputDeserializers.FloatDeserializer;
            }

            if (typeof(T) == typeof(double)) {
                return InputDeserializers.DoubleDeserializer;
            }

            if (typeof(T) == typeof(string)) {
                return InputDeserializers.StringDeserializer;
            }

            throw new Exception($"InputElement with generic type {typeof(T)} requires a custom serializer and deserializer in order to function because {typeof(T)} is not a float, int, or string");
        }

        protected object GetSerializer() {
            if (typeof(T) == typeof(int)) {
                return InputSerializers.IntSerializer;
            }

            if (typeof(T) == typeof(float)) {
                return InputSerializers.FloatSerializer;
            }

            if (typeof(T) == typeof(double)) {
                return InputSerializers.DoubleSerializer;
            }

            if (typeof(T) == typeof(string)) {
                return InputSerializers.StringSerializer;
            }

            throw new Exception($"InputElement with generic type {typeof(T)} requires a custom serializer and deserializer in order to function because {typeof(T)} is not a float, int, or string");
        }

        protected IInputFormatter GetFormatter() {
            if (typeof(T) == typeof(float)) {
                return InputFormatters.FloatFormatter;
            }
            if (typeof(T) == typeof(double)) {
                return InputFormatters.FloatFormatter;
            }
            if (typeof(T) == typeof(int)) {
                return InputFormatters.IntFormatter;
            }

            return null;
        }

    }


    public abstract class UIInputElement : BaseInputElement, IFocusableEvented {

        [CustomPainter("UIForia::Input")]
        internal class InputElementPainter : StandardRenderBox  {

            public Path2D path = new Path2D();

            public override void PaintBackground(RenderContext ctx) {
                base.PaintBackground(ctx);
                
                UIInputElement inputElement = (UIInputElement) element;

                path.Clear();
                path.SetTransform(inputElement.layoutResult.matrix.ToMatrix4x4());
                
                float blinkPeriod = 1f / inputElement.caretBlinkRate;

                bool blinkState = (Time.unscaledTime - inputElement.blinkStartTime) % blinkPeriod < blinkPeriod / 2;

                Rect contentRect = inputElement.layoutResult.ContentRect;

                var textInfo = inputElement.textElement.textInfo;

                // float baseLineHeight = textInfo.rootSpan.textStyle.fontAsset.faceInfo.LineHeight;
                // float scaledSize = textInfo.rootSpan.fontSize / textInfo.rootSpan.textStyle.fontAsset.faceInfo.PointSize;
                // float lh = baseLineHeight * scaledSize;

                if (!inputElement.isSelecting && inputElement.hasFocus && blinkState) {
                    path.BeginPath();
                    path.SetStroke(inputElement.style.CaretColor);
                    path.SetStrokeWidth(1f);
                    Vector2 p = textInfo.GetCursorPosition(inputElement.selectionRange.cursorIndex) - inputElement.textScroll;
                    path.MoveTo(inputElement.layoutResult.ContentRect.min + p);
                    path.VerticalLineTo(inputElement.layoutResult.ContentRect.yMax);
                    path.EndPath();
                    path.Stroke();
                }

                if (inputElement.selectionRange.HasSelection) {
                    RangeInt lineRange = new RangeInt(0, 1); //textInfo.GetLineRange(selectionRange));textInfo.GetLineRange(selectionRange);
                    path.BeginPath();
                    path.SetFill(inputElement.style.SelectionBackgroundColor);
    
                    if (lineRange.length > 1) {
                        // todo this doesn't really work yet
                        for (int i = lineRange.start + 1; i < lineRange.end - 1; i++) {
    //                        Rect rect = textInfo.GetLineRect(i);
    //                        rect.x += contentRect.x;
    //                        rect.y += contentRect.y;
    //                        path.Rect(rect);
                        }
                    }
                    else {
                        Rect rect = textInfo.GetLineRect(lineRange.start);
                        Vector2 cursorPosition = textInfo.GetCursorPosition(inputElement.selectionRange.cursorIndex) - inputElement.textScroll;
                        Vector2 selectPosition = textInfo.GetSelectionPosition(inputElement.selectionRange) - inputElement.textScroll;
                        float minX = Mathf.Min(cursorPosition.x, selectPosition.x);
                        float maxX = Mathf.Max(cursorPosition.x, selectPosition.x);
                        minX += contentRect.x;
                        maxX += contentRect.x;
                        rect.y += contentRect.y;
                        float x = Mathf.Max(minX, contentRect.x);
                        float cursorToContentEnd = contentRect.width - x;
                        float cursorToMax = maxX - x;
                        path.Rect(x, rect.y, Mathf.Min(cursorToContentEnd, cursorToMax), rect.height);
                    }
    
                    path.Fill();
                }

                ctx.DrawPath(path);
            }
        }

        private UITextElement textElement;
        // internal TextInfo textInfo;
        internal string text;

        private string m_placeholder;
        public string placeholder {
            get {
                return string.IsNullOrEmpty(m_placeholder) ? "" : m_placeholder;
            }
            set { m_placeholder = value; }
        }
        
        public event Action<FocusEvent> onFocus;
        public event Action<BlurEvent> onBlur;
        public bool autofocus;

        protected float holdDebounce = 0.05f;
        protected float timestamp;

        public float caretBlinkRate = 0.85f;
        protected float blinkStartTime;

        protected SelectionRange selectionRange;

        protected bool hasFocus;
        protected bool selectAllOnFocus;
        protected bool isSelecting;

        protected Vector2 textScroll = new Vector2(0, 0);

        [WriteBinding(nameof(text))]
        public event Action<string> onTextChanged;

        protected float keyLockTimestamp;

        private bool isReady;

        public UIInputElement() {
            selectionRange = new SelectionRange(0);
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public override void OnCreate() {
            text = text ?? string.Empty;
            style.SetPainter("UIForia::Input", StyleState.Normal);
            Application.InputSystem.RegisterFocusable(this);
        }

        public override void OnEnable() {
            textElement = FindById<UITextElement>("input-element-text");
            if (autofocus) {
                Application.InputSystem.RequestFocus(this);
            }

            if (string.IsNullOrEmpty(m_placeholder)) {
                FindById("placeholder-text")?.SetAttribute("empty", "true");
            }
        }

        public override void OnUpdate() {
           if (isReady) {
                ScrollToCursor();
           }

           isReady = true;
        }

        protected void EmitTextChanged() {
            textElement.SetText(text);
            onTextChanged?.Invoke(text);
        }

        protected abstract void HandleCharactersEntered(string characters);

        protected abstract void HandleCharactersDeletedForwards();
        protected abstract void HandleCharactersDeletedBackwards();

        public override void OnDestroy() {
            Blur();
            Application.InputSystem.UnRegisterFocusable(this);
        }

        public override void OnDisable() {
            Blur();
        }

        public override string GetDisplayName() {
            return "InputElement";
        }

        [UsedImplicitly]
        [OnMouseClick]
        protected void OnMouseClick(MouseInputEvent evt) {
            bool hadFocus = hasFocus;

            if (evt.IsConsumed || (!hasFocus && !Input.RequestFocus(this))) {
                return;
            }

            if (!hadFocus && selectAllOnFocus) {
                return;
            }

            blinkStartTime = Time.unscaledTime;
            Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            mouse += textScroll;

            if (evt.IsDoubleClick) {
                selectionRange = textElement.textInfo.SelectWordAtPoint(mouse);
            }
            else if (evt.IsTripleClick) {
               selectionRange = textElement.textInfo.SelectLineAtPoint(mouse);
            }
            else {
                selectionRange = new SelectionRange(textElement.textInfo.GetIndexAtPoint(mouse));
                ScrollToCursor();
            }

            evt.StopPropagation();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus]
        protected void EnterText(KeyboardInputEvent evt) {
            if (evt.ctrl) {
                return;
            }

            evt.StopPropagation();
            if (HasDisabledAttr()) return;

            char c = evt.character;

            if (evt.keyCode == KeyCode.Return) {
                Input.DelayEvent(this, new SubmitEvent());
                return;
            }

            if (evt.keyCode == KeyCode.Tab) {
                // todo: find next IFocusable; implement tab index
                Input.DelayEvent(this, new TabNavigationEvent(evt));
                return;
            }

            if (c == '\n' || c == '\t') return;

            // assume we only ever use 1 text span for now, this should change in the future
            if (!textElement.textInfo.rootSpan.textStyle.fontAsset.HasCharacter(c)) {
                return;
            }

            HandleCharactersEntered(c.ToString());
            ScrollToCursor();
        }

        protected void ScrollToCursor() {
            if (!hasFocus || textElement == null || textElement.textInfo == null) {
                return;
            }

            textElement.textInfo.Layout(Vector2.zero, float.MaxValue);

            Rect rect = VisibleTextRect;
            Vector2 cursor = textElement.textInfo.GetCursorPosition(selectionRange.cursorIndex);
            if (cursor.x - textScroll.x >= rect.width) {
                textScroll.x = (cursor.x - rect.width + rect.x);
            }
            else if (cursor.x - textScroll.x < rect.xMin) {
                textScroll.x = (cursor.x - rect.x);
                if (textScroll.x < 0) textScroll.x = 0;
                
            }
 
            textElement.style.SetTransformPositionX(-textScroll.x, StyleState.Normal);
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.Home)]
        protected void HandleHome(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            selectionRange = textElement.textInfo.MoveToStartOfLine(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.End)]
        protected void HandleEnd(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            selectionRange = textElement.textInfo.MoveToEndOfLine(selectionRange, evt.shift);
            ScrollToCursor();
            blinkStartTime = Time.unscaledTime;
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.Backspace)]
        protected void HandleBackspace(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            keyLockTimestamp = Time.unscaledTime;
            blinkStartTime = Time.unscaledTime;
            HandleCharactersDeletedBackwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldDownWithFocus(KeyCode.Backspace)]
        protected void HandleBackspaceHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (!CanTriggerHeldKey()) return;
            timestamp = Time.unscaledTime;
            blinkStartTime = Time.unscaledTime;
            HandleCharactersDeletedBackwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldDownWithFocus(KeyCode.Delete)]
        protected void HandleDeleteHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (!CanTriggerHeldKey()) return;
            timestamp = Time.unscaledTime;
            blinkStartTime = Time.unscaledTime;
            HandleCharactersDeletedForwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.Delete)]
        protected void HandleDelete(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            keyLockTimestamp = Time.unscaledTime;
            blinkStartTime = Time.unscaledTime;
            if (evt.ctrl || evt.command) {
                selectionRange = new SelectionRange(selectionRange.cursorIndex, text.Length);
            }
            HandleCharactersDeletedForwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldDownWithFocus(KeyCode.LeftArrow)]
        protected void HandleLeftArrowHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (!CanTriggerHeldKey()) return;

            timestamp = Time.unscaledTime;
            selectionRange = textElement.textInfo.MoveCursorLeft(selectionRange, evt.shift, evt.ctrl || evt.command);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.LeftArrow)]
        protected void HandleLeftArrowDown(KeyboardInputEvent evt) {
            evt.StopPropagation();

            if (HasDisabledAttr()) return;

            keyLockTimestamp = Time.unscaledTime;
            selectionRange = textElement.textInfo.MoveCursorLeft(selectionRange, evt.shift, evt.ctrl || evt.command);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldDownWithFocus(KeyCode.RightArrow)]
        protected void HandleRightArrowHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();

            if (!CanTriggerHeldKey()) return;

            blinkStartTime = Time.unscaledTime;
            timestamp = Time.unscaledTime;

            selectionRange = textElement.textInfo.MoveCursorRight(selectionRange, evt.shift, evt.ctrl || evt.command);
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.RightArrow)]
        protected void HandleRightArrow(KeyboardInputEvent evt) {
            evt.StopPropagation();

            if (HasDisabledAttr()) return;
            keyLockTimestamp = Time.unscaledTime;

            blinkStartTime = Time.unscaledTime;

            selectionRange = textElement.textInfo.MoveCursorRight(selectionRange, evt.shift, evt.ctrl || evt.command);
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.C, KeyboardModifiers.Control)]
        protected void HandleCopy(KeyboardInputEvent evt) {
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textElement.textInfo.GetSelectedString(selectionRange);
                evt.StopPropagation();
            }
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.X, KeyboardModifiers.Control)]
        protected void HandleCut(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textElement.textInfo.GetSelectedString(selectionRange);
                HandleCharactersDeletedBackwards();
                evt.StopPropagation();
            }
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.V, KeyboardModifiers.Control)]
        protected void HandlePaste(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl) {
                HandleCharactersEntered(clipboard);
                evt.StopPropagation();
            }
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.A, KeyboardModifiers.Control)]
        protected void HandleSelectAll(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl) {
                selectionRange = new SelectionRange(0, int.MaxValue);
                evt.StopPropagation();
            }
        }

        public bool HasDisabledAttr() {
            return GetAttribute("disabled") != null;
        }

        public bool CanTriggerHeldKey() {
            if (GetAttribute("disabled") != null) return false;

            if (Time.unscaledTime - keyLockTimestamp < 0.5f) {
                return false;
            }

            if (Time.unscaledTime - timestamp < holdDebounce) {
                return false;
            }

            return true;
        }

        [UsedImplicitly]
        [OnDragCreate]
        protected TextSelectDragEvent CreateDragEvent(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;

            if (!hasFocus) {
                Application.InputSystem.RequestFocus(this);
            }
            TextSelectDragEvent retn = new TextSelectDragEvent(this);
            Vector2 mouseDownPosition = evt.LeftMouseDownPosition - layoutResult.screenPosition - layoutResult.ContentRect.position + textScroll;
            Vector2 mousePosition = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position + textScroll;
            
            int indexAtDownPoint = textElement.textInfo.GetIndexAtPoint(mouseDownPosition);
            int indexAtPoint = textElement.textInfo.GetIndexAtPoint(mousePosition);
            if (indexAtDownPoint < indexAtPoint) {
                selectionRange = new SelectionRange(indexAtPoint, indexAtDownPoint);
            }
            else {
                selectionRange = new SelectionRange(indexAtDownPoint, indexAtPoint);
            }
            return retn;
        }

        protected Rect VisibleTextRect {
            get { return layoutResult.ContentRect; }
        }

        public bool Focus() {
            if (GetAttribute("disabled") != null) {
                return false;
            }

            ScrollToCursor();

            hasFocus = true;
            onFocus?.Invoke(new FocusEvent());
            return true;
        }

        public void Blur() {
            hasFocus = false;
            selectionRange = new SelectionRange(selectionRange.cursorIndex);
            onBlur?.Invoke(new BlurEvent());
        }

        protected class TextSelectDragEvent : DragEvent {

            protected readonly UIInputElement _uiInputElement;

            public TextSelectDragEvent(UIInputElement origin) : base(origin) {
                this._uiInputElement = origin;
                _uiInputElement.isSelecting = true;
            }

            public override void Update() {
                Vector2 mouse = MousePosition - _uiInputElement.layoutResult.screenPosition - _uiInputElement.layoutResult.ContentRect.position;
                mouse += _uiInputElement.textScroll;
                _uiInputElement.selectionRange = new SelectionRange(_uiInputElement.textElement.textInfo.GetIndexAtPoint(mouse), _uiInputElement.selectionRange.selectIndex > -1 
                        ? _uiInputElement.selectionRange.selectIndex 
                        : _uiInputElement.selectionRange.cursorIndex);
                _uiInputElement.ScrollToCursor();
            }

            public override void OnComplete() {
                _uiInputElement.isSelecting = false;
                _uiInputElement.selectionRange = new SelectionRange(_uiInputElement.selectionRange.selectIndex, _uiInputElement.selectionRange.cursorIndex);
            }
        }
    }
}