using Src;
using Src.Input;
using Src.Systems;

public abstract partial class InputSystem {

    public void OnReady() { }

    public void OnInitialize() { }

    public void OnReset() {
        // don't clear key states
        m_FocusedId = -1;
        m_LayoutResultCount = 0;
        m_ElementsLastFrame.Clear();
        m_ElementsThisFrame.Clear();
        keyboardEventTree.Clear();
        m_MouseHandlerMap.Clear();
    }

    public void OnDestroy() { }

    public void OnElementEnabled(UIElement element) { }

    public void OnElementDisabled(UIElement element) { }

    public void OnElementDestroyed(UIElement element) {
        m_ElementsLastFrame.Remove(element);
        m_ElementsThisFrame.Remove(element);
        keyboardEventTree.RemoveHierarchy(element);
        // todo -- clear child handlers
        m_MouseHandlerMap.Remove(element.id);
    }

    public void OnElementShown(UIElement element) { }

    public void OnElementHidden(UIElement element) { }

    public void OnElementCreated(InitData elementData) {
        MouseEventHandler[] mouseHandlers = elementData.mouseEventHandlers;
        KeyboardEventHandler[] keyboardHandlers = elementData.keyboardEventHandlers;

        if (mouseHandlers != null && mouseHandlers.Length > 0) {
            InputEventType handledEvents = 0;

            for (int i = 0; i < mouseHandlers.Length; i++) {
                handledEvents |= mouseHandlers[i].eventType;
            }

            m_MouseHandlerMap[elementData.elementId] = new MouseHandlerGroup(elementData.context, mouseHandlers, handledEvents);
        }

        if (keyboardHandlers != null && keyboardHandlers.Length > 0) {
            keyboardEventTree.AddItem(new KeyboardEventTreeNode(elementData.element, keyboardHandlers));
        }

        for (int i = 0; i < elementData.children.Count; i++) {
            OnElementCreated(elementData.children[i]);
        }
    }

}