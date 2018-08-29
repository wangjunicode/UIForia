using System;
using System.Collections.Generic;
using Rendering;
using Src.Input;
using Src.InputBindings;
using UnityEngine;

namespace Src.Systems {

    public class GOInputSystem : IInputSystem {

        public const string EventAlias = "$event";

        private readonly IStyleSystem styleSystem;
        private readonly ILayoutSystem layoutSystem;

        private HashSet<int> hoverStylesThisFrame;
        private HashSet<int> hoverStylesLastFrame;
        private HashSet<int> elementsThisFrame;
        private HashSet<int> elementsLastFrame;

        private readonly HashSet<int> hoverStyles;
        private readonly IElementRegistry elementSystem;
        private readonly Dictionary<int, InputBindingGroup> bindingMap;
        private int[] scratchArray;
        private int resultCount;
        private LayoutResult[] queryResults;
        private Vector2 mousePosition;

        public GOInputSystem(ILayoutSystem layoutSystem, IElementRegistry elementSystem, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;
            this.elementSystem = elementSystem;

            this.hoverStyles = new HashSet<int>();
            this.hoverStylesThisFrame = new HashSet<int>();
            this.hoverStylesLastFrame = new HashSet<int>();

            this.elementsThisFrame = new HashSet<int>();
            this.elementsLastFrame = new HashSet<int>();

            this.bindingMap = new Dictionary<int, InputBindingGroup>();

            this.styleSystem.onAvailableStatesChanged += HandleStatefulStyle;
            queryResults = new LayoutResult[16];
            scratchArray = new int[16];
        }

        public void OnReset() {
            resultCount = 0;
            elementsLastFrame.Clear();
            elementsThisFrame.Clear();
            hoverStyles.Clear();
            hoverStylesLastFrame.Clear();
            hoverStylesThisFrame.Clear();
            bindingMap.Clear();
        }

        public enum MouseState {

            ContextClick,
            MouseDown,
            MouseUp,
            MoveMove,
            MouseEnter,
            MouseExit,

        }

        public void OnUpdate() {
            Vector2 positionLastFrame = mousePosition;
            mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);
            QueryLayout();
            MouseState state = 0;
            if (UnityEngine.Input.GetMouseButtonDown(0)) {
                RunMouseEvent(InputEventType.MouseDown);
            }
            RunMouseEvent(InputEventType.MouseMove);
            // UnityEngine.Input.mousePosition;
            // UnityEngine.Input.GetMouseButtonDown()
            // UnityEngine.Input.GetMouseButton(0);
//            switch (Event.current.type) {
//                case EventType.ContextClick:
//                    mousePosition = Event.current.mousePosition;
//                    RunMouseEvent(InputEventType.MouseContext);
//                    break;
//                case EventType.MouseDown:
//                    if (Event.current.button == 1) break;
//                    mousePosition = Event.current.mousePosition;
//                    RunMouseEvent(InputEventType.MouseDown);
//                    break;
//                case EventType.MouseUp:
//                    mousePosition = Event.current.mousePosition;
//                    RunMouseEvent(InputEventType.MouseUp);
//                    break;
//                case EventType.MouseMove:
//                    mousePosition = Event.current.mousePosition;
//                    RunMouseEvent(InputEventType.MouseMove);
//                    break;
//                case EventType.MouseEnterWindow:
//                    break;
//
//                case EventType.MouseLeaveWindow:
//                    break;
//
//                case EventType.ScrollWheel:
//
//                    break;
//                case EventType.MouseDrag:
//                    break;
//            }

            Swap(ref elementsLastFrame, ref elementsThisFrame);
            Swap(ref hoverStylesLastFrame, ref hoverStylesThisFrame);
            elementsThisFrame.Clear();
            hoverStylesThisFrame.Clear();
        }

        public void OnDestroy() {
            this.styleSystem.onAvailableStatesChanged -= HandleStatefulStyle;
        }

        public void OnReady() { }

        public void OnInitialize() { }

        private void HandleStatefulStyle(int elementId, StyleState availableStates) {
            bool hasHoverState = (availableStates & StyleState.Hover) != 0;
            if (hasHoverState) {
                hoverStyles.Add(elementId);
            }
            else {
                hoverStyles.Remove(elementId);
            }
        }

