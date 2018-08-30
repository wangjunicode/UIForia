using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputFieldFocusHandler : MonoBehaviour, ISelectHandler {

    public Action onFocus;
    public Action<string> onBlur;

    public void Awake() {
        GetComponent<UnityEngine.UI.InputField>().onEndEdit.AddListener(HandleBlur);
    }

    private void HandleBlur(string text) {
        onBlur?.Invoke(text);
    }

    public void OnSelect(BaseEventData eventData) {
        onFocus?.Invoke();
    }

    private void OnDestroy() {
        GetComponent<UnityEngine.UI.InputField>().onEndEdit.RemoveAllListeners();
    }

}