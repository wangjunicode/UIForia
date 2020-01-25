using System;
using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/Documentation.style",
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/Features/AnimationDemo.style",
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Elements/InputElement.xml.style",

        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[3];
            templates[0] = Template_76c28ddd16392524a91a6698ec1d038e; // Documentation/Features/AnimationDemo.xml
            templates[1] = Template_a96bf8a5a021ff544a3f52e43c4343f5; // Elements/InputElement.xml
            templates[2] = Template_8b7861935f17af54498cbc103e1945b1; // Elements/InputElement.xml
            return templates;

        }

        public TemplateMetaData[] LoadTemplateMetaData(Dictionary<string, StyleSheet> sheetMap, UIStyleGroupContainer[] styleMap) {
            TemplateMetaData[] templateData = new TemplateMetaData[3];
            TemplateMetaData template;
            StyleSheetReference[] styleSheetRefs;
            styleSheetRefs = new StyleSheetReference[1];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Documentation/Features/AnimationDemo.style"]);
            template = new TemplateMetaData(0, @"Documentation/Features/AnimationDemo.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[0] = template;
            styleSheetRefs = new StyleSheetReference[1];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Elements/InputElement.xml.style"]);
            template = new TemplateMetaData(1, @"Elements/InputElement.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[1] = template;
            styleSheetRefs = new StyleSheetReference[1];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Elements/InputElement.xml.style"]);
            template = new TemplateMetaData(2, @"Elements/InputElement.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[2] = template;
            return templateData;

        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[67];
            bindings[0] = Binding_OnUpdate_eee71d8bffc06d244b146449e397df4c;
            bindings[1] = Binding_OnCreate_8d93bae2bc6021a43bfb5072e94ab71b;
            bindings[2] = Binding_OnCreate_593d59cde129d154db475d26c4d97aa8;
            bindings[3] = Binding_OnCreate_38d0613b7c70a2c488ba8bb6782fa2da;
            bindings[4] = Binding_OnCreate_70042549899bca041bc06e7fee664909;
            bindings[5] = Binding_OnCreate_4e2b5b56f9edf24408c626f63f8d9909;
            bindings[6] = Binding_OnCreate_f2ec6bc3a7309284f97fb1952bfb3f85;
            bindings[7] = Binding_OnCreate_70586a026ba3c8f4198f421e30950a5a;
            bindings[8] = Binding_OnCreate_dfbfc16ff1e80324396dba99e5ecff93;
            bindings[9] = Binding_OnCreate_f6dbbd1a8ba59914b90359037924180e;
            bindings[10] = Binding_OnCreate_9e61b7088876b894a863ca0ad4ab6b87;
            bindings[11] = Binding_OnCreate_9b53a14527daf7c419b39cfaa5d1f3c8;
            bindings[12] = Binding_OnCreate_6c9074d60683abb4685b7105537b60b4;
            bindings[13] = Binding_OnCreate_a32d2f7605168514084014c577e95b17;
            bindings[14] = Binding_OnCreate_f7e8c9e82d474c545a87c46a76cb558a;
            bindings[15] = Binding_OnCreate_1d0222c6a82ba3a4797ef16bd56a9e88;
            bindings[16] = Binding_OnCreate_e0adaa0abe7977449bb7e535e04c3eb6;
            bindings[17] = Binding_OnCreate_46bbaa32e544fec44be79ae43ef82af5;
            bindings[18] = Binding_OnCreate_3b676e09a951fa6479c1ae03b2a03b58;
            bindings[19] = Binding_OnCreate_443b1b8d7598c394790db388c410c9c4;
            bindings[20] = Binding_OnCreate_03e0df23b15e9814cbfc4389c115bcde;
            bindings[21] = Binding_OnCreate_c465acc15369aff43a2cd2a32a6a556a;
            bindings[22] = Binding_OnCreate_059513629b460ea41a928fbff6142c4b;
            bindings[23] = Binding_OnCreate_8ea117ce17d3fb84fb4fbe85d34c552e;
            bindings[24] = Binding_OnCreate_e3c611adad2945e489952bd31aba6c73;
            bindings[25] = Binding_OnCreate_e961987bc9835124b9902f7b17ff8616;
            bindings[26] = Binding_OnCreate_c2a835d045416704b86f1d7f342ac76d;
            bindings[27] = Binding_OnCreate_25b2f28bba2b9e54facab983f0f39abf;
            bindings[28] = Binding_OnCreate_0183e9c25fb6a68478e84dcaa62312bb;
            bindings[29] = Binding_OnCreate_c823c299188799a45b7136b2589c9d83;
            bindings[30] = Binding_OnCreate_f5ab2921368b7734dafbb859f92e755d;
            bindings[31] = Binding_OnCreate_26bed92de49bd604ea8620bfdc4902ff;
            bindings[32] = Binding_OnCreate_7ff9a0e9b9d94b54e9ce85eb8270e1e8;
            bindings[33] = Binding_OnCreate_40cf342e2fcfb8040be2bbde069f129f;
            bindings[34] = Binding_OnCreate_f6d79bad9c786a14a8d5709815a4813b;
            bindings[35] = Binding_OnCreate_0720f7835c8de1b4d91157a44d3fd177;
            bindings[36] = Binding_OnCreate_a9e158a3e629df74fb56b11380025e4d;
            bindings[37] = Binding_OnCreate_eee4543966b84dd41b98e5e321d0dced;
            bindings[38] = Binding_OnCreate_a51596fc34c57ff4fb18204c2ea5723c;
            bindings[39] = Binding_OnUpdate_aa94628d935a3224eb9180379fa89255;
            bindings[40] = Binding_OnCreate_0e21de367cc2b8c4e92a5eb8a952120f;
            bindings[41] = Binding_OnUpdate_c13e4263a85aa4842a8ca6b5d7acacd4;
            bindings[42] = Binding_OnCreate_26b5f1d3945844145b4da1077a0077fe;
            bindings[43] = Binding_OnUpdate_6bd8f714b1b927743a3a4835debff7d7;
            bindings[44] = Binding_OnUpdate_60c411e3d38c0f643a7c969603a00ee2;
            bindings[45] = Binding_OnUpdate_c69ef37abc897244382f18a2b748f926;
            bindings[46] = Binding_OnLateUpdate_afd9e3d721f798e42ab4660eb41fb500;
            bindings[47] = Binding_OnCreate_59537bc48d47e5c4e8dd2fd647327ca9;
            bindings[48] = Binding_OnUpdate_82131eda5e6737347bd1a6c3a9915152;
            bindings[49] = Binding_OnLateUpdate_7ede0fca07fbe504d916a0104d7e47b9;
            bindings[50] = Binding_OnCreate_c7fd48d105c380b43a5221a4ff1f9991;
            bindings[51] = Binding_OnUpdate_13b9cac6703e52d48a39152c815da900;
            bindings[52] = Binding_OnUpdate_7403f7cddc1dadb4d98603b5551dc689;
            bindings[53] = Binding_OnUpdate_ca0216731e067ba4f8ac5ba7aaab5579;
            bindings[54] = Binding_OnLateUpdate_d9d387c647aa3714bb43d6481bc28d51;
            bindings[55] = Binding_OnCreate_acfc25e1d1bb1364c8041785c69d996d;
            bindings[56] = Binding_OnCreate_4ba1090ff92644e49b62a8238eb878a5;
            bindings[57] = Binding_OnUpdate_34f430a69cf60294b935ad097853e92f;
            bindings[58] = Binding_OnCreate_3a3b6876f8325fc41aaec2cddda10167;
            bindings[59] = Binding_OnUpdate_71b9cffeb472bf549b96b35ce33e672f;
            bindings[60] = Binding_OnCreate_64a384c059f2ef144a83671f93bfd6e3;
            bindings[61] = Binding_OnUpdate_16137b3ed10efca4abc1c83dde6cc3f8;
            bindings[62] = Binding_OnCreate_edfad68de7637e44c8c9ea067ae66fd0;
            bindings[63] = Binding_OnUpdate_87627d7b68fbc96439ac3d4d2a3c3ae3;
            bindings[64] = Binding_OnCreate_7b8f584215096c649b422f69937c1e87;
            bindings[65] = Binding_OnUpdate_38065ee9967aca047914fe7fd884a1b1;
            bindings[66] = Binding_OnUpdate_2bded98b79e10a8498643ed8c91be99f;
            return bindings;

        }

        public Func<UIElement, UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, UIElement, TemplateScope, UIElement>[2];
            slots[0] = Slot_Default_0_e788ac9467bc2224cacdcbd68e444c35;
            slots[1] = Slot_Default_1_5bcb89aac0b3d834fa4b502fff73dfde;
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 12:
                    return new Documentation.Features.AnimationDemo();
                case 50:
                    return new UIForia.Elements.UIGroupElement();
                case 52:
                    return new UIForia.Elements.UISectionElement();
                case 53:
                    return new UIForia.Elements.UIDivElement();
                case 56:
                    return new UIForia.Elements.UILabelElement();
                case 57:
                    return new UIForia.Elements.UIParagraphElement();
                case 59:
                    return new UIForia.Elements.UIHeading2Element();
                case 83:
                    return new UIForia.Elements.UISlotOverride();
                case 85:
                    return new UIForia.Elements.UITextElement();
                case 235:
                    return new UIForia.Elements.InputElement<float>();
                case 236:
                    return new UIForia.Elements.InputElement<int>();

            }
            return null;
        }

    }

}