using System;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Exceptions;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct InputHandlerDescriptor {

        public InputEventType handlerType;
        public KeyboardModifiers modifiers;
        public EventPhase eventPhase;
        public bool requiresFocus;

    }

    public struct InputHandler {

        public Action<GenericInputEvent> handler;
        public InputHandlerDescriptor descriptor;
        public MethodInfo methodInfo;
        public bool useEventParameter;

    }

    public static class InputCompiler {

        private static readonly char[] s_SplitDot = {'.'};

        public static InputHandlerDescriptor ParseMouseDescriptor(string input) {
            InputHandlerDescriptor retn = default;
            retn.eventPhase = EventPhase.Bubble;
            retn.modifiers = KeyboardModifiers.None;
            retn.requiresFocus = false;

            input = input.ToLower();

            int dotIndex = input.IndexOf('.');

            if (dotIndex == -1) {
                retn.handlerType = ParseMouseInputEventType(input);
                return retn;
            }
            else {
                string[] parts = input.Split(s_SplitDot, StringSplitOptions.RemoveEmptyEntries);

                retn.handlerType = ParseMouseInputEventType(parts[0]);

                for (int i = 1; i < parts.Length; i++) {
                    string part = parts[i];

                    switch (part) {
                        case "capture": {
                            retn.eventPhase = EventPhase.Capture;
                            break;
                        }
                        case "shift": {
                            retn.modifiers |= KeyboardModifiers.Shift;
                            break;
                        }
                        case "ctrl":
                        case "control": {
                            retn.modifiers |= KeyboardModifiers.Control;
                            break;
                        }
                        case "cmd":
                        case "command": {
                            retn.modifiers |= KeyboardModifiers.Command;
                            break;
                        }
                        case "alt": {
                            retn.modifiers |= KeyboardModifiers.Alt;
                            break;
                        }
                        default:
                            throw new ParseException("Invalid mouse modifier: " + part + " in input string: " + input);
                    }
                }
            }

            return retn;
        }

        private static InputEventType ParseMouseInputEventType(string input) {
            switch (input) {
                case "click":
                    return InputEventType.MouseClick;

                case "down":
                    return InputEventType.MouseDown;

                case "up":
                    return InputEventType.MouseUp;

                case "enter":
                    return InputEventType.MouseEnter;

                case "exit":
                    return InputEventType.MouseExit;

                case "helddown":
                    return InputEventType.MouseHeldDown;

                case "move":
                    return InputEventType.MouseMove;

                case "hover":
                    return InputEventType.MouseHover;

                case "scroll":
                    return InputEventType.MouseScroll;

                case "context":
                    return InputEventType.MouseContext;

                default:
                    throw new CompileException("Invalid mouse event in template: " + input);
            }
        }

        public static StructList<InputHandler> CompileInputAnnotations(Type targetType) {
            StructList<InputHandler> handlers = new StructList<InputHandler>();
            MethodInfo[] methods = ReflectionUtil.GetInstanceMethods(targetType);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo methodInfo = methods[i];
                
                GetMouseEventHandlers(methodInfo, handlers);
                
                // GetKeyboardEventHandlers(methodInfo, handlers);
            }

            return handlers;
        }

        private static void GetMouseEventHandlers(MethodInfo methodInfo, StructList<InputHandler> handlers) {
            object[] customAttributes = methodInfo.GetCustomAttributes(typeof(MouseEventHandlerAttribute), true);

            if (customAttributes.Length == 0) {
                return;
            }
                        
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length > 1 ||(parameters.Length > 1 && parameters[0].ParameterType != typeof(MouseInputEvent))) {
                throw new Exception("Method with attribute " + customAttributes.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(MouseInputEvent));
            }

            for (int j = 0; j < customAttributes.Length; j++) {
                MouseEventHandlerAttribute attr = (MouseEventHandlerAttribute) customAttributes[j];
                handlers.Add(new InputHandler() {
                    descriptor = new InputHandlerDescriptor() {
                        eventPhase = attr.phase,
                        modifiers = attr.modifiers,
                        requiresFocus = false,
                        handlerType = attr.eventType
                    },
                    methodInfo = methodInfo,
                    useEventParameter = parameters.Length == 1
                });
            }
        }

    }

}