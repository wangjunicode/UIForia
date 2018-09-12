using System;
using System.Collections.Generic;
using System.Reflection;
using Src.Input;
using UnityEngine;

namespace Src.Compilers {

    public class InputBindingCompiler {

        private readonly ExpressionCompiler compiler;

        private static readonly Dictionary<Type, List<KeyboardEventHandler>> s_KeyboardHandlerCache = new Dictionary<Type, List<KeyboardEventHandler>>();
        private static readonly Dictionary<Type, List<MouseEventHandler>> s_MouseHandlerCache = new Dictionary<Type, List<MouseEventHandler>>();

        public InputBindingCompiler(ContextDefinition context) {
            this.compiler = new ExpressionCompiler(context);
        }

        public void SetContext(ContextDefinition context) {
            compiler.SetContext(context);
        }

        public List<MouseEventHandler> CompileMouseEventHandlers(Type targetType, List<AttributeDefinition> attributeDefinitions) {
            List<MouseEventHandler> handlersFromTemplateAttrs = CompileMouseTemplateAttributes(targetType, attributeDefinitions);
            List<MouseEventHandler> handlersFromClassAttrs = CompileMouseInputClassAttributes(targetType);

            if (handlersFromClassAttrs == null) {
                return handlersFromTemplateAttrs;
            }

            if (handlersFromTemplateAttrs == null) {
                return handlersFromClassAttrs;
            }

            handlersFromTemplateAttrs.AddRange(handlersFromClassAttrs);
            return handlersFromTemplateAttrs;
        }

        public List<KeyboardEventHandler> CompileKeyboardEventHandlers(Type targetType, List<AttributeDefinition> attributeDefinitions) {
            List<KeyboardEventHandler> handlersFromTemplateAttrs = CompileKeyboardTemplateAttributes(attributeDefinitions);
            List<KeyboardEventHandler> handlersFromClassAttrs = CompileKeyboardClassAttributes(targetType);

            if (handlersFromClassAttrs == null) {
                return handlersFromTemplateAttrs;
            }

            if (handlersFromTemplateAttrs == null) {
                return handlersFromClassAttrs;
            }

            handlersFromTemplateAttrs.AddRange(handlersFromClassAttrs);
            return handlersFromTemplateAttrs;
        }

        private List<KeyboardEventHandler> CompileKeyboardClassAttributes(Type type) {
            if (s_KeyboardHandlerCache.ContainsKey(type)) {
                return s_KeyboardHandlerCache[type];
            }

            MethodInfo[] methods = type.GetMethods(ReflectionUtil.InstanceBindFlags);

            if (methods.Length == 0) {
                s_KeyboardHandlerCache[type] = null;
                return null;
            }

            List<KeyboardEventHandler> retn = new List<KeyboardEventHandler>();

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo info = methods[i];
                object[] customAttributes = info.GetCustomAttributes(typeof(KeyboardInputBindingAttribute), true);

                for (int j = 0; j < customAttributes.Length; j++) {
                    KeyboardInputBindingAttribute attr = (KeyboardInputBindingAttribute) customAttributes[j];

                    ParameterInfo[] parameters = info.GetParameters();
                    KeyboardEventHandler handler = null;
                    Type handlerType = null;
                    Type openDelegateType = ReflectionUtil.GetOpenDelegateType(info);
                    ReflectionUtil.ObjectArray1[0] = ReflectionUtil.CreateOpenDelegate(openDelegateType, info);

                    switch (parameters.Length) {
                        case 0: {
                            handlerType = ReflectionUtil.CreateGenericType(typeof(KeyboardEventHandler_IgnoreEvent<>), type);
                            break;
                        }
                        case 1: {
                            handlerType = ReflectionUtil.CreateGenericType(typeof(KeyboardEventHandler_WithEvent<>), type);
                            break;
                        }
                        default:
                            throw new Exception("Method with attribute " + attr.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(KeyboardInputEvent));
                    }

                    handler = (KeyboardEventHandler) ReflectionUtil.CreateGenericInstance(handlerType, ReflectionUtil.ObjectArray1);

#if DEBUG
                    handler.methodInfo = info;
#endif
                    handler.eventType = attr.eventType;
                    handler.keyCode = attr.key;
                    handler.character = attr.character;
                    handler.requiredModifiers = attr.modifiers;
                    handler.requiresFocus = attr.requiresFocus;

                    retn.Add(handler);
                }
            }

            if (retn.Count == 0) {
                s_KeyboardHandlerCache[type] = null;
                return null;
            }

