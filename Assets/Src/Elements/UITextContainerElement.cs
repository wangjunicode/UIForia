using System;
using JetBrains.Annotations;
using Src;
using TMPro;

public class UITextContainerElement : UIElement {

    // todo -- wrap these in an interface for interop with other text systems
    protected TMP_TextInfo m_TextInfo;
    protected TMP_FontAsset m_FontAsset;

    public event Action<int, string> onTextContentChanged;
    private UITextElement m_TextElement;

    public UITextContainerElement() {
        flags |= UIElementFlags.TextContainer;
    }

    public TMP_TextInfo textInfo {
        get { return m_TextInfo; }
        set {
            if (m_TextInfo != null) {
                throw new Exception("Can only set textInfo once");
            }

            m_TextInfo = value;
        }
    }

    public TMP_FontAsset fontAsset {
        get { return m_FontAsset; }
        set { m_FontAsset = value; }
    }

    public override void OnCreate() {
        m_TextElement = (UITextElement) ownChildren[0];
        m_TextElement.onTextChanged += HandleTextElementTextChanged;
        m_TextElement.DisableBinding("text");
        style.width = UIMeasurement.Content100;
        style.height = UIMeasurement.Content100;
    }

    public override void OnDestroy() {
        m_TextElement = (UITextElement) ownChildren[0];
        m_TextElement.onTextChanged -= HandleTextElementTextChanged;
    }

    private void HandleTextElementTextChanged(UITextElement element, string text) {
        onTextContentChanged?.Invoke(element.id, text);
    }

    [PublicAPI]
    public bool IsCharacterValid(char character) {
        return m_FontAsset.HasCharacter(character);
    }

    public void SetText(string text) {
        m_TextElement.SetText(text);
    }

}