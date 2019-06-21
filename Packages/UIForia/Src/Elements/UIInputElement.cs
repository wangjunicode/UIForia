using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SVGX;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.UIInput;
using UnityEngine;
using Vertigo;
using TextInfo = UIForia.Text.TextInfo;

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

    public static class FormatStrings {

        public const string DoubleFixedPoint = "0.###################################################################################################################################################################################################################################################################################################################################################";

    }

    public static class InputSerializers {

        public static IInputSerializer<int> IntSerializer = new CallbackSerializer<int>((int input) => input.ToString());
        public static IInputSerializer<float> FloatSerializer = new CallbackSerializer<float>((float input) => input.ToString(FormatStrings.DoubleFixedPoint));
        public static IInputSerializer<string> StringSerializer = new CallbackSerializer<string>((string input) => input);

    }

    public static class InputDeserializers {

        public static IInputDeserializer<int> IntDeserializer = new CallbackDeserializer<int>((string input) => int.Parse(input));
        public static IInputDeserializer<float> FloatDeserializer = new CallbackDeserializer<float>((string input) => float.Parse(input));
        public static IInputDeserializer<string> StringDeserializer = new CallbackDeserializer<string>((string input) => input);

    }

    public static class InputFormatters {

        public static IInputFormatter FloatFormatter = new FloatFormatter();

    }

    public class FloatFormatter : IInputFormatter {

        private static StringBuilder builder = new StringBuilder(32);

        private static char k_Decimal = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        public string Format(string input) {
            builder.Clear();
            bool foundDecimal = false;

            for (int i = 0; i < input.Length; i++) {
                char c = input[i];
                if (char.IsDigit(c)) {
                    builder.Append(c);
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

        [WriteBinding(nameof(value))]
        public event Action<T> onValueChanged;

        public override void OnCreate() {
            deserializer = deserializer ?? (IInputDeserializer<T>) GetDeserializer();
            serializer = serializer ?? (IInputSerializer<T>) GetSerializer();
            formatter = formatter ?? GetFormatter();

            text = text ?? string.Empty;
            style.SetPainter("self", StyleState.Normal);
            textInfo = new TextInfo(new TextSpan(text, style.GetTextStyle()));
            textInfo.UpdateSpan(0, text);
            textInfo.Layout();
        }


        [OnPropertyChanged(nameof(value))]
        protected void OnInputValueChanged(string name) {
            string oldText = text;
            text = serializer.Serialize(value) ?? string.Empty;

            textInfo.UpdateSpan(0, text);
            textInfo.Layout();

            selectionRange = textInfo.MoveToEndOfText();
            T v = deserializer.Deserialize(text);

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

            if (text != preFormat) {
                int diff = text.Length - preFormat.Length;
                selectionRange = new SelectionRange(selectionRange.cursorIndex - diff, TextEdge.Left);
            }

            textInfo.UpdateSpan(0, text);
            textInfo.Layout();

            T newValue = deserializer.Deserialize(text);
            if ((value == null && newValue != null) || !value.Equals(newValue)) {
                value = newValue;
                onValueChanged?.Invoke(value);
            }

            if (text != previous) {
                EmitTextChanged();
            }
        }

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

            if (typeof(T) == typeof(string)) {
                return InputSerializers.StringSerializer;
            }

            throw new Exception($"InputElement with generic type {typeof(T)} requires a custom serializer and deserializer in order to function because {typeof(T)} is not a float, int, or string");
        }

        protected IInputFormatter GetFormatter() {
            if (typeof(T) == typeof(float)) {
                return InputFormatters.FloatFormatter;
            }

            return null;
        }

    }


    public abstract class UIInputElement : UIElement, IFocusable, ISVGXPaintable, IStylePropertiesDidChangeHandler {

        internal TextInfo textInfo;
        internal string text;
        internal string placeholder;

        protected float holdDebounce = 0.05f;
        protected float timestamp;

        public Color caretColor = Color.black;
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

        public UIInputElement() {
            flags |= UIElementFlags.BuiltIn;
            selectionRange = new SelectionRange(0, TextEdge.Left);
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public override void OnCreate() {
            text = text ?? string.Empty;
            style.SetPainter("self", StyleState.Normal);
            textInfo = new TextInfo(new TextSpan(text, style.GetTextStyle()));
            textInfo.UpdateSpan(0, text);
            textInfo.Layout();
        }

        protected void EmitTextChanged() {
            onTextChanged?.Invoke(text);
        }

        protected abstract void HandleCharactersEntered(string characters);

        protected abstract void HandleCharactersDeletedForwards();
        protected abstract void HandleCharactersDeletedBackwards();

        public override void OnDestroy() {
            Blur();
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
                selectionRange = textInfo.SelectWordAtPoint(mouse);
            }
            else if (evt.IsTripleClick) {
                selectionRange = textInfo.SelectLineAtPoint(mouse);
            }
            else {
                selectionRange = textInfo.GetSelectionAtPoint(mouse);
                ScrollToCursor();
            }

            evt.StopPropagation();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus]
        protected void EnterText(KeyboardInputEvent evt) {
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

            if (!textInfo.spanList[0].textStyle.fontAsset.HasCharacter(c)) {
                return;
            }

//            selectionRange = textInfo.InsertText(selectionRange, c);
            HandleCharactersEntered(c.ToString());
            ScrollToCursor();
        }

        protected void ScrollToCursor() {
            Rect rect = VisibleTextRect;
            Vector2 cursor = textInfo.GetCursorPosition(selectionRange);
            if (cursor.x - textScroll.x >= rect.width) {
                textScroll.x = (cursor.x - rect.width + rect.x);
            }
            else if (cursor.x - textScroll.x < rect.xMin) {
                textScroll.x = (cursor.x - rect.x);
                if (textScroll.x < 0) textScroll.x = 0;
            }
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.Home)]
        protected void HandleHome(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            selectionRange = textInfo.MoveToStartOfLine(selectionRange);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.End)]
        protected void HandleEnd(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            selectionRange = textInfo.MoveToEndOfLine(selectionRange);
            ScrollToCursor();
            blinkStartTime = Time.unscaledTime;
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.Backspace)]
        protected void HandleBackspace(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;
            keyLockTimestamp = Time.unscaledTime;
//            selectionRange = textInfo.DeleteTextBackwards(selectionRange);
            blinkStartTime = Time.unscaledTime;
            HandleCharactersDeletedBackwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldWithFocus(KeyCode.Backspace)]
        protected void HandleBackspaceHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (!CanTriggerHeldKey()) return;
            timestamp = Time.unscaledTime;
            //selectionRange = textInfo.DeleteTextBackwards(selectionRange);
            blinkStartTime = Time.unscaledTime;
            HandleCharactersDeletedBackwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldWithFocus(KeyCode.Delete)]
        protected void HandleDeleteHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (!CanTriggerHeldKey()) return;
            timestamp = Time.unscaledTime;

            //selectionRange = textInfo.DeleteTextForwards(selectionRange);
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
//            selectionRange = textInfo.DeleteTextForwards(selectionRange);
            blinkStartTime = Time.unscaledTime;
            HandleCharactersDeletedForwards();
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldWithFocus(KeyCode.LeftArrow)]
        protected void HandleLeftArrowHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (!CanTriggerHeldKey()) return;

            timestamp = Time.unscaledTime;
            selectionRange = textInfo.MoveCursorLeft(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.LeftArrow)]
        protected void HandleLeftArrowDown(KeyboardInputEvent evt) {
            evt.StopPropagation();

            if (HasDisabledAttr()) return;

            keyLockTimestamp = Time.unscaledTime;
            selectionRange = textInfo.MoveCursorLeft(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyHeldWithFocus(KeyCode.RightArrow)]
        protected void HandleRightArrowHeld(KeyboardInputEvent evt) {
            evt.StopPropagation();

            if (!CanTriggerHeldKey()) return;

            blinkStartTime = Time.unscaledTime;
            timestamp = Time.unscaledTime;

            selectionRange = textInfo.MoveCursorRight(selectionRange, evt.shift);
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.RightArrow)]
        protected void HandleRightArrow(KeyboardInputEvent evt) {
            evt.StopPropagation();

            if (HasDisabledAttr()) return;
            keyLockTimestamp = Time.unscaledTime;

            blinkStartTime = Time.unscaledTime;

            selectionRange = textInfo.MoveCursorRight(selectionRange, evt.shift);
            ScrollToCursor();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.C, KeyboardModifiers.Control)]
        protected void HandleCopy(KeyboardInputEvent evt) {
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textInfo.GetSelectedString(selectionRange);
                evt.StopPropagation();
            }
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus(KeyCode.X, KeyboardModifiers.Control)]
        protected void HandleCut(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textInfo.GetSelectedString(selectionRange);
                //selectionRange = textInfo.DeleteTextBackwards(selectionRange);
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
                selectionRange = textInfo.SelectAll();
                evt.StopPropagation();
            }
        }

        protected bool HasDisabledAttr() {
            return GetAttribute("disabled") != null;
        }

        protected bool CanTriggerHeldKey() {
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
            TextSelectDragEvent retn = new TextSelectDragEvent(this);
            Vector2 mouse = evt.MouseDownPosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            selectionRange = textInfo.GetSelectionAtPoint(mouse);
            return retn;
        }

        public void Paint(ImmediateRenderContext ctx, in SVGXMatrix matrix) {
            ctx.SetTransform(matrix);
            SVGXRenderSystem.PaintElement(ctx, this);

            float blinkPeriod = 1f / caretBlinkRate;

            bool blinkState = (Time.unscaledTime - blinkStartTime) % blinkPeriod < blinkPeriod / 2;

            Rect contentRect = layoutResult.ContentRect;

//            ctx.EnableScissorRect(new Rect(contentRect) {
//                x = contentRect.x + layoutResult.screenPosition.x,
//                y = contentRect.y + layoutResult.screenPosition.y
//            });

            ctx.DisableScissorRect();
            if (isSelecting) {
                ctx.BeginPath();
                ctx.SetStroke(caretColor);
                ctx.SetStrokeWidth(1f);
                Vector2 p = textInfo.GetSelectionPosition(selectionRange) - textScroll;
                ctx.MoveTo(layoutResult.ContentRect.min + p + new Vector2(0, -4f)); // todo remove + 4 on y
                ctx.VerticalLineTo(layoutResult.ContentRect.y + p.y + style.GetResolvedFontSize());
                ctx.Stroke();
            }

            if (!isSelecting && hasFocus && blinkState) {
                ctx.BeginPath();
                ctx.SetStroke(caretColor);
                ctx.SetStrokeWidth(1f);
                Vector2 p = textInfo.GetCursorPosition(selectionRange) - textScroll;
                ctx.MoveTo(layoutResult.ContentRect.min + p + new Vector2(0, -4f)); // todo remove + 4 on y
                ctx.VerticalLineTo(layoutResult.ContentRect.y + p.y + style.GetResolvedFontSize());
                ctx.Stroke();
            }

            if (selectionRange.HasSelection) {
                RangeInt lineRange = new RangeInt(0, 1); //textInfo.GetLineRange(selectionRange));textInfo.GetLineRange(selectionRange);
                ctx.BeginPath();
                ctx.SetFill(new Color(0.5f, 0, 0, 0.5f));

                if (lineRange.length > 1) {
                    // todo this doesn't really work yet
                    for (int i = lineRange.start + 1; i < lineRange.end - 1; i++) {
                        Rect rect = textInfo.GetLineRect(i);
                        rect.x += contentRect.x;
                        rect.y += contentRect.y;
                        ctx.Rect(rect);
                    }
                }
                else {
                    // todo the highlight is wrong when scrolled
                    Rect rect = textInfo.GetLineRect(lineRange.start);
                    Vector2 cursorPosition = textInfo.GetCursorPosition(selectionRange) - textScroll;
                    Vector2 selectPosition = textInfo.GetSelectionPosition(selectionRange) - textScroll;
                    float minX = Mathf.Min(cursorPosition.x, selectPosition.x);
                    float maxX = Mathf.Max(cursorPosition.x, selectPosition.x);
                    minX += contentRect.x;
                    maxX += contentRect.x;
                    rect.y += contentRect.y;
                    ctx.Rect(minX, rect.y, maxX - minX, rect.height);
                }

                ctx.Fill();
            }

//            ctx.BeginPath();
//            ctx.Rect(VisibleTextRect);
//            ctx.SetFill(new Color32(0, 255, 0, 125));
//            ctx.Fill();

            ctx.BeginPath();
            ctx.SetFill(style.TextColor);

            ctx.Text(contentRect.x - textScroll.x, contentRect.y, textInfo);
            ctx.Fill();
            ctx.DisableScissorRect();
        }

        protected Rect VisibleTextRect {
            get { return layoutResult.ContentRect; }
        }

        public void Focus() {
            if (GetAttribute("disabled") != null) {
                return;
            }

            hasFocus = true;
        }

        public void Blur() {
            hasFocus = false;
        }

        public void OnStylePropertiesDidChange() {
            textInfo.SetSpanStyle(0, style.GetTextStyle());
            textInfo.Layout();
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
                _uiInputElement.selectionRange = _uiInputElement.textInfo.SelectToPoint(_uiInputElement.selectionRange, mouse);
            }

            public override void OnComplete() {
                _uiInputElement.isSelecting = false;
                _uiInputElement.selectionRange = _uiInputElement.selectionRange.Invert();
            }

        }

        public void Paint(VertigoContext ctx, in Matrix4x4 matrix) {
            throw new NotImplementedException();
        }

    }

}