            s_KeyboardHandlerCache[type] = retn;

            return retn;
        }

        private KeyboardEventHandler CompileKeyboardTemplateAttribute(AttributeDefinition attr) {
            for (int i = 0; i < s_KeyboardAttributeDefs.Length; i++) {
                if (attr.key == s_KeyboardAttributeDefs[i].attrName) {
                    InputAttributeTuple tuple = s_KeyboardAttributeDefs[i];

                    string source = attr.value;

                    if (source[0] != '{') {
                        source = '{' + attr.value + '}';
                    }

                    compiler.AddRuntimeAlias(tuple.alias.Item1, tuple.alias.Item2);
                    Expression<Terminal> expression = compiler.Compile<Terminal>(source);
                    compiler.RemoveRuntimeAlias(tuple.alias.Item1);
                    attr.isCompiled = true;
                    return new KeyboardEventHandler_Expression(tuple.eventType, expression);
                }
            }

            return null;
        }

        private MouseEventHandler CompileMouseAttribute(AttributeDefinition attr) {
            if (!attr.key.Contains("Mouse")) {
                return null;
            }

            for (int i = 0; i < s_MouseAttributeDefs.Length; i++) {
                if (attr.key.StartsWith(s_MouseAttributeDefs[i].attrName)) {
                    InputAttributeTuple tuple = s_MouseAttributeDefs[i];


                    // todo -- this stuff
                    // bool isBubble = attr.key.Contains(".bubble");
                    // bool isOnce = attr.key.Contains(".once");
                    //.shift
                    //.control
                    //.alt

                    EventPhase phase = EventPhase.Bubble;
                    KeyboardModifiers modifiers = KeyboardModifiers.None;

                    if (attr.key != tuple.attrName) {
                        bool isCapture = attr.key.Contains(".capture");
                        bool isShift = attr.key.Contains(".shift");
                        bool isControl = attr.key.Contains(".ctrl") || attr.key.Contains(".control");
                        bool isCommand = attr.key.Contains(".cmd") || attr.key.Contains(".command");
                        bool isAlt = attr.key.Contains(".alt");

                        if (isShift) {
                            modifiers |= KeyboardModifiers.Shift;
                        }

                        if (isControl) {
                            modifiers |= KeyboardModifiers.Control;
                        }

                        if (isCommand) {
                            modifiers |= KeyboardModifiers.Command;
                        }

                        if (isAlt) {
                            modifiers |= KeyboardModifiers.Alt;
                        }

                        if (isCapture) {
                            phase = EventPhase.Capture;
                        }
                    }

                    string source = attr.value;


                    if (source[0] != '{') {
                        source = '{' + attr.value + '}';
                    }

                    compiler.AddRuntimeAlias(tuple.alias.Item1, tuple.alias.Item2);
                    Expression<Terminal> expression = compiler.Compile<Terminal>(source);
                    compiler.RemoveRuntimeAlias(tuple.alias.Item1);
                    attr.isCompiled = true;
                    return new MouseEventHandler_Expression(tuple.eventType, expression, modifiers, phase);
                }
            }

            return null;
        }

        private List<KeyboardEventHandler> CompileKeyboardTemplateAttributes(List<AttributeDefinition> attributeDefinitions) {
            if (attributeDefinitions == null) return null;

            List<KeyboardEventHandler> retn = null;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled || attributeDefinitions[i].isRealAttribute) {
                    continue;
                }

