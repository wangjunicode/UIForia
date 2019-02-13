using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Input;

namespace UIForia.Compilers {

    public class InputBindingCompiler {

        private ExpressionCompiler compiler;

        private static readonly Dictionary<Type, List<KeyboardEventHandler>> s_KeyboardHandlerCache = new Dictionary<Type, List<KeyboardEventHandler>>();
        private static readonly Dictionary<Type, List<MouseEventHandler>> s_MouseHandlerCache = new Dictionary<Type, List<MouseEventHandler>>();
        private static readonly Dictionary<Type, List<DragEventCreator>> s_DragCreatorCache = new Dictionary<Type, List<DragEventCreator>>();
        private static readonly Dictionary<Type, List<DragEventHandler>> s_DragHandlerCache = new Dictionary<Type, List<DragEventHandler>>();

        private static readonly MouseEventResolver s_MouseEventResolver = new MouseEventResolver("$event");
        private static readonly KeyboardEventResolver s_KeyboardEventResolver = new KeyboardEventResolver("$event");
        
        public InputBindingCompiler() {
            this.compiler = new ExpressionCompiler();
        }

        public List<DragEventCreator> CompileDragEventCreators(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions, bool attributesOnly) {
            List<DragEventCreator> creatorsFromTemplateAttrs = CompileDragCreatorTemplateAttributes(rootType, elementType, attributeDefinitions);
            if (attributesOnly) {
                return creatorsFromTemplateAttrs;
            }
            List<DragEventCreator> creatorsFromClassAttrs = CompileDragCreatorClassAttributes(elementType);
            return Combine(creatorsFromClassAttrs, creatorsFromTemplateAttrs);
        }
  
        public List<MouseEventHandler> CompileMouseEventHandlers(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions, bool attributesOnly) {
            List<MouseEventHandler> handlersFromTemplateAttrs = CompileMouseEventTemplateAttributes(rootType, elementType, attributeDefinitions);
            if (attributesOnly) {
                return handlersFromTemplateAttrs;
            }
            List<MouseEventHandler> handlersFromClassAttrs = CompileMouseEventClassAttributes(elementType);
            return Combine(handlersFromTemplateAttrs, handlersFromClassAttrs);
        }

        public List<KeyboardEventHandler> CompileKeyboardEventHandlers(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions, bool attributesOnly) {
            List<KeyboardEventHandler> handlersFromTemplateAttrs = CompileKeyboardTemplateAttributes(rootType, elementType, attributeDefinitions);
            if (attributesOnly) {
                return handlersFromTemplateAttrs;
            }
            List<KeyboardEventHandler> handlersFromClassAttrs = CompileKeyboardClassAttributes(elementType);
            return Combine(handlersFromTemplateAttrs, handlersFromClassAttrs);
        }

        public List<DragEventHandler> CompileDragEventHandlers(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions, bool attributesOnly) {
            List<DragEventHandler> handlersFromTemplateAttrs = CompileDragEventHandlerTemplateAttributes(rootType, elementType, attributeDefinitions);
            if (attributesOnly) {
                return handlersFromTemplateAttrs;
            }
            List<DragEventHandler> handlersFromClassAttrs = CompileDragEventHandlerClassAttributes(elementType);
            return Combine(handlersFromClassAttrs, handlersFromTemplateAttrs);
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

        public List<KeyboardEventHandler> CompileKeyboardTemplateAttributes(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions) {
            if (attributeDefinitions == null) return null;

            List<KeyboardEventHandler> retn = null;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled || attributeDefinitions[i].isRealAttribute) {
                    continue;
                }

                KeyboardEventHandler binding = CompileKeyboardTemplateAttribute(rootType, elementType, attributeDefinitions[i]);
                if (binding != null) {
                    retn = retn ?? new List<KeyboardEventHandler>();
                    retn.Add(binding);
                }
            }

            return retn;
        }

        private KeyboardEventHandler CompileKeyboardTemplateAttribute(Type rootType, Type elementType, AttributeDefinition attr) {
            for (int i = 0; i < s_KeyboardAttributeDefs.Length; i++) {
                if (attr.key == s_KeyboardAttributeDefs[i].attrName) {
                    InputAttributeTuple tuple = s_KeyboardAttributeDefs[i];

                    string source = attr.value;

                    if (source[0] != '{') {
                        source = '{' + attr.value + '}';
                    }

                    compiler.AddAliasResolver(s_KeyboardEventResolver);
                    
                    Expression<Terminal> expression = compiler.Compile<Terminal>(rootType, elementType, source);
                    
                    compiler.RemoveAliasResolver(s_KeyboardEventResolver);
                    
                    attr.isCompiled = true;
                    return new KeyboardEventHandler_Expression(tuple.eventType, expression);
                }
            }

            return null;
        }

