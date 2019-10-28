using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;
using UnityEditor;

namespace UIForia {

    public abstract class CompiledTemplateData {

        protected LightList<CompiledTemplate> compiledTemplates;
        protected LightList<CompiledSlot> compiledSlots;
        protected LightList<CompiledBinding> compiledBindings;
        
        protected Func<UIElement, TemplateScope2, UIElement>[] templates;
        protected TemplateSettings templateSettings;
        
        protected CompiledTemplateData(TemplateSettings templateSettings) {
            this.templateSettings = templateSettings;
            this.compiledSlots = new LightList<CompiledSlot>();
            this.compiledTemplates = new LightList<CompiledTemplate>(128);
            this.compiledBindings = new LightList<CompiledBinding>(128);
        }

        public CompiledTemplate CreateTemplate(string filePath) {
            CompiledTemplate compiledTemplate = new CompiledTemplate();
            compiledTemplate.filePath = filePath;
            compiledTemplate.guid = GUID.Generate();
            compiledTemplate.templateId = compiledTemplates.size;
            compiledTemplates.Add(compiledTemplate);
            return compiledTemplate;
        }

        public CompiledSlot CreateSlot(string filePath, string slotName, SlotType slotType) {
            CompiledSlot compiledSlot = new CompiledSlot();
            compiledSlot.filePath = filePath;
            compiledSlot.slotName = slotName;
            compiledSlot.slotType = slotType;
            compiledSlot.guid = GUID.Generate();
            compiledSlot.slotId = compiledSlots.size;
            compiledSlots.Add(compiledSlot);
            return compiledSlot;
        }
        
        public CompiledBinding AddBinding(TemplateNode templateNode, CompiledBindingType bindingType) {
            CompiledBinding binding = new CompiledBinding();
            binding.filePath = templateNode.astRoot.fileName;
            binding.bindingType = bindingType;
            binding.elementTag = templateNode.originalString;
            binding.bindingId = compiledBindings.size;
            binding.guid = GUID.Generate().ToString();
            compiledBindings.Add(binding);
            return binding;
        }

    }

}