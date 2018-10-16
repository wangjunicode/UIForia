using System;
using System.Collections.Generic;
using Src;
using Src.Input;
using UnityEngine;

public abstract partial class InputSystem {

    public DragEvent CurrentDragEvent => m_CurrentDragEvent;

    private void ProcessDragEvents() {
        if (IsDragging) {
            if (m_MouseState.isLeftMouseUpThisFrame) {
                EndDrag();
                m_MouseDownElements.Clear();
            }
            else {
                UpdateDrag();
            }

            return;
        }

        if (m_MouseState.isLeftMouseDown) {
            if (Vector2.Distance(m_MouseState.mouseDownPosition, m_MouseState.mousePosition) >= k_DragThreshold) {
                BeginDrag();
            }

            return;
        }

        IsDragging = false;
    }

    private void UpdateDrag(bool firstFrame = false) {
        if (m_CurrentDragEvent == null) {
            return;
        }

        m_CurrentDragEvent.MousePosition = MousePosition;
        m_CurrentDragEvent.Modifiers = modifiersThisFrame;

        if (firstFrame) {
            RunDragEvent(m_ElementsThisFrame, InputEventType.DragEnter);
            m_CurrentDragEvent.Update();
        }
        else {
            RunDragEvent(m_ExitedElements, InputEventType.DragExit);
            RunDragEvent(m_EnteredElements, InputEventType.DragEnter);
            m_CurrentDragEvent.Update();
            RunDragEvent(m_ElementsThisFrame, m_MouseState.DidMove ? InputEventType.DragMove : InputEventType.DragHover);
        }

        if (m_CurrentDragEvent.IsCanceled) {
            EndDrag();
        }

        if (m_CurrentDragEvent.IsDropped) {
            // evt.Drop(success);         
        }
    }

    private void BeginDrag() {
        IsDragging = true;
        m_EventPropagator.Reset(m_MouseState);
        MouseInputEvent mouseEvent = new MouseInputEvent(m_EventPropagator, InputEventType.DragCreate, modifiersThisFrame);

        for (int i = 0; i < m_MouseDownElements.Count; i++) {
            DragCreatorGroup dragCreatorGroup;
            UIElement element = m_MouseDownElements[i];

            if (!m_DragCreatorMap.TryGetValue(element.id, out dragCreatorGroup)) {
                continue;
            }

            // todo -- figure out if these should respect propagation
            dragCreatorGroup.context.SetContextValue(element, k_EventAlias, mouseEvent);

            m_CurrentDragEvent = dragCreatorGroup.TryCreateEvent(element, mouseEvent);

            dragCreatorGroup.context.RemoveContextValue<MouseInputEvent>(element, k_EventAlias);

            if (m_CurrentDragEvent == null) {
                continue;
            }

            m_CurrentDragEvent.StartTime = Time.realtimeSinceStartup;
            m_CurrentDragEvent.DragStartPosition = MousePosition;

            UpdateDrag(true);
            return;
        }
    }

    private void EndDrag() {
        IsDragging = false;
        if (m_CurrentDragEvent == null) {
            return;
        }
    }

    private void RunDragEvent(List<UIElement> elements, InputEventType eventType) {
        if (m_CurrentDragEvent.IsCanceled) return;

        m_CurrentDragEvent.CurrentEventType = eventType;
        m_CurrentDragEvent.source = m_EventPropagator;

        m_EventPropagator.Reset(m_MouseState);
//        MouseInputEvent mouseEvent = new MouseInputEvent(m_EventPropagator, eventType, modifiersThisFrame);
//        object boxedEvent = mouseEvent;
        for (int i = 0; i < elements.Count; i++) {
            UIElement element = elements[i];
            DragHandlerGroup dragHandlerGroup;

            if (!m_DragHandlerMap.TryGetValue(element.id, out dragHandlerGroup)) {
                continue;
            }

            if ((dragHandlerGroup.handledEvents & eventType) == 0) {
                continue;
            }

            DragEventHandler[] handlers = dragHandlerGroup.handlers;
            dragHandlerGroup.context.SetContextValue(element, k_EventAlias, m_CurrentDragEvent);

            for (int j = 0; j < handlers.Length; j++) {
                DragEventHandler handler = handlers[j];
                if (handler.eventType != eventType) {
                    continue;
                }

                if (handler.eventPhase != EventPhase.Bubble) {
                    m_DragEventCaptureList.Add(ValueTuple.Create(handler, element, dragHandlerGroup.context));
                    continue;
                }

                handler.Invoke(element, dragHandlerGroup.context, m_CurrentDragEvent);

                if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                    break;
                }
            }

            dragHandlerGroup.context.RemoveContextValue<DragEvent>(element, k_EventAlias);

            if (m_EventPropagator.shouldStopPropagation) {
                break;
            }

            if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                m_DragEventCaptureList.Clear();
                return;
            }
        }

        for (int i = 0; i < m_DragEventCaptureList.Count; i++) {
            DragEventHandler handler = m_DragEventCaptureList[i].Item1;
            UIElement element = m_DragEventCaptureList[i].Item2;
            UITemplateContext context = m_DragEventCaptureList[i].Item3;
            context.SetContextValue(element, k_EventAlias, m_CurrentDragEvent);

            handler.Invoke(element, context, m_CurrentDragEvent);

            context.RemoveContextValue<DragEvent>(element, k_EventAlias);

            if (m_EventPropagator.shouldStopPropagation) {
                m_DragEventCaptureList.Clear();
                return;
            }
        }

        m_DragEventCaptureList.Clear();
    }

}