        private List<MouseEventHandler> CompileMouseEventTemplateAttributes(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions) {
            if (attributeDefinitions == null) return null;

            List<MouseEventHandler> retn = null;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled || attributeDefinitions[i].isRealAttribute) {
                    continue;
                }

                MouseEventHandler binding = CompileMouseTemplateAttribute(rootType, elementType, attributeDefinitions[i]);
                if (binding != null) {
                    retn = retn ?? new List<MouseEventHandler>();
                    retn.Add(binding);
                }
            }

            return retn;
        }

        private MouseEventHandler CompileMouseTemplateAttribute(Type rootType, Type elementType, AttributeDefinition attr) {
            if (!attr.key.Contains("Mouse")) {
                return null;
            }

            for (int i = 0; i < s_MouseAttributeDefs.Length; i++) {
                if (attr.key.StartsWith(s_MouseAttributeDefs[i].attrName)) {
                    InputAttributeTuple tuple = s_MouseAttributeDefs[i];

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

                    compiler.AddAliasResolver(s_MouseEventResolver);
                    
                    Expression<Terminal> expression = compiler.Compile<Terminal>(rootType, elementType, source);
                    
                    compiler.RemoveAliasResolver(s_MouseEventResolver);
                    
                    attr.isCompiled = true;
                    return new MouseEventHandler_Expression(tuple.eventType, expression, modifiers, phase);
                }
            }

            return null;
        }

        private List<MouseEventHandler> CompileMouseEventClassAttributes(Type targetType) {
            if (s_MouseHandlerCache.ContainsKey(targetType)) {
                return s_MouseHandlerCache[targetType];
            }

            List<MouseEventHandler> retn = null;

            MethodInfo[] methods = targetType.GetMethods(ReflectionUtil.InstanceBindFlags);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo info = methods[i];
                object[] customAttributes = info.GetCustomAttributes(typeof(MouseEventHandlerAttribute), true);

                for (int j = 0; j < customAttributes.Length; j++) {
                    MouseEventHandlerAttribute attr = (MouseEventHandlerAttribute) customAttributes[j];

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

            s_MouseHandlerCache[targetType] = retn;

            return retn;
        }

        private List<DragEventHandler> CompileDragEventHandlerClassAttributes(Type targetType) {
            if (s_DragHandlerCache.ContainsKey(targetType)) {
                return s_DragHandlerCache[targetType];
            }

            List<DragEventHandler> retn = null;

            MethodInfo[] methods = targetType.GetMethods(ReflectionUtil.InstanceBindFlags);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo info = methods[i];
                object[] customAttributes = info.GetCustomAttributes(typeof(DragEventHandlerAttribute), true);

                for (int j = 0; j < customAttributes.Length; j++) {
                    DragEventHandlerAttribute attr = (DragEventHandlerAttribute) customAttributes[j];

                    ParameterInfo[] parameters = info.GetParameters();
                    Type handlerType = null;
                    DragEventHandler handler = null;
                    Type openDelegateType = ReflectionUtil.GetOpenDelegateType(info);

                    ReflectionUtil.ObjectArray5[0] = attr.eventType;
                    ReflectionUtil.ObjectArray5[1] = attr.requiredType;
                    ReflectionUtil.ObjectArray5[2] = attr.modifiers;
                    ReflectionUtil.ObjectArray5[3] = attr.phase;
                    ReflectionUtil.ObjectArray5[4] = ReflectionUtil.CreateOpenDelegate(openDelegateType, info);

                    switch (parameters.Length) {
                        case 0: {
                            handlerType = ReflectionUtil.CreateGenericType(typeof(DragEventHandler_IgnoreEvent<>), targetType);
                            break;
                        }
                        case 1: {
                            System.Diagnostics.Debug.Assert(parameters[0].ParameterType == typeof(MouseInputEvent));
                            handlerType = ReflectionUtil.CreateGenericType(typeof(DragEventHandler_WithEvent<>), targetType);
                            break;
                        }
                        default:
                            throw new Exception("Method with attribute " + attr.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(MouseInputEvent));
                    }

                    handler = (DragEventHandler) ReflectionUtil.CreateGenericInstance(handlerType, ReflectionUtil.ObjectArray5);
#if DEBUG
                    handler.methodInfo = info;
#endif
                    
                    if (retn == null) {
                        retn = new List<DragEventHandler>();
                    }

                    retn.Add(handler);
                }
            }

            s_DragHandlerCache[targetType] = retn;

            return retn;
        }

        public List<DragEventHandler> CompileDragEventHandlerTemplateAttributes(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions) {
            if (attributeDefinitions == null) return null;

            List<DragEventHandler> retn = null;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled || attributeDefinitions[i].isRealAttribute) {
                    continue;
                }

                DragEventHandler handler = CompileDragHandlerTemplateAttribute(rootType, elementType, attributeDefinitions[i]);
                if (handler != null) {
                    retn = retn ?? new List<DragEventHandler>();
                    retn.Add(handler);
                }
            }

            return retn;
        }

        private DragEventHandler CompileDragHandlerTemplateAttribute(Type rootType, Type elementType, AttributeDefinition attr) {
            if (!attr.key.StartsWith("onDrag")) {
                return null;
            }

            for (int i = 0; i < s_DragAttributeDefs.Length; i++) {
                if (!attr.key.StartsWith(s_DragAttributeDefs[i].attrName)) {
                    continue;
                }

                InputAttributeTuple tuple = s_DragAttributeDefs[i];

                EventPhase phase = EventPhase.Bubble;
                KeyboardModifiers modifiers = KeyboardModifiers.None;

                if (attr.key != tuple.attrName) {
                    phase = GetPhase(attr.key);
                    modifiers = GetModifiers(attr.key);
                }

                string source = attr.value;

                if (source[0] != '{') {
                    source = '{' + attr.value + '}';
                }

                compiler.AddAliasResolver(s_MouseEventResolver);
                
                Expression<Terminal> expression = compiler.Compile<Terminal>(rootType, elementType, source);
                
                compiler.RemoveAliasResolver(s_MouseEventResolver);
                
                
                attr.isCompiled = true;
                return new DragEventHandler_Expression(tuple.eventType, expression, modifiers, phase);
            }

            return null;
        }

        private static EventPhase GetPhase(string attrKey) {
            return attrKey.Contains(".capture") ? EventPhase.Capture : EventPhase.Bubble;
        }

        private static KeyboardModifiers GetModifiers(string attrKey) {
            KeyboardModifiers modifiers = KeyboardModifiers.None;
            bool isShift = attrKey.Contains(".shift");
            bool isControl = attrKey.Contains(".ctrl") || attrKey.Contains(".control");
            bool isCommand = attrKey.Contains(".cmd") || attrKey.Contains(".command");
            bool isAlt = attrKey.Contains(".alt");
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

            return modifiers;
        }

        public List<DragEventCreator> CompileDragCreatorTemplateAttributes(Type rootType, Type elementType, List<AttributeDefinition> attributeDefinitions) {
            if (attributeDefinitions == null) return null;

            List<DragEventCreator> retn = null;

            for (int i = 0; i < attributeDefinitions.Count; i++) {
                if (attributeDefinitions[i].isCompiled || attributeDefinitions[i].isRealAttribute) {
                    continue;
                }

                DragEventCreator creator = CompileDragCreatorTemplateAttribute(rootType, elementType, attributeDefinitions[i]);
                if (creator != null) {
                    retn = retn ?? new List<DragEventCreator>();
                    retn.Add(creator);
                }
            }

            return retn;
        }

        private DragEventCreator CompileDragCreatorTemplateAttribute(Type rootType, Type elementType, AttributeDefinition attr) {
            if (!attr.key.StartsWith("onDragCreate")) {
                return null;
            }

            EventPhase phase = EventPhase.Bubble;
            KeyboardModifiers modifiers = KeyboardModifiers.None;

            if (attr.key != "onDragCreate") {
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

            compiler.AddAliasResolver(s_MouseEventResolver);

            Expression<DragEvent> expression = compiler.Compile<DragEvent>(rootType, elementType, source);

            compiler.RemoveAliasResolver(s_MouseEventResolver);
            
            attr.isCompiled = true;
            return new DragEventCreator_Expression(expression, modifiers, phase);
        }

        private List<DragEventCreator> CompileDragCreatorClassAttributes(Type targetType) {
            if (s_DragCreatorCache.ContainsKey(targetType)) {
                return s_DragCreatorCache[targetType];
            }

            List<DragEventCreator> retn = null;

            MethodInfo[] methods = targetType.GetMethods(ReflectionUtil.InstanceBindFlags);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo info = methods[i];
                object[] customAttributes = info.GetCustomAttributes(typeof(OnDragCreateAttribute), true);

                for (int j = 0; j < customAttributes.Length; j++) {
                    OnDragCreateAttribute attr = (OnDragCreateAttribute) customAttributes[j];

                    if (!typeof(DragEvent).IsAssignableFrom(info.ReturnType)) {
                        throw new Exception($"Methods annotated with {nameof(OnDragCreateAttribute)} must return an instance of {nameof(DragEvent)}");
                    }

                    Type handlerType;
                    ParameterInfo[] parameters = info.GetParameters();
                    Type openDelegateType = ReflectionUtil.GetOpenDelegateType(info);

                    ReflectionUtil.ObjectArray3[0] = attr.modifiers;
                    ReflectionUtil.ObjectArray3[1] = attr.phase;
                    ReflectionUtil.ObjectArray3[2] = ReflectionUtil.CreateOpenDelegate(openDelegateType, info);

                    switch (parameters.Length) {
                        case 0: {
                            handlerType = ReflectionUtil.CreateGenericType(typeof(DragEventCreator_IgnoreEvent<>), targetType);
                            break;
                        }
                        case 1: {
                            System.Diagnostics.Debug.Assert(parameters[0].ParameterType == typeof(MouseInputEvent));
                            handlerType = ReflectionUtil.CreateGenericType(typeof(DragEventCreator_WithEvent<>), targetType);
                            break;
                        }
                        default:
                            throw new Exception("Method with attribute " + attr.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(MouseInputEvent));
                    }

                    DragEventCreator creator = (DragEventCreator) ReflectionUtil.CreateGenericInstance(handlerType, ReflectionUtil.ObjectArray3);
#if DEBUG
                    creator.methodInfo = info;
#endif

                    if (retn == null) {
                        retn = new List<DragEventCreator>();
                    }

                    retn.Add(creator);
                }
            }

            s_DragCreatorCache[targetType] = retn;
            return retn;
        }

        private static readonly ValueTuple<string, Type> s_MouseEventAlias = ValueTuple.Create("$event", typeof(MouseInputEvent));
        private static readonly ValueTuple<string, Type> s_KeyboardEventAlias = ValueTuple.Create("$event", typeof(KeyboardInputEvent));
        private static readonly ValueTuple<string, Type> s_FocusEventAlias = ValueTuple.Create("$event", typeof(FocusEvent));

        private static readonly InputAttributeTuple[] s_MouseAttributeDefs = {
            new InputAttributeTuple("onMouseEnter", InputEventType.MouseEnter, s_MouseEventAlias),
            new InputAttributeTuple("onMouseExit", InputEventType.MouseExit, s_MouseEventAlias),
            new InputAttributeTuple("onMouseClick", InputEventType.MouseClick, s_MouseEventAlias),
            new InputAttributeTuple("onMouseDown", InputEventType.MouseDown, s_MouseEventAlias),
            new InputAttributeTuple("onMouseUp", InputEventType.MouseUp, s_MouseEventAlias),
            new InputAttributeTuple("onMouseMove", InputEventType.MouseMove, s_MouseEventAlias),
            new InputAttributeTuple("onMouseHover", InputEventType.MouseHover, s_MouseEventAlias),
            new InputAttributeTuple("onMouseScroll", InputEventType.MouseScroll, s_MouseEventAlias),
            new InputAttributeTuple("onMouseContext", InputEventType.MouseContext, s_MouseEventAlias),
        };

        private static readonly InputAttributeTuple[] s_DragAttributeDefs = {
            new InputAttributeTuple("onDragMove", InputEventType.DragMove, s_MouseEventAlias),
            new InputAttributeTuple("onDragHover", InputEventType.DragHover, s_MouseEventAlias),
            new InputAttributeTuple("onDragEnter", InputEventType.DragEnter, s_MouseEventAlias),
            new InputAttributeTuple("onDragExit", InputEventType.DragExit, s_MouseEventAlias),
            new InputAttributeTuple("onDragDrop", InputEventType.DragDrop, s_MouseEventAlias),
            new InputAttributeTuple("onDragCancel", InputEventType.DragCancel, s_MouseEventAlias),
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

        private static List<T> Combine<T>(List<T> a, List<T> b) {
            if (a == null) {
                return b;
            }

            if (b == null) {
                return a;
            }

            a.AddRange(b);
            return a;
        }

        public void SetCompiler(ExpressionCompiler templateCompiler) {
            this.compiler = templateCompiler;
        }

    }

}