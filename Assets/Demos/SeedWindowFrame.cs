using System;
using System.Collections.Generic;
using Src;
using Src.Animation;
using Src.Rendering;
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
    
    [Template("Demos/SeedWindowFrame.xml")]
    public class SeedWindowFrame : UIElement {

        public event Action<ITabMenuEntry> onMenuChanged;
        public List<ISideBarEntry> sideBarEntries = new List<ISideBarEntry>();
        public List<ITabMenuEntry> tabs = new List<ITabMenuEntry>();

        private int selectedMenuEntry;
        private UIElement runner;
        
        public override void OnCreate() {
            sideBarEntries.Add(new SidebarThing("Matt " + 1));
            sideBarEntries.Add(new SidebarThing("Matt " + 2));
            sideBarEntries.Add(new SidebarThing2("Matt " + 3));
            sideBarEntries.Add(new SidebarThing2("Matt " + 4));
            tabs.Add(new TabThing("ALPHABETICAL"));
            tabs.Add(new TabThing("BY CLASS"));
            tabs.Add(new TabThing("BY CONTAINER"));
            runner = FindById("tab-runner");
        }

        public override void OnReady() {
            runner.style.SetTransformPositionX(0, StyleState.Normal);
            runner.style.SetPreferredWidth(100f, StyleState.Normal);
            
        }

        public void MenuItem_MouseDown(int index) {
            if (selectedMenuEntry == index) {
                return;
            }

            selectedMenuEntry = index;
            onMenuChanged?.Invoke(tabs[selectedMenuEntry]);
            UIElement tabContainer = FindById("tab-container");
            
            runner.style.PlayAnimation(RunnerAnimation(tabContainer.ownChildren[0].ownChildren[index]));
        }

        private StyleAnimation RunnerAnimation(UIElement target) {
            AnimationOptions options = new AnimationOptions();

            options.duration = 1;
            options.iterations = 1;
            
            UIMeasurement width = new UIMeasurement(target.layoutResult.ActualWidth);
            float x = target.layoutResult.localPosition.x;
            StyleProperty targetWidth = new StyleProperty(StylePropertyId.PreferredWidth, width);
            StyleProperty targetX = new StyleProperty(StylePropertyId.TransformPositionX, x);
            
            return new AnimationGroup( 
                new PropertyAnimation(targetX, options),
                new PropertyAnimation(targetWidth, options)
            );
            
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