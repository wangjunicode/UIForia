using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UI {

    public class BuildMenuItem {

        public string name;
        public RepeatableList<BuildMenuItem> childItems;
        public Action action;

    }


    [Template("Klang/Seed/UIForia/BuildMenu/BuildMenu.xml")]
    public class BuildMenu : UIElement {

        public RepeatableList<BuildMenuItem> buildMenuItems;

        private static BuildMenuItem CreateBuildItem(string name, params BuildMenuItem[] childItems) {
            BuildMenuItem item = new BuildMenuItem();
            item.name = name;
            item.childItems = new RepeatableList<BuildMenuItem>(childItems);
            item.action = null;
            return item;
        }

        private static BuildMenuItem CreateBuildItem(string name, Action action) {
            BuildMenuItem item = new BuildMenuItem();
            item.name = name;
            item.childItems = null;
            item.action = action;
            return item;
        }

        private void PlaceCotton() { }

        public void Toggle(int index) {
            UIRepeatElement repeatElement = FindFirstByType<UIRepeatElement>();

//            for (int i = 0; i < repeatElement.children.Length; i++) {
//                UIElement child = repeatElement.children[i];
//                if (i == index) {
//                    child.style.PlayAnimation(OffsetAnimation(-30f));
//                    for (int j = 0; j < child.children.Length; j++) {
//                        child.children[j].style.PlayAnimation(FadeIn(Color.blue, 0.3f + (j * 0.2f)));
//                    }
//                }
//                else {
//                    child.style.PlayAnimation(OffsetAnimation(30f));
//                }
//            }
        }

        private static StyleAnimation FadeIn(Color c, float delay) {
            AnimationOptions options = new AnimationOptions();
            options.duration = 2f;
            options.delay = delay;
            
            return new KeyFrameAnimation(options,
                new AnimationKeyFrame(0, StyleProperty.BackgroundColor(new Color(c.r, c.g, c.b, 0))),
                new AnimationKeyFrame(0.4f, StyleProperty.BackgroundColor(new Color(c.r, c.g, c.b, 0))),
                new AnimationKeyFrame(1f, StyleProperty.BackgroundColor(new Color(c.r, c.g, c.b, 1))
            ));
        }

        private static StyleAnimation OffsetAnimation(float offset) {
            StyleProperty targetY = new StyleProperty(StylePropertyId.TransformPositionY, new UIFixedLength(offset));
            AnimationOptions options = new AnimationOptions();
            options.duration = 0.3f;
            options.timingFunction = EasingFunction.CubicEaseIn;
            return new PropertyAnimation(targetY, options);
        }

        public override void OnCreate() {
            buildMenuItems = new RepeatableList<BuildMenuItem>();

            buildMenuItems.Add(CreateBuildItem("Farming",
                CreateBuildItem("Corn", PlaceCotton),
                CreateBuildItem("Cotton", PlaceCotton),
                CreateBuildItem("Coffee", PlaceCotton)
            ));

            buildMenuItems.Add(CreateBuildItem("Industry",
                CreateBuildItem("Building 1", PlaceCotton),
                CreateBuildItem("Building 2", PlaceCotton),
                CreateBuildItem("Building 3", PlaceCotton)
            ));

//            buildMenuItems.Add(CreateBuildItem("Buildings",
//                CreateBuildItem("Corn", PlaceCotton),
//                CreateBuildItem("Cotton", PlaceCotton),
//                CreateBuildItem("Coffee", PlaceCotton)
//            ));
//            
//            buildMenuItems.Add(CreateBuildItem("Infrastructure",
//                CreateBuildItem("Corn", PlaceCotton),
//                CreateBuildItem("Cotton", PlaceCotton),
//                CreateBuildItem("Coffee", PlaceCotton)
//            ));
        }

    }

}