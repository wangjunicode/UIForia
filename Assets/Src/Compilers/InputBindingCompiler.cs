using System;
using System.Collections.Generic;
using Src.Input;
using Src.InputBindings;

namespace Src.Compilers {

    public class InputBindingCompiler {
        
        private ContextDefinition context;
        private readonly ExpressionCompiler compiler;

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