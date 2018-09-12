using Rendering;
using Src.Input;
using UnityEngine;

public abstract partial class InputSystem {

    public bool IsMouseLeftDown => m_MouseState.isLeftMouseDown;
    public bool IsMouseLeftDownThisFrame => m_MouseState.isLeftMouseDownThisFrame;
    public bool IsMouseLeftUpThisFrame => m_MouseState.isLeftMouseUpThisFrame;

    public bool IsMouseRightDown => m_MouseState.isRightMouseDown;
    public bool IsMouseRightDownThisFrame => m_MouseState.isRightMouseDownThisFrame;
    public bool IsMouseRightUpThisFrame => m_MouseState.isRightMouseUpThisFrame;

    public bool IsMouseMiddleDown => m_MouseState.isMiddleMouseDown;
    public bool IsMouseMiddleDownThisFrame => m_MouseState.isMiddleMouseDownThisFrame;
    public bool IsMouseMiddleUpThisFrame => m_MouseState.isMiddleMouseUpThisFrame;

    public Vector2 ScrollDelta => m_MouseState.scrollDelta;

    public Vector2 MousePosition => m_MouseState.mousePosition;
    public Vector2 MouseDownPosition => m_MouseState.mouseDownPosition;

    public bool IsDragging => m_MouseState.isDragging;

    private void ProcessMouseEvents() {
        m_LayoutResultCount = m_LayoutSystem.QueryPoint(m_MouseState.mousePosition, ref m_LayoutQueryResults);

        for (int i = 0; i < m_LayoutResultCount; i++) {
            int elementId = m_LayoutQueryResults[i].element.id;

            UIElement element = m_LayoutQueryResults[i].element;

            m_ElementsThisFrame.Add(element);

            if (!m_ElementsLastFrame.Contains(element)) {
                m_EnteredElements.Add(element);
                m_StyleSystem.EnterState(elementId, StyleState.Hover);
            }
        }

        for (int i = 0; i < m_ElementsLastFrame.Count; i++) {
            if (!m_ElementsThisFrame.Contains(m_ElementsLastFrame[i])) {
                m_ExitedElements.Add(m_ElementsLastFrame[i]);
                m_StyleSystem.ExitState(m_ElementsLastFrame[i].id, StyleState.Hover);
            }
        }

        m_EnteredElements.Sort(s_DepthComparer);
        m_ElementsThisFrame.Sort(s_DepthComparer);

        RunMouseEvents(m_EnteredElements, InputEventType.MouseEnter);
        RunMouseEvents(m_ExitedElements, InputEventType.MouseExit);
        
        if (m_MouseState.isLeftMouseDownThisFrame) {
            RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseDown);
        }
        else if (m_MouseState.isLeftMouseUpThisFrame) {
            RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseUp);
        }

        RunMouseEvents(m_ElementsThisFrame, m_MouseState.DidMove ? InputEventType.MouseMove : InputEventType.MouseHover);

        m_EnteredElements.Clear();
        m_ExitedElements.Clear();
    }

}