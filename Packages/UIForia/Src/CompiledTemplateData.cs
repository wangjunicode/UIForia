using System;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Templates;
using UIForia.Util;
using UnityEditor;

namespace UIForia {

    public abstract class CompiledTemplateData {

        protected LightList<CompiledTemplate> compiledTemplates;
        protected LightList<CompiledSlot> compiledSlots;
        protected LightList<CompiledBinding> compiledBindings;
        protected StyleSheetImporter styleImporter;

        public TemplateMetaData[] templateMetaData;
        public Func<UIElement, TemplateScope2, UIElement>[] templates;
        public Func<UIElement, TemplateScope2, UIElement>[] slots;
        public Action<UIElement, UIElement>[] bindings;
        public UIStyleGroupContainer[] styles;
        
        public readonly TemplateSettings templateSettings;
        
        public abstract void LoadTemplates();
        
        protected CompiledTemplateData(TemplateSettings templateSettings) {
            this.templateSettings = templateSettings;
            this.compiledSlots = new LightList<CompiledSlot>();
            this.compiledTemplates = new LightList<CompiledTemplate>(128);
            this.compiledBindings = new LightList<CompiledBinding>(128);
            this.styleImporter = new StyleSheetImporter(templateSettings.templateResolutionBasePath);
        }

        public CompiledTemplate CreateTemplate(string filePath) {
            CompiledTemplate compiledTemplate = new CompiledTemplate();
            compiledTemplate.filePath = filePath;
            compiledTemplate.guid = GUID.Generate();
            compiledTemplate.templateId = compiledTemplates.size;
            compiledTemplates.Add(compiledTemplate);
            compiledTemplate.templateMetaData = new TemplateMetaData(compiledTemplate.templateId, filePath, null);
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
        
        public StyleSheet ImportStyleSheet(in StyleDefinition styleDefinition) {
            return styleImporter.Import(styleDefinition);
        }

    }

}