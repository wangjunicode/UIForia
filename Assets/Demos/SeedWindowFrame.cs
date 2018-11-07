using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Demos {

    public interface ISideBarEntry {

        Action onClick { get; }
        string imageUrl { get; }

    }

    public interface ITabMenuEntry {

        Action onClick { get; }
        string text { get; }

    }

    public class ResourceItem {

        public string name;
        public string iconUrl;
        public int count;

        public ResourceItem(string name, string iconUrl, int count) {
            this.name = name;
            this.iconUrl = "ui_icon_inventory_item_" + iconUrl;
            this.count = count;
        }

    }
    public class ResourceGroup {

        public string name;
        public List<ResourceItem> contents;
        public int total;
        
        public ResourceGroup(string name, List<ResourceItem> contents) {
            this.name = name;
            this.contents = contents ?? new List<ResourceItem>();
            for (int i = 0; i < contents.Count; i++) {
                total += contents[i].count;
            }
            
        }

    }

    [Template("Demos/SeedWindowFrame.xml")]
    public class SeedWindowFrame : UIElement {

        public event Action<ITabMenuEntry> onMenuChanged;
        public event Action<ISideBarEntry> onSidebarMenuChanged;

        public List<ITabMenuEntry> tabs = new List<ITabMenuEntry>();
        public List<ISideBarEntry> sideBarEntries = new List<ISideBarEntry>();

        public List<ResourceGroup> groups = new List<ResourceGroup>();
            
        private int selectedMenuEntry;
        private int selectedSideBarEntry;
        private UIElement tabRunner;
        private UIElement sideBarRunner;
        private UIRepeatElement sidebarRepeater;
        private UIRepeatElement tabRepeater;

        public override void OnCreate() {
            sideBarEntries.Add(new SidebarThing("icon_1"));
            sideBarEntries.Add(new SidebarThing("icon_2"));
            sideBarEntries.Add(new SidebarThing2("icon_3"));
            sideBarEntries.Add(new SidebarThing2("icon_4"));

            tabs.Add(new TabThing("ALPHABETICAL"));
            tabs.Add(new TabThing("BY CLASS"));
            tabs.Add(new TabThing("BY CONTAINER"));

            tabRunner = FindById("tab-runner");
            sideBarRunner = FindById("sidebar-runner");

            tabRepeater = FindById<UIRepeatElement>("tab-repeater");
            sidebarRepeater = FindById<UIRepeatElement>("sidebar-repeater");

            tabRepeater.onListPopulated += TabRepeaterOnListPopulated;
            tabRepeater.onListEmptied += TabRepeaterOnListEmptied;
            sidebarRepeater.onListPopulated += SidebarRepeaterOnListPopulated;
            sidebarRepeater.onListEmptied += SidebarRepeaterOnListEmptied;
            
            groups = new List<ResourceGroup>();
            groups.Add(new ResourceGroup("Main Food Storage", new List<ResourceItem>(new ResourceItem[] {
                new ResourceItem("Corn", "corn", 276), 
                new ResourceItem("Food Paste", "food_paste", 102) 
            })));
            groups.Add(new ResourceGroup("Processed Ores", new List<ResourceItem>(new ResourceItem[] {
                new ResourceItem("Iron Ore", "iron_ore", 108), 
                new ResourceItem("Silver Ore", "silver_ore", 64), 
                new ResourceItem("Titanium Ore", "titanium_ore", 130), 
                new ResourceItem("Uranium Ore", "uranium_ore", 35) 
            })));
        }

        public void ToggleResourceGroup(int index) {
                        
        }
        
        public void SidebarRepeaterOnListPopulated() {
            
            sideBarRunner.SetEnabled(true);
            selectedMenuEntry = Mathf.Clamp(selectedMenuEntry, 0, sidebarRepeater.ChildCount);
            
            UIElement target = sidebarRepeater.GetChild(selectedSideBarEntry);
            float y = target.layoutResult.localPosition.y + 10f;
            sideBarRunner.style.SetTransformPositionY(y, StyleState.Normal);
        }

        private void SidebarRepeaterOnListEmptied() {
            sideBarRunner.SetEnabled(false);
        }

        private void TabRepeaterOnListPopulated() {
            tabRunner.SetEnabled(true);

            UIElement target = tabRepeater.GetChild(selectedMenuEntry);
            UIElement child = target.GetChild(0);
            float x = target.layoutResult.localPosition.x + child.layoutResult.ContentRect.x;

            tabRunner.style.SetTransformPositionX(x, StyleState.Normal);
            tabRunner.style.SetPreferredWidth(child.layoutResult.ContentWidth, StyleState.Normal);
        }

        private void TabRepeaterOnListEmptied() {
            tabRunner.SetEnabled(false);
        }

        public void MenuItem_MouseDown(int index) {
            if (selectedMenuEntry == index) {
                return;
            }

            selectedMenuEntry = index;
            onMenuChanged?.Invoke(tabs[selectedMenuEntry]);
            tabRunner.style.PlayAnimation(TabRunnerAnimation(tabRepeater.GetChild(index)));
        }

        public void Sidebar_MouseDown(int index) {
            if (selectedSideBarEntry == index) {
                return;
            }

            selectedSideBarEntry = index;
            onSidebarMenuChanged?.Invoke(sideBarEntries[selectedSideBarEntry]);
            sideBarRunner.style.PlayAnimation(SidebarRunnerAnimation(sidebarRepeater.GetChild(index)));
        }

        private static StyleAnimation TabRunnerAnimation(UIElement target) {
            AnimationOptions options = new AnimationOptions();

            options.duration = 0.3f;
            options.timingFunction = EasingFunction.CubicEaseIn;

            UIElement child = target.GetChild(0);
            UIMeasurement width = new UIMeasurement(child.layoutResult.ContentWidth);

            float x = target.layoutResult.localPosition.x + child.layoutResult.ContentRect.x;

            StyleProperty targetWidth = new StyleProperty(StylePropertyId.PreferredWidth, width);
            StyleProperty targetX = new StyleProperty(StylePropertyId.TransformPositionX, new UIFixedLength(x));

            return new AnimationGroup(
                new PropertyAnimation(targetX, options),
                new PropertyAnimation(targetWidth, options)
            );
        }

        private static StyleAnimation SidebarRunnerAnimation(UIElement target) {
            AnimationOptions options = new AnimationOptions();

            options.duration = 0.3f;
            options.timingFunction = EasingFunction.CubicEaseIn;

            float y = target.layoutResult.localPosition.y + 10f;

            StyleProperty targetY = new StyleProperty(StylePropertyId.TransformPositionY, new UIFixedLength(y));

            return new PropertyAnimation(targetY, options);
        }

    }

    public class TabThing : ITabMenuEntry {

        public Action onClick { get; }
        public string text { get; }

        public TabThing(string title) {
            this.text = title;
        }

    }

    public class SidebarThing : ISideBarEntry {

        public Action onClick { get; }
        public string imageUrl { get; }

        public SidebarThing(string imageUrl) {
            this.imageUrl = imageUrl;
            onClick = () => { UnityEngine.Debug.Log(imageUrl); };
        }

    }

    public class SidebarThing2 : ISideBarEntry {

        public Action onClick { get; }
        public string imageUrl { get; }

        public SidebarThing2(string imageUrl) {
            this.imageUrl = imageUrl;
            this.onClick = () => { Debug.Log(imageUrl); };
        }

    }

}