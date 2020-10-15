using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace SeedLib {

    [Template("SeedLib/SeedLibRoot.xml")]
    public class SeedLibRoot : UIElement {

        public Action click => Click;
        public Action actionClick => ActionClicked;
        public Action toggleLeft => ToggleLeft;
        public Action toggleMiddle => ToggleMiddle;
        public Action toggleRight => ToggleRight;

        #region ListSample
        
        public struct ListItemData {
            public ImageLocator Icon;
            public string Name;
        }
        
        public List<ListItemData> items = new List<ListItemData>();
        
        #endregion //ListSample

        #region SelectSample

        public List<ISelectOption<string>> activities = new List<ISelectOption<string>>() {
                new SelectOption<string>("Sleep", "sleep"), 
                new SelectOption<string>("Work", "work"), 
                new SelectOption<string>("Recreation", "recreation")
        };
        
        #endregion //SelectSample

        public override void OnEnable() {
            items.Add(new ListItemData() {Icon = "Icons/success", Name = "Meat"});            
            items.Add(new ListItemData() {Icon = "Icons/success", Name = "Tomato"});
            items.Add(new ListItemData() {Icon = "Icons/success", Name = "Carrot"});
            items.Add(new ListItemData() {Icon = "Icons/success", Name = "Headers"});
        }

        public void Click() {
            Debug.Log("clicked");
        }

        public void ActionClicked() {
            Debug.Log("action-clicked");
        }

        public void ToggleLeft() {
            Debug.Log("Toggled Left");
        }
        
        public void ToggleMiddle() {
            Debug.Log("Toggled Middle");
        }
        
        public void ToggleRight() {
            Debug.Log("Toggled Right");
        }

        public void OnNumberValueChanged(float newValue) {
            Debug.Log("OnNumberValueChanged:" + newValue);
        }
    }
    
    public static class ThemeColors {

        public static Color coal0 = ColorUtil.ColorFromRGBHash(0xffffff);
        public static Color coal50 = ColorUtil.ColorFromRGBHash(0xeeeeee);
        public static Color coal100 = ColorUtil.ColorFromRGBHash(0xdddddd);
        public static Color coal150 = ColorUtil.ColorFromRGBHash(0xcccccc);
        public static Color coal200 = ColorUtil.ColorFromRGBHash(0xbbbbbb);
        public static Color coal250 = ColorUtil.ColorFromRGBHash(0xaaaaaa);
        public static Color coal300 = ColorUtil.ColorFromRGBHash(0x999999);
        public static Color coal400 = ColorUtil.ColorFromRGBHash(0x888888);
        public static Color coal500 = ColorUtil.ColorFromRGBHash(0x777777);
        public static Color coal600 = ColorUtil.ColorFromRGBHash(0x666666);
        public static Color coal700 = ColorUtil.ColorFromRGBHash(0x555555);
        public static Color coal800 = ColorUtil.ColorFromRGBHash(0x444444);
        public static Color coal850 = ColorUtil.ColorFromRGBHash(0x333333);
        public static Color coal900 = ColorUtil.ColorFromRGBHash(0x222222);
        public static Color coal950 = ColorUtil.ColorFromRGBHash(0x111111);
        public static Color coal1000 = ColorUtil.ColorFromRGBHash(0x000000);

        public static Color chili100 = ColorUtil.ColorFromRGBHash(0xffcccc);
        public static Color chili300 = ColorUtil.ColorFromRGBHash(0xff9898);
        public static Color chili500 = ColorUtil.ColorFromRGBHash(0xff5454);
        public static Color chili700 = ColorUtil.ColorFromRGBHash(0x992828);
        public static Color chili900 = ColorUtil.ColorFromRGBHash(0x5a1212);

        public static Color carrot100 = ColorUtil.ColorFromRGBHash(0xffd3c7);
        public static Color carrot300 = ColorUtil.ColorFromRGBHash(0xffa68e);
        public static Color carrot500 = ColorUtil.ColorFromRGBHash(0xff6b43);
        public static Color carrot700 = ColorUtil.ColorFromRGBHash(0x993a20);
        public static Color carrot900 = ColorUtil.ColorFromRGBHash(0x571e0e);

        public static Color pumpkin100 = ColorUtil.ColorFromRGBHash(0xffd9c2);
        public static Color pumpkin300 = ColorUtil.ColorFromRGBHash(0xffb385);
        public static Color pumpkin500 = ColorUtil.ColorFromRGBHash(0xff8133);
        public static Color pumpkin700 = ColorUtil.ColorFromRGBHash(0x904517);
        public static Color pumpkin900 = ColorUtil.ColorFromRGBHash(0x57270a);

        public static Color apricot100 = ColorUtil.ColorFromRGBHash(0xffe3be);
        public static Color apricot300 = ColorUtil.ColorFromRGBHash(0xffc67d);
        public static Color apricot500 = ColorUtil.ColorFromRGBHash(0xffa026);
        public static Color apricot700 = ColorUtil.ColorFromRGBHash(0x8a5410);
        public static Color apricot900 = ColorUtil.ColorFromRGBHash(0x4c2e07);

        public static Color lemon100 = ColorUtil.ColorFromRGBHash(0xffecbb);
        public static Color lemon300 = ColorUtil.ColorFromRGBHash(0xffd876);
        public static Color lemon500 = ColorUtil.ColorFromRGBHash(0xffbe1a);
        public static Color lemon700 = ColorUtil.ColorFromRGBHash(0x87640c);
        public static Color lemon900 = ColorUtil.ColorFromRGBHash(0x4c3805);

        public static Color wasabi100 = ColorUtil.ColorFromRGBHash(0xecedc1);
        public static Color wasabi300 = ColorUtil.ColorFromRGBHash(0xd9db83);
        public static Color wasabi500 = ColorUtil.ColorFromRGBHash(0xc0c330);
        public static Color wasabi700 = ColorUtil.ColorFromRGBHash(0x666815);
        public static Color wasabi900 = ColorUtil.ColorFromRGBHash(0x393a08);

        public static Color lime100 = ColorUtil.ColorFromRGBHash(0xd9efc8);
        public static Color lime300 = ColorUtil.ColorFromRGBHash(0xb3de91);
        public static Color lime500 = ColorUtil.ColorFromRGBHash(0x80c847);
        public static Color lime700 = ColorUtil.ColorFromRGBHash(0x416c1f);
        public static Color lime900 = ColorUtil.ColorFromRGBHash(0x213c0d);

        public static Color apple100 = ColorUtil.ColorFromRGBHash(0xb4f1d6);
        public static Color apple300 = ColorUtil.ColorFromRGBHash(0x67e3ac);
        public static Color apple500 = ColorUtil.ColorFromRGBHash(0x02d174);
        public static Color apple700 = ColorUtil.ColorFromRGBHash(0x016e3e);
        public static Color apple900 = ColorUtil.ColorFromRGBHash(0x013e23);

        public static Color mint100 = ColorUtil.ColorFromRGBHash(0xb3ede5);
        public static Color mint300 = ColorUtil.ColorFromRGBHash(0x67dbca);
        public static Color mint500 = ColorUtil.ColorFromRGBHash(0x01c3a7);
        public static Color mint700 = ColorUtil.ColorFromRGBHash(0x016858);
        public static Color mint900 = ColorUtil.ColorFromRGBHash(0x003a32);

        public static Color poppy100 = ColorUtil.ColorFromRGBHash(0xb3e9f4);
        public static Color poppy300 = ColorUtil.ColorFromRGBHash(0x66d3e8);
        public static Color poppy500 = ColorUtil.ColorFromRGBHash(0x00b5d9);
        public static Color poppy700 = ColorUtil.ColorFromRGBHash(0x006073);
        public static Color poppy900 = ColorUtil.ColorFromRGBHash(0x003641);

        public static Color lupine100 = ColorUtil.ColorFromRGBHash(0xcadff6);
        public static Color lupine300 = ColorUtil.ColorFromRGBHash(0x94bfec);
        public static Color lupine500 = ColorUtil.ColorFromRGBHash(0x4c94e0);
        public static Color lupine700 = ColorUtil.ColorFromRGBHash(0x255486);
        public static Color lupine900 = ColorUtil.ColorFromRGBHash(0x102c4c);

        public static Color iris100 = ColorUtil.ColorFromRGBHash(0xd5daf7);
        public static Color iris300 = ColorUtil.ColorFromRGBHash(0xaab5ee);
        public static Color iris500 = ColorUtil.ColorFromRGBHash(0x7283e3);
        public static Color iris700 = ColorUtil.ColorFromRGBHash(0x39488f);
        public static Color iris900 = ColorUtil.ColorFromRGBHash(0x1a2358);

        public static Color lavender100 = ColorUtil.ColorFromRGBHash(0xe0d5f8);
        public static Color lavender300 = ColorUtil.ColorFromRGBHash(0xc1abf0);
        public static Color lavender500 = ColorUtil.ColorFromRGBHash(0x9873e6);
        public static Color lavender700 = ColorUtil.ColorFromRGBHash(0x563a91);
        public static Color lavender900 = ColorUtil.ColorFromRGBHash(0x2f1a59);

        public static Color viola100 = ColorUtil.ColorFromRGBHash(0xe8d3ed);
        public static Color viola300 = ColorUtil.ColorFromRGBHash(0xd1a6da);
        public static Color viola500 = ColorUtil.ColorFromRGBHash(0xb26bc2);
        public static Color viola700 = ColorUtil.ColorFromRGBHash(0x71377c);
        public static Color viola900 = ColorUtil.ColorFromRGBHash(0x461a4f);

        public static Color orchid100 = ColorUtil.ColorFromRGBHash(0xf0d1e2);
        public static Color orchid300 = ColorUtil.ColorFromRGBHash(0xe0a1c4);
        public static Color orchid500 = ColorUtil.ColorFromRGBHash(0xcc639d);
        public static Color orchid700 = ColorUtil.ColorFromRGBHash(0x82325f);
        public static Color orchid900 = ColorUtil.ColorFromRGBHash(0x4d1635);

        public static Color rose100 = ColorUtil.ColorFromRGBHash(0xf8ced7);
        public static Color rose300 = ColorUtil.ColorFromRGBHash(0xf09dae);
        public static Color rose500 = ColorUtil.ColorFromRGBHash(0xe65b78);
        public static Color rose700 = ColorUtil.ColorFromRGBHash(0x912e42);
        public static Color rose900 = ColorUtil.ColorFromRGBHash(0x551322);

    }

}