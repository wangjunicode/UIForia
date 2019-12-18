using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Templates;
using UIForia.Util;
using UnityEditor;

namespace UIForia {

    public class CompiledTemplateData {

        public LightList<CompiledTemplate> compiledTemplates;
        public LightList<CompiledSlot> compiledSlots;
        public LightList<CompiledBinding> compiledBindings;
        public StyleSheetImporter styleImporter;
        public Func<int, UIElement> constructElement;

        public TemplateMetaData[] templateMetaData;
        public Func<UIElement, TemplateScope, UIElement>[] templates;
        public Func<UIElement, TemplateScope, UIElement>[] slots;
        public Action<UIElement, UIElement>[] bindings;
        public UIStyleGroupContainer[] styles;
        
        public readonly Dictionary<Type, int> templateTypeMap = new Dictionary<Type, int>();
        public TemplateSettings templateSettings;

//        public abstract void LoadTemplates();
        
        public CompiledTemplateData(TemplateSettings templateSettings) {
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
            compiledTemplate.templateMetaData = new TemplateMetaData(compiledTemplate.templateId, filePath, null, null);
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
            return styleImporter.Import(styleDefinition, true);
        }

        public Func<UIElement, TemplateScope, UIElement> GetTemplate<T>() where T : UIElement {
            
            if (templateTypeMap.TryGetValue(typeof(T), out int id)) {
                return templates[id];
            }

            return null;
        }

        public UIElement ConstructElement(int typeId) {
            return constructElement.Invoke(typeId);
        }

    }

}