                KeyboardEventHandler binding = CompileKeyboardTemplateAttribute(attributeDefinitions[i]);
                if (binding != null) {
                    retn = retn ?? new List<KeyboardEventHandler>();
                    retn.Add(binding);
                }
            }

            return retn;
        }

        private List<MouseEventHandler> CompileMouseTemplateAttributes(Type targetType, List<AttributeDefinition> attributeDefinitions) {
            if (attributeDefinitions == null) return null;

            List<MouseEventHandler> retn = null;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled || attributeDefinitions[i].isRealAttribute) {
                    continue;
                }

                MouseEventHandler binding = CompileMouseAttribute(attributeDefinitions[i]);
                if (binding != null) {
                    retn = retn ?? new List<MouseEventHandler>();
                    retn.Add(binding);
                }
            }

            return retn;
        }

        private List<MouseEventHandler> CompileMouseInputClassAttributes(Type targetType) {
            if (s_MouseHandlerCache.ContainsKey(targetType)) {
                return s_MouseHandlerCache[targetType];
            }

            List<MouseEventHandler> retn = null;

            MethodInfo[] methods = targetType.GetMethods(ReflectionUtil.InstanceBindFlags);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo info = methods[i];
                object[] customAttributes = info.GetCustomAttributes(typeof(MouseInputBindingAttribute), true);

                for (int j = 0; j < customAttributes.Length; j++) {
                    MouseInputBindingAttribute attr = (MouseInputBindingAttribute) customAttributes[j];

                    ParameterInfo[] parameters = info.GetParameters();
                    Type handlerType = null;
                    MouseEventHandler handler = null;
                    Type openDelegateType = ReflectionUtil.GetOpenDelegateType(info);

                    ReflectionUtil.ObjectArray4[0] = attr.eventType;
                    ReflectionUtil.ObjectArray4[1] = attr.modifiers;
                    ReflectionUtil.ObjectArray4[2] = attr.phase;
                    ReflectionUtil.ObjectArray4[3] = ReflectionUtil.CreateOpenDelegate(openDelegateType, info);

                    switch (parameters.Length) {
                        case 0: {
                            handlerType = ReflectionUtil.CreateGenericType(typeof(MouseEventHandler_IgnoreEvent<>), targetType);
                            break;
                        }
                        case 1: {
                            System.Diagnostics.Debug.Assert(parameters[0].ParameterType == typeof(MouseInputEvent));
                            handlerType = ReflectionUtil.CreateGenericType(typeof(MouseEventHandler_WithEvent<>), targetType);
                            break;
                        }
                        default:
                            throw new Exception("Method with attribute " + attr.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(MouseInputEvent));
                    }

                    handler = (MouseEventHandler) ReflectionUtil.CreateGenericInstance(handlerType, ReflectionUtil.ObjectArray4);
#if DEBUG
                    handler.methodInfo = info;
#endif

                    if (retn == null) {
                        retn = new List<MouseEventHandler>();
                    }

                    retn.Add(handler);
                }
            }

            if (retn == null || retn.Count == 0) {
                s_MouseHandlerCache[targetType] = null;
                return null;
            }

            s_MouseHandlerCache[targetType] = retn;

            return retn;
        }

        private static readonly ValueTuple<string, Type> s_MouseEventAlias = ValueTuple.Create("$event", typeof(MouseInputEvent));
        private static readonly ValueTuple<string, Type> s_KeyboardEventAlias = ValueTuple.Create("$event", typeof(KeyboardInputEvent));
        private static readonly ValueTuple<string, Type> s_FocusEventAlias = ValueTuple.Create("$event", typeof(FocusEvent));

        private static readonly InputAttributeTuple[] s_MouseAttributeDefs = {
            new InputAttributeTuple("onMouseEnter", InputEventType.MouseEnter, s_MouseEventAlias),
            new InputAttributeTuple("onMouseExit", InputEventType.MouseExit, s_MouseEventAlias),
            new InputAttributeTuple("onMouseDown", InputEventType.MouseDown, s_MouseEventAlias),
            new InputAttributeTuple("onMouseUp", InputEventType.MouseUp, s_MouseEventAlias),
            new InputAttributeTuple("onMouseMove", InputEventType.MouseMove, s_MouseEventAlias),
            new InputAttributeTuple("onMouseHover", InputEventType.MouseHover, s_MouseEventAlias),
            new InputAttributeTuple("onMouseScroll", InputEventType.MouseScroll, s_MouseEventAlias),
            new InputAttributeTuple("onMouseContext", InputEventType.MouseContext, s_MouseEventAlias),
        };

        private static readonly InputAttributeTuple[] s_KeyboardAttributeDefs = {
            new InputAttributeTuple("onKeyDown", InputEventType.KeyDown, s_KeyboardEventAlias),
            new InputAttributeTuple("onKeyUp", InputEventType.KeyUp, s_KeyboardEventAlias),
        };

        private static readonly InputAttributeTuple[] s_FocusAttributeDefs = {
            new InputAttributeTuple("onFocus", InputEventType.Focus, s_FocusEventAlias),
            new InputAttributeTuple("onBlur", InputEventType.Blur, s_KeyboardEventAlias),
        };

        private struct InputAttributeTuple {

            public readonly string attrName;
            public readonly InputEventType eventType;
            public readonly ValueTuple<string, Type> alias;

            public InputAttributeTuple(string attrName, InputEventType eventType, ValueTuple<string, Type> alias) {
                this.attrName = attrName;
                this.eventType = eventType;
                this.alias = alias;
            }

        }

    }

}