// using UIForia.Elements;
// using UIForia.Util;
//
// namespace UIForia.Compilers {
//
//     public struct TemplateScope2 {
//
//         public readonly UIElement rootRef;
//         public readonly ElementSystem system;
//         public readonly TemplateData templateData;
//         public SizedArray<SlotOverride> slotOverrides;
//         
//         public TemplateScope2(ElementSystem system, TemplateData data, UIElement rootRef, SizedArray<SlotOverride> slotOverrides) {
//             this.system = system;
//             this.templateData = data;
//             this.rootRef = rootRef;
//             this.slotOverrides = slotOverrides;
//         }
//
//         public void OverrideSlot(UIElement element, string slotName, int slotIndex) {
//             element.bindingNode.referencedContexts[0] = element;
//             slotOverrides.Add(new SlotOverride(slotName, templateData.slots[slotIndex]));
//         }
//
//         public void ForwardSlot(UIElement element, string slotName, int slotIndex) {
//             // setup context reference
//             for(int i = 0; i < element.bindingNode.referencedContexts.Length; i++) {
//                 if(element.bindingNode.referencedContexts[i] == null) {
//                     element.bindingNode.referencedContexts[i] = element;
//                     break;
//                 }
//             }
//
//             if (slotIndex == -1) return;
//             
//             for (int i = 0; i < slotOverrides.size; i++) {
//                 if (slotOverrides[i].slotName == slotName) {
//                     return;
//                 }
//             }
//
//             slotOverrides.Add(new SlotOverride(slotName, templateData.slots[slotIndex]));
//             
//         }
//
//     }
//
// }