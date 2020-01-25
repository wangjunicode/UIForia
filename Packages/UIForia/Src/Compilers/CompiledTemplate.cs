using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEditor;

namespace UIForia.Compilers {

    public class CompiledTemplate {

        public GUID guid;
        public int templateId;
        public string filePath;
        public string templateName;
        public LambdaExpression templateFn;
        public TemplateMetaData templateMetaData;
        internal ProcessedType elementType;
        internal AttributeDefinition[] attributes;
        public LightList<CompiledBinding> bindings;
        public LightList<CompiledSlot> slots;

        public void AddBinding(CompiledBinding binding) {
            bindings = bindings ?? new LightList<CompiledBinding>();
            bindings.Add(binding);
        }

        public void AddSlot(CompiledSlot slot) {
            slots = slots ?? new LightList<CompiledSlot>();
            slots.Add(slot);
        }

    }

}