        private void QueryLayout() {
            resultCount = layoutSystem.QueryPoint(mousePosition, ref queryResults);

            for (int i = 0; i < resultCount; i++) {
                int elementId = queryResults[i].elementId;
                elementsThisFrame.Add(elementId);

                if (hoverStyles.Contains(elementId)) {
                    hoverStylesThisFrame.Add(elementId);
                    styleSystem.EnterState(elementId, StyleState.Hover);
                }
            }

            hoverStylesLastFrame.ExceptWith(hoverStylesThisFrame);
            foreach (int elementId in hoverStylesLastFrame) {
                styleSystem.ExitState(elementId, StyleState.Hover);
            }

            // elements this frame that were not in the set last frame
            // -> if has binding for mouse enter -> mouse enter
            // if was in last frame & not in this frame -> mouse exit

            RunMouseEnter();
            RunMouseExit();
        }

        private void RunMouseEnter() {
            if (elementsThisFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsThisFrame.CopyTo(scratchArray);
            InputEvent mouseEnter = new MouseInputEvent(InputEventType.MouseEnter, mousePosition);
            for (int i = 0; i < elementsThisFrame.Count; i++) {
                int elementId = scratchArray[i];
                if (!elementsLastFrame.Contains(elementId)) {
                    RunBindings(elementId, mouseEnter);
                }
            }
        }

        private void RunMouseExit() {
            if (elementsLastFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsLastFrame.CopyTo(scratchArray);
            InputEvent mouseExit = new MouseInputEvent(InputEventType.MouseExit, mousePosition);
            for (int i = 0; i < elementsLastFrame.Count; i++) {
                int elementId = scratchArray[i];
                if (!elementsThisFrame.Contains(elementId)) {
                    RunBindings(elementId, mouseExit);
                }
                else {
                    //RunBindings(elementId, mouseHover);
                }
            }
        }

        private void RunMouseEvent(InputEventType eventType) {
            if (elementsThisFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsThisFrame.CopyTo(scratchArray);
            InputEvent mouseEvent = new MouseInputEvent(eventType, mousePosition);
            for (int i = 0; i < elementsThisFrame.Count; i++) {
                RunBindings(scratchArray[i], mouseEvent);
            }
        }

        private void RunBindings(int elementId, InputEvent inputEvent) {
            InputBindingGroup bindingGroup;
            if (!bindingMap.TryGetValue(elementId, out bindingGroup)) {
                return;
            }

            if ((bindingGroup.handledEvents & inputEvent.type) == 0) {
                return;
            }

            InputBinding[] bindings = bindingGroup.bindings;
            UIElement element = elementSystem.GetElement(elementId);
            InputEventType eventType = inputEvent.type;

            bindingGroup.context.SetObjectAlias(EventAlias, inputEvent);
            for (int i = 0; i < bindings.Length; i++) {
                InputBinding binding = bindings[i];
                if (binding.eventType == eventType) {
                    binding.Execute(element, bindingGroup.context);
                }
            }

            bindingGroup.context.RemoveObjectAlias(EventAlias);
        }

        public void OnElementCreated(InitData elementData) {
            InputBinding[] inputBindings = elementData.inputBindings;

            if (inputBindings != null && inputBindings.Length > 0) {
                InputEventType handledEvents = 0;

                for (int i = 0; i < inputBindings.Length; i++) {
                    handledEvents |= inputBindings[i].eventType;
                }

                bindingMap[elementData.elementId] = new InputBindingGroup(elementData.context, inputBindings, handledEvents);
            }

            for (int i = 0; i < elementData.children.Count; i++) {
                OnElementCreated(elementData.children[i]);
            }
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            elementsLastFrame.Remove(element.id);
            elementsThisFrame.Remove(element.id);
            hoverStyles.Remove(element.id);
            hoverStylesLastFrame.Remove(element.id);
            hoverStylesThisFrame.Remove(element.id);
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        private static void Swap<T>(ref T lhs, ref T rhs) {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

    }

}