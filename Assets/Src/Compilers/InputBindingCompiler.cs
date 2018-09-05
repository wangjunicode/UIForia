using System;
using System.Collections.Generic;
using System.Reflection;
using Src.Input;
using Src.InputBindings;

namespace Src.Compilers {

    public class InputBindingCompiler {

        private ContextDefinition context;
        private readonly ExpressionCompiler compiler;

        private static readonly Dictionary<Type, List<KeyboardEventHandler>> s_KeyboardHandlerCache = new Dictionary<Type, List<KeyboardEventHandler>>();

        public InputBindingCompiler(ContextDefinition context) {
            this.context = context;
            this.compiler = new ExpressionCompiler(context);
        }

        public void SetContext(ContextDefinition context) {
            this.context = context;
            this.compiler.SetContext(context);
        }

        public List<InputBinding> Compile(List<AttributeDefinition> attributeDefinitions) {
            List<InputBinding> retn = new List<InputBinding>();

            if (attributeDefinitions == null) return retn;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled) continue;
                InputBinding binding = CompileAttribute(attributeDefinitions[i]);
                if (binding != null) {
                    retn.Add(binding);
                }
            }

            return retn;
        }

        public List<KeyboardEventHandler> CompileKeyboardInputAttributes(Type type) {
            if (s_KeyboardHandlerCache.ContainsKey(type)) {
                return s_KeyboardHandlerCache[type];
            }

            MethodInfo[] methods = type.GetMethods(ReflectionUtil.InstanceBindFlags);

            if (methods.Length == 0) {
                s_KeyboardHandlerCache[type] = null;
                return null;
            }

            List<KeyboardEventHandler> retn = new List<KeyboardEventHandler>();
            ReflectionUtil.TypeArray1[0] = type;

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo info = methods[i];
                object[] customAttributes = info.GetCustomAttributes(typeof(KeyboardInputBindingAttribute), true);

                for (int j = 0; j < customAttributes.Length; j++) {
                    KeyboardInputBindingAttribute attr = (KeyboardInputBindingAttribute) customAttributes[j];

                    ParameterInfo[] parameters = info.GetParameters();
                    KeyboardEventHandler handler = null;

                    switch (parameters.Length) {
                        case 0: {
                            Type openDelegateType = ReflectionUtil.GetOpenDelegateType(info);
                            Type handlerType = ReflectionUtil.CreateGenericType(typeof(KeyboardEventHandlerIgnoreEvent<>), ReflectionUtil.TypeArray1[0]);
                            ReflectionUtil.ObjectArray1[0] = ReflectionUtil.CreateOpenDelegate(openDelegateType, info);
                            handler = (KeyboardEventHandler) ReflectionUtil.CreateGenericInstance(handlerType, ReflectionUtil.ObjectArray1);
                            break;
                        }
                        case 1: {
                            ReflectionUtil.TypeArray1[0] = type;
                            Type openDelegateType = ReflectionUtil.GetOpenDelegateType(info);
                            Type handlerType = ReflectionUtil.CreateGenericType(typeof(KeyboardEventHandler<>), ReflectionUtil.TypeArray1[0]);
                            ReflectionUtil.ObjectArray1[0] = ReflectionUtil.CreateOpenDelegate(openDelegateType, info);
                            handler = (KeyboardEventHandler) ReflectionUtil.CreateGenericInstance(handlerType, ReflectionUtil.ObjectArray1);
                            break;
                        }
                        default:
                            continue;
                    }

#if DEBUG
                    handler.methodInfo = info;
#endif
                    handler.eventType = attr.eventType;
                    handler.keyCode = attr.key;
                    handler.requiredModifiers = attr.modifiers;
                    handler.requiresFocus = attr.requireFocus;

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

        private Action<UIElement, KeyboardInputEvent> GetValidDelegate(Delegate callback) {
            if (callback.Method.ReturnType != typeof(void)) {
                throw new Exception("Return type of InputHandler function must be void");
            }

            ParameterInfo[] parameters = callback.Method.GetParameters();
            if (parameters.Length == 0) {
                Action<UIElement> actual = (Action<UIElement>) callback;
                return (element, evt) => { actual(element); };
            }

            if (parameters.Length == 1) {
                if (parameters[0].ParameterType == typeof(KeyboardInputEvent)) {
                    Action<UIElement, KeyboardInputEvent> actual = (Action<UIElement, KeyboardInputEvent>) callback;
                    return (element, evt) => { actual(element, evt); };
                }
            }

            return null;
        }

        private InputBinding CompileAttribute(AttributeDefinition attr) {
            for (int i = 0; i < s_InputAttributeDefs.Length; i++) {
                if (attr.key == s_InputAttributeDefs[i].attrName) {
                    InputAttributeTuple tuple = s_InputAttributeDefs[i];
                    string source = attr.value;
                    if (source[0] != '{') {
                        source = '{' + attr.value + '}';
                    }

                    context.AddRuntimeAlias(tuple.alias.Item1, tuple.alias.Item2);
                    Expression<Terminal> expression = compiler.Compile<Terminal>(source);
                    context.RemoveRuntimeAlias(tuple.alias.Item1);
                    attr.isCompiled = true;
                    return new InputBinding(tuple.eventType, expression);
                }
            }

            return null;
        }

        private static readonly ValueTuple<string, Type> mouseEventAlias = ValueTuple.Create("$event", typeof(MouseInputEvent));
        private static readonly ValueTuple<string, Type> keyboardEventAlias = ValueTuple.Create("$event", typeof(KeyboardInputEvent));

        private static readonly InputAttributeTuple[] s_InputAttributeDefs = {
            new InputAttributeTuple("onMouseEnter", InputEventType.MouseEnter, mouseEventAlias),
            new InputAttributeTuple("onMouseExit", InputEventType.MouseExit, mouseEventAlias),
            new InputAttributeTuple("onMouseDown", InputEventType.MouseDown, mouseEventAlias),
            new InputAttributeTuple("onMouseUp", InputEventType.MouseUp, mouseEventAlias),
            new InputAttributeTuple("onMouseMove", InputEventType.MouseMove, mouseEventAlias),
            new InputAttributeTuple("onMouseHover", InputEventType.MouseHover, mouseEventAlias),
            new InputAttributeTuple("onMouseScroll", InputEventType.MouseScroll, mouseEventAlias),
            new InputAttributeTuple("onMouseContext", InputEventType.MouseContext, mouseEventAlias),
            new InputAttributeTuple("onKeyDown", InputEventType.KeyDown, keyboardEventAlias),
            new InputAttributeTuple("onKeyUp", InputEventType.KeyUp, keyboardEventAlias),
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