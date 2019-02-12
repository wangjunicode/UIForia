using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Routing;
using UIForia.Util;
using UnityEngine;

namespace UI {

    [Template(BasePath + "Root.xml")]
    public class Root : UIElement {

        public const string BasePath = "Klang/Seed/UIForia/";

        public override void OnCreate() {
            Application.Router.GoTo("/game");
        }

        public void StartGame() {
//            SeedDebugWindow window1 = CreateChild<SeedDebugWindow>();
        }

        public void RouteTransitions() {
//            
//            Router.Find("game").Transition("/game/userview/:id", "*", (float elapsed) => RouteTransitionState.Completed);
//
//            Router.Find("game").Transition("/splash", "*", (float elapsed) => {
//                if (elapsed > 500f) {
//                    return RouteTransitionState.Completed;
//                }
//
//                return RouteTransitionState.Pending;
//            });
        }

    }

}