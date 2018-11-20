using UIForia;
using UIForia.Input;
using UIForia.Systems;

public abstract partial class InputSystem {

    public void OnReset() {
        // don't clear key states
        m_FocusedElement = null;
        m_ElementsLastFrame.Clear();
        m_ElementsThisFrame.Clear();
        m_MouseDownElements.Clear();
        m_KeyboardEventTree.Clear();
        m_MouseHandlerMap.Clear();
        m_DragCreatorMap.Clear();
        m_DragHandlerMap.Clear();
    }

    public void OnDestroy() { }
    
    public void OnViewAdded(UIView view) { }

    public void OnViewRemoved(UIView view) { }

    public void OnElementEnabled(UIElement element) { }

    public void OnElementDisabled(UIElement element) { }

    public void OnElementDestroyed(UIElement element) {
        m_ElementsLastFrame.Remove(element);
        m_ElementsThisFrame.Remove(element);
        m_MouseDownElements.Remove(element);
        m_KeyboardEventTree.RemoveHierarchy(element);
        // todo -- clear child handlers
        m_MouseHandlerMap.Remove(element.id);
        m_DragCreatorMap.Remove(element.id);
        m_DragHandlerMap.Remove(element.id);
    }

    public void OnElementCreated(UIElement element) {
        UITemplate template = element.OriginTemplate;
        MouseEventHandler[] mouseHandlers = template.mouseEventHandlers;
        DragEventCreator[] dragEventCreators = template.dragEventCreators;
        DragEventHandler[] dragEventHandlers = template.dragEventHandlers;
        KeyboardEventHandler[] keyboardHandlers = template.keyboardEventHandlers;

        if (mouseHandlers != null && mouseHandlers.Length > 0) {
            InputEventType handledEvents = 0;

            for (int i = 0; i < mouseHandlers.Length; i++) {
                handledEvents |= mouseHandlers[i].eventType;
            }

            m_MouseHandlerMap[element.id] = new MouseHandlerGroup(element.TemplateContext, mouseHandlers, handledEvents);
        }

        if (dragEventHandlers != null && dragEventHandlers.Length > 0) {
            InputEventType handledEvents = 0;

            for (int i = 0; i < dragEventHandlers.Length; i++) {
                handledEvents |= dragEventHandlers[i].eventType;
            }

            m_DragHandlerMap[element.id] = new DragHandlerGroup(element.TemplateContext, dragEventHandlers, handledEvents);
        }

        if (keyboardHandlers != null && keyboardHandlers.Length > 0) {
            m_KeyboardEventTree.AddItem(new KeyboardEventTreeNode(element, keyboardHandlers));
        }

        if (dragEventCreators != null) {
            m_DragCreatorMap[element.id] = new DragCreatorGroup(element.TemplateContext, dragEventCreators);
        }

        element.Input = this;

        if (element.children == null) {
            return;
        }

        for (int i = 0; i < element.children.Length; i++) {
            OnElementCreated(element.children[i]);
        }
